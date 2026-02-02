import { useMemo } from 'react';
import { useSelector } from '@redux/hooks';

export default function useViewedDocIndex() {
  const documents = useSelector((state) => state.inbox.documents);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const docIndex = useMemo(() => {
    if (documents) {
      const index = documents.findIndex(
        (i) => i.sutureSignRequestId === viewedDocument?.sutureSignRequestId
      );

      return index >= 0 ? index : 0;
    }

    return 0;
  }, [viewedDocument, documents]);

  return docIndex;
}
