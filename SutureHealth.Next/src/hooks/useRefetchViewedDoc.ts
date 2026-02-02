import { useCallback, useRef } from 'react';
import camelcaseKeys from 'camelcase-keys';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { RequestDocumentType } from '@utils/zodModels';

export default function useRefetchViewedDoc() {
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { setDocumentAt } = useInboxActions();
  const refetchingDocs = useRef(new Map<number, AbortController>());

  const refetch = useCallback(
    (documentId?: number) => {
      const controller = new AbortController();
      const docId = documentId || viewedDocument!.sutureSignRequestId;

      refetchingDocs.current.forEach((controllerRef, key) => {
        if (docId === key) {
          controllerRef.abort();
        }
      });

      refetchingDocs.current.set(docId, controller);

      fetch(`/api/inbox/${docId}`, { signal: controller.signal })
        .then(async (response) => {
          if (response.ok) {
            const doc = await response.json();
            const document: RequestDocumentType = camelcaseKeys(doc, {
              deep: true,
            });

            setDocumentAt({
              id: document.document.sutureSignRequestId,
              document,
            });
          }
        })
        .finally(() => {
          refetchingDocs.current.delete(docId);
        });
    },
    [viewedDocument, setDocumentAt]
  );

  return refetch;
}
