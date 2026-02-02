import { useEffect, useMemo } from 'react';
import { Box } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import debounce from 'lodash.debounce';
import { AnnotationItem, RequestDocumentType } from '@utils/zodModels';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import useAnnotationValidation from '@hooks/useAnnotationValidation';
import ResizableAnnotation from './components/ResizableAnnotation';
import ElementSwitch from './components/ElementSwitch';

type AnnotationEngineProps = {
  documentReqId: number;
  pageNumber: number;
  document: RequestDocumentType;
  width: number;
  height: number;
};

const AnnotationEngine = ({
  documentReqId,
  pageNumber,
  document,
  width,
  height,
}: AnnotationEngineProps) => {
  const { annotations } = document.template;
  const annotationMap = useSelector((state) => state.inbox.documentAnnotations);
  const pageData = useSelector((state) => state.inbox.currentPageData);
  const isAnnotationsUpdating = useSelector(
    (state) => state.inbox.isAnnotationsUpdating
  );
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const pageAnnotations = useMemo(
    () => annotationMap.filter((i) => i.pageNumber === pageNumber),
    [annotationMap, pageNumber]
  );
  const { isAnnotationsChanged, updateAnnotations } = useUpdateAnnotations();
  const { isMovedTo } = useAnnotationValidation();
  const serverHasSignature = useMemo(
    () => annotations.some((i) => i.annotationTypeId === 1),
    [annotations]
  );
  const clientHasSignature = useMemo(
    () => annotationMap.some((i) => i.annotationTypeId === 1),
    [annotationMap]
  );

  const debouncedAnnotationUpdate = useMemo(
    () =>
      debounce(updateAnnotations, 10_000, {
        maxWait: 10_000,
      }),
    [updateAnnotations]
  );

  // it will trigger on the stack animation
  useEffect(() => {
    if (stackAnimation) {
      debouncedAnnotationUpdate.cancel();
    }
  }, [stackAnimation, debouncedAnnotationUpdate]);

  useEffect(() => {
    if (
      pageData.documentReqId === documentReqId &&
      pageData.pageNumber === pageNumber &&
      isAnnotationsChanged &&
      !isMovedTo &&
      serverHasSignature &&
      clientHasSignature
    ) {
      debouncedAnnotationUpdate({
        annotations: annotationMap,
        document,
      });
    }
  }, [
    annotationMap,
    document,
    isAnnotationsChanged,
    debouncedAnnotationUpdate,
    isAnnotationsUpdating,
    pageData.documentReqId,
    pageData.pageNumber,
    documentReqId,
    pageNumber,
    isMovedTo,
  ]);

  useEffect(() => {
    if (isAnnotationsUpdating) {
      debouncedAnnotationUpdate.cancel();
    }
  }, [isAnnotationsUpdating, debouncedAnnotationUpdate]);

  return (
    <Box position="relative" w="100%" h="100%">
      {pageData.documentReqId === documentReqId &&
        pageAnnotations.map((annotation: AnnotationItem) => (
          <ResizableAnnotation
            key={annotation.id}
            annotation={annotation}
            docWidth={width}
            docHeight={height}
          >
            <ElementSwitch annotation={annotation} />
          </ResizableAnnotation>
        ))}
    </Box>
  );
};

export default AnnotationEngine;
