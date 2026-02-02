import { useMemo } from 'react';
import queryString from 'query-string';
import { useSelector } from '@redux/hooks';
import { AnnotationType, AnnotationItem } from '@utils/zodModels';
import isFaceToFaceDoc from '@utils/isFaceToFaceDoc';
import { FilterQuery } from '@containers/Inbox/components/FilterHandler';

const useAnnotationValidation = () => {
  const documentAnnotations = useSelector(
    (state) => state.inbox.documentAnnotations
  );
  const viewedDoc = useSelector((state) => state.inbox.viewedDocument);
  const currentPageData = useSelector((state) => state.inbox.currentPageData);

  const calcRequiredFields = (annotations: AnnotationItem[]) =>
    // AnnotationType.VisibleSignature
    annotations.length === 0
      ? 1
      : annotations.filter(
          (i) => !i.value && i.annotationTypeId === AnnotationType.TextArea
        ).length +
        // at least one signature element should exist
        (annotations.some(
          (i) => i.annotationTypeId === AnnotationType.VisibleSignature
        )
          ? 0
          : 1);

  const clientRequiredFields = useMemo(() => {
    if (
      viewedDoc &&
      currentPageData.documentReqId === viewedDoc.sutureSignRequestId &&
      !isFaceToFaceDoc(viewedDoc)
    ) {
      return calcRequiredFields(documentAnnotations);
    }

    return undefined;
  }, [documentAnnotations, viewedDoc, currentPageData]);

  const isMovedTo = useMemo(() => {
    if (typeof window !== 'undefined' && clientRequiredFields !== undefined) {
      const query = queryString.parse(window.location.search, {
        arrayFormat: 'index',
      }) as Partial<FilterQuery>;

      if (viewedDoc && query.documentProcessStatus !== 'All') {
        if (viewedDoc.isIncomplete && clientRequiredFields === 0) {
          return 'Needs Signature';
        }
        if (!viewedDoc.isIncomplete && clientRequiredFields > 0) {
          return 'Needs Filling Out';
        }
      }
    }

    return null;
  }, [clientRequiredFields, viewedDoc]);

  return {
    clientRequiredFields,
    isMovedTo,
  };
};

export default useAnnotationValidation;
