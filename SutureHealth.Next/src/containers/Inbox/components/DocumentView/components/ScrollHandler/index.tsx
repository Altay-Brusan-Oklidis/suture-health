import { RefObject, useMemo, useEffect, useState } from 'react';
import { VirtuosoHandle } from 'react-virtuoso';
import { useDocumentsRef, useSetDocViewRef } from '@containers/Inbox/context';
import { useSelector } from '@redux/hooks';
import debounce from 'lodash.debounce';
import { useInboxActions } from '@containers/Inbox/localReducer';
import useDeviceHandler from '@hooks/useDeviceHandler';
import useViewedDocIndex from '@hooks/useViewedDocIndex';
import usePrevious from '@hooks/usePrevious';

interface Props {
  virtuosoRef: RefObject<VirtuosoHandle>;
  cancelDebouncedInView: () => void;
}

interface ScrollArgs {
  docsList: VirtuosoHandle;
  docList: RefObject<VirtuosoHandle>;
  index: number;
  localIndex: number;
}

export default function ScrollHandler({
  virtuosoRef,
  cancelDebouncedInView,
}: Props) {
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const documents = useSelector((state) => state.inbox.documents);
  const isDocumentsLoading = useSelector(
    (state) => state.inbox.isDocumentsLoading
  );
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const documentsRef = useDocumentsRef();
  const setDocViewRef = useSetDocViewRef();
  const { setViewedDocument } = useInboxActions();
  const { isMobile } = useDeviceHandler();
  const [currentDocIndex, setCurrentDocIndex] = useState<number>(0);
  const [isScrollingDisable, setIsScrollingDisable] = useState<boolean>(false);
  const viewedDocIndex = useViewedDocIndex();
  const prevDocsLength = usePrevious(documents?.length);

  const debouncedScroll = useMemo(
    () =>
      debounce(({ docsList, docList, index, localIndex }: ScrollArgs) => {
        if (
          docsList &&
          docList.current &&
          localIndex !== index // to avoid scrolling on the first render
        ) {
          docsList.scrollToIndex({
            align: 'center',
            index,
            behavior: 'smooth',
          });
          if (index === 0) {
            docList.current.scrollTo({
              top: 0,
            });
          } else {
            docList.current.scrollToIndex({
              align: 'start',
              index,
              behavior: 'smooth',
            });
          }
          setCurrentDocIndex(index);
        }
      }, 0),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  useEffect(() => {
    if (
      documentsRef &&
      virtuosoRef.current &&
      prevDocsLength &&
      documents?.length &&
      prevDocsLength - documents.length !== 0
    ) {
      const realIndex = documents.findIndex(
        (i) => i.sutureSignRequestId === viewedDocument?.sutureSignRequestId
      );

      setIsScrollingDisable(true);
      cancelDebouncedInView();
      debouncedScroll.cancel();

      documentsRef.scrollToIndex({
        align: 'center',
        index: realIndex,
        behavior: 'smooth',
      });
      virtuosoRef.current.scrollToIndex({
        index: realIndex,
      });

      setCurrentDocIndex(realIndex);
      setIsScrollingDisable(false);
    }
  }, [prevDocsLength, documents?.length]);

  useEffect(() => {
    if (virtuosoRef.current && setDocViewRef) {
      setDocViewRef(virtuosoRef.current);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [virtuosoRef.current, setDocViewRef]);

  useEffect(() => {
    if (
      !isDocumentsLoading &&
      documents &&
      documentsRef &&
      !isScrollingDisable
    ) {
      debouncedScroll({
        docsList: documentsRef,
        docList: virtuosoRef,
        index: viewedDocIndex,
        localIndex: currentDocIndex,
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    documentsRef,
    virtuosoRef.current,
    debouncedScroll,
    viewedDocIndex,
    documents,
    isDocumentsLoading,
    isScrollingDisable,
  ]);

  useEffect(() => {
    if (documents && !isDocumentsLoading) {
      if (documents.length && !viewedDocument && !isMobile) {
        setViewedDocument(documents[0]);
      } else if (
        documents.findIndex(
          (i) => i.sutureSignRequestId === viewedDocument?.sutureSignRequestId
        ) === -1 &&
        !isMobile
      ) {
        setViewedDocument(documents[0]);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [documents, viewedDocument, isMobile, isDocumentsLoading]);

  useEffect(() => {
    if (stackAnimation) {
      virtuosoRef.current?.scrollToIndex({
        align: 'start',
        index: viewedDocIndex,
      });
    }
  }, [stackAnimation, viewedDocIndex]);

  return null;
}
