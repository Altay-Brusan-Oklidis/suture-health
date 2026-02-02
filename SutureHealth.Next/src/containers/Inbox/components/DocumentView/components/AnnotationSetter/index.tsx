import { useEffect, useMemo, useState } from 'react';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';

const AnnotationEngine = () => {
  const [isAnnotationsSynced, setIsAnnotationsSynced] =
    useState<boolean>(false);
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const pageData = useSelector((state) => state.inbox.currentPageData);
  const { updateAnnotationMap } = useInboxActions();

  const doc = useMemo(() => {
    if (
      viewedDocument &&
      documentsObj[viewedDocument.sutureSignRequestId] !== 'loading'
    ) {
      return documentsObj[viewedDocument.sutureSignRequestId];
    }

    return undefined;
  }, [documentsObj, viewedDocument?.sutureSignRequestId]);

  useEffect(() => {
    if (
      viewedDocument &&
      pageData.documentReqId === viewedDocument.sutureSignRequestId &&
      !isAnnotationsSynced
    ) {
      if (doc && doc !== 'loading') {
        updateAnnotationMap({
          documentAnnotations: doc.template.annotations.map((i) => ({
            ...i,
            id: crypto.randomUUID(),
          })),
          annotationsDocumentId: viewedDocument.sutureSignRequestId,
        });
        setIsAnnotationsSynced(true);
      }
    } else if (
      viewedDocument &&
      pageData.documentReqId !== viewedDocument.sutureSignRequestId &&
      isAnnotationsSynced
    ) {
      setIsAnnotationsSynced(false);
    }
  }, [viewedDocument, pageData.documentReqId, updateAnnotationMap, doc]);

  return null;
};

export default AnnotationEngine;
