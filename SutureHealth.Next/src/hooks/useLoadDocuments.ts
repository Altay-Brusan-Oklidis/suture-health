import { useEffect, useRef } from 'react';
import camelcaseKeys from 'camelcase-keys';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';
import useErrorToast from './useErrorToast';
import useViewedDocIndex from './useViewedDocIndex';

const maxAreaBounds = 6;

export default function useLoadDocuments() {
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const documents = useSelector((state) => state.inbox.documents);
  const index = useViewedDocIndex();
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { setDocumentsObj, setDocumentAt, setViewedDocument } =
    useInboxActions();
  const fetchingDocs = useRef(new Map<number, AbortController>());
  const showErrorToast = useErrorToast();

  const fetchDocument = (requestId: number) => {
    if (requestId) {
      const controller = new AbortController();

      fetchingDocs.current.set(requestId, controller);

      fetch(`/api/inbox/${requestId}`, { signal: controller.signal })
        .then(async (response) => {
          if (response.ok) {
            const doc = await response.json();

            setDocumentAt({
              id: requestId,
              document: camelcaseKeys(doc, {
                deep: true,
              }),
            });

            return doc;
          }

          return Promise.reject(response);
        })
        .catch((e) => {
          showErrorToast(e);
        })
        .finally(() => {
          fetchingDocs.current.delete(requestId);
        });
    }
  };

  useEffect(() => {
    if (fetchingDocs.current.size && documents && documents.length) {
      const startIndexRange = Math.max(index - maxAreaBounds, 0);
      const endIndexRange = index + maxAreaBounds;
      const ids = documents
        .slice(startIndexRange, endIndexRange)
        .map((i) => i.sutureSignRequestId);

      fetchingDocs.current.forEach((controller, key) => {
        if (!ids.includes(key)) {
          controller.abort();
        }
      });
    }
  }, [fetchingDocs, documents, index]);

  useEffect(() => {
    if (documents) {
      const currentDoc = documents.find(
        (i) => i.sutureSignRequestId === viewedDocument?.sutureSignRequestId
      );

      if (currentDoc) {
        setViewedDocument(currentDoc);
      }

      setDocumentsObj(
        documents.reduce(
          (acc, cur) => ({
            ...acc,
            [cur.sutureSignRequestId]:
              documentsObj[cur.sutureSignRequestId] || 'loading',
          }),
          {}
        )
      );
    }
  }, [documents]);

  useEffect(() => {
    if (Object.keys(documentsObj).length > 0) {
      const startIndexRange = Math.max(index - maxAreaBounds, 0);
      const endIndexRange = index + maxAreaBounds;

      if (
        documents &&
        documents
          .slice(startIndexRange, endIndexRange)
          .some((i) => documentsObj[i.sutureSignRequestId] === 'loading')
      ) {
        documents.forEach(({ sutureSignRequestId }, listIndex) => {
          if (
            !fetchingDocs.current.get(sutureSignRequestId) &&
            documentsObj[sutureSignRequestId] === 'loading' &&
            listIndex >= startIndexRange &&
            listIndex <= endIndexRange
          ) {
            fetchDocument(sutureSignRequestId);
          }
        });
      }
    }
  }, [index, documents, documentsObj, fetchingDocs.current]);
}
