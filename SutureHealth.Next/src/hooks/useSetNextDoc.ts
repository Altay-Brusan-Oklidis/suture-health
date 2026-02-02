import { useCallback, useMemo } from 'react';
import { useInboxActions, Animation } from '@containers/Inbox/localReducer';
import sortBy from '@utils/sortBy';
import { useSelector } from '@redux/hooks';
import useViewedDocIndex from '@hooks/useViewedDocIndex';

interface SetNextDocArgs {
  animation?: Animation;
  callback?: () => void;
  selectedDocuments?: string[];
}

export default function useSetNextDoc() {
  const documents = useSelector((state) => state.inbox.documents);
  const docIndex = useViewedDocIndex();
  const { setViewedDocument, setAnimation } = useInboxActions();

  const nextDoc = useMemo(() => {
    if (documents && documents.length) {
      return documents[docIndex + 1] || documents[docIndex - 1];
    }

    return undefined;
  }, [documents, docIndex]);

  const setNextDoc = useCallback(
    (args?: SetNextDocArgs) => {
      const { animation, callback, selectedDocuments } = args || {};
      let nextDocument = nextDoc;

      if (documents && selectedDocuments?.length) {
        const indexes = sortBy(
          documents.reduce((acc: number[], cur, index) => {
            if (
              selectedDocuments.includes(cur.sutureSignRequestId.toString())
            ) {
              return [...acc, index];
            }

            return acc;
          }, []),
          [(i) => i],
          [1]
        );
        const firstIndex = (indexes[0] || 0) - 1;
        const lastIndex = (indexes.at(-1) || 0) + 1;

        nextDocument = documents[firstIndex] || documents[lastIndex];
      }

      if (animation) {
        setAnimation(animation);

        setTimeout(() => {
          setViewedDocument(nextDocument);
          callback?.();
        }, 800);
      } else {
        setViewedDocument(nextDocument);
      }
    },
    [nextDoc, setViewedDocument, documents, setAnimation]
  );

  return setNextDoc;
}
