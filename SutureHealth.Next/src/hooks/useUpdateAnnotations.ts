/* eslint-disable @typescript-eslint/no-unused-expressions */
import { useMemo, useCallback } from 'react';
import { useToast } from '@chakra-ui/react';
import { useChangeDocumentAnnotationsMutation } from '@containers/Inbox/apiReducer';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { SaveIconFA } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import { AnnotationItem, RequestDocumentType } from '@utils/zodModels';
import isFaceToFaceDoc from '@utils/isFaceToFaceDoc';
import useAnnotationValidation from './useAnnotationValidation';

export type AnnotationPage = {
  pageNumber: number;
  base64: string;
  width: number;
  height: number;
  documentReqId: number;
};

interface AnnotationsUpdateArgs {
  annotations: AnnotationItem[];
  document: RequestDocumentType;
  showToast?: boolean;
}

let isAnnotationsUpdating = false;

// converts new annotation type to postable annotation type
export const createPostableAnnotationList = (
  list: AnnotationItem[]
): AnnotationItem[] => list.map(({ id, ...annotation }) => ({ ...annotation }));

export default function useUpdateAnnotations() {
  const { isMovedTo } = useAnnotationValidation();
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const doc = useMemo(
    () => viewedDocument && documentsObj[viewedDocument.sutureSignRequestId],
    [documentsObj, viewedDocument?.sutureSignRequestId]
  );
  const toast = useToast();
  const annotationMap = useSelector((state) => state.inbox.documentAnnotations);
  const annotationsDocumentId = useSelector(
    (state) => state.inbox.annotationsDocumentId
  );
  const [changeAnnotations] = useChangeDocumentAnnotationsMutation();
  const pageData = useSelector((state) => state.inbox.currentPageData);
  const { setDocumentAt, setIsAnnotationsUpdating } = useInboxActions();

  const formattedAnnotations = useMemo(
    () => createPostableAnnotationList(annotationMap),
    [annotationMap]
  );

  const isAnnotationsChanged = useMemo(() => {
    if (
      doc &&
      doc !== 'loading' &&
      pageData.documentReqId === doc.document.sutureSignRequestId &&
      pageData.documentReqId === annotationsDocumentId
    ) {
      if (
        JSON.stringify(formattedAnnotations) ===
        JSON.stringify(doc.template.annotations)
      ) {
        return false;
      }

      return true;
    }

    return false;
  }, [
    annotationsDocumentId,
    doc,
    formattedAnnotations,
    pageData.documentReqId,
  ]);

  const updateAnnotations = useCallback(
    async ({ annotations, document }: AnnotationsUpdateArgs) => {
      const clonedDoc = structuredClone(document);

      try {
        const newAnnotations = createPostableAnnotationList(annotations);

        setIsAnnotationsUpdating(true);
        setDocumentAt({
          id: document.document.sutureSignRequestId,
          document: {
            ...document,
            template: {
              ...document.template,
              annotations: newAnnotations,
            },
          },
        });

        await changeAnnotations({
          documentReqId: document.document.sutureSignRequestId,
          annotations: newAnnotations,
        }).unwrap();
      } catch (e) {
        setDocumentAt({
          id: clonedDoc.document.sutureSignRequestId,
          document: clonedDoc,
        });

        toast({
          title: 'Error saving document',
          status: 'error',
          position: 'bottom-left',
          duration: 2000,
          isClosable: true,
        });
      } finally {
        setIsAnnotationsUpdating(false);
      }
    },
    []
  );

  const syncAnnotations = useCallback(
    async (withPrevToast?: boolean) => {
      if (
        isAnnotationsChanged &&
        viewedDocument &&
        !isFaceToFaceDoc(viewedDocument) &&
        !isAnnotationsUpdating
      ) {
        const clonedDoc = structuredClone(doc);

        try {
          if (doc && doc !== 'loading') {
            setDocumentAt({
              id: doc?.document.sutureSignRequestId,
              document: {
                ...doc,
                template: {
                  ...doc.template,
                  annotations: formattedAnnotations,
                },
              },
            });
          }

          setIsAnnotationsUpdating(true);
          isAnnotationsUpdating = true;
          await changeAnnotations({
            documentReqId: viewedDocument.sutureSignRequestId,
            annotations: formattedAnnotations,
          }).unwrap();

          if (withPrevToast) {
            toast({
              icon: SaveIconFA({ w: '24px', h: '24px' }),
              title: 'Previous Document Saved',
              description: isMovedTo ? `Moved to ${isMovedTo} list` : undefined,
              position: 'bottom-left',
              duration: 1000,
            });
          }
        } catch (e) {
          if (clonedDoc && clonedDoc !== 'loading') {
            setDocumentAt({
              id: clonedDoc.document.sutureSignRequestId,
              document: clonedDoc,
            });
          }

          toast({
            title: withPrevToast
              ? 'ERROR: Previous Document Save Failed'
              : 'Error saving document',
            status: 'error',
            position: 'bottom-left',
            duration: 2000,
            isClosable: true,
          });
        } finally {
          setTimeout(() => {
            isAnnotationsUpdating = false;
          }, 1500); // sometimes ItemWrapper can call it twice in the onChange prop
          setIsAnnotationsUpdating(false);
        }
      }
    },
    [
      annotationMap,
      changeAnnotations,
      formattedAnnotations,
      isAnnotationsChanged,
      toast,
      viewedDocument,
      doc,
    ]
  );

  return { isAnnotationsChanged, syncAnnotations, updateAnnotations };
}
