import { useRef, memo, ReactNode, useMemo } from 'react';
import { Virtuoso, VirtuosoHandle } from 'react-virtuoso';
import { InView } from 'react-intersection-observer';
import { Flex } from '@chakra-ui/react';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { DocumentSummaryItemType } from '@utils/zodModels';
import useDeviceHandler from '@hooks/useDeviceHandler';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { useSelector } from '@redux/hooks';
import debounce from 'lodash.debounce';
import ImageViewer from './components/ImageViewer';
import DocumentViewActions from './components/DocumentViewActions';
import DocumentHeader from './components/DocumentHeader';
import ScrollHandler from './components/ScrollHandler';
import RevenuePanel from './components/RevenuePanel';
import AnnotationSetter from './components/AnnotationSetter';
import MetadataSaver from './components/MetadataSaver';

interface Props {
  doc: DocumentSummaryItemType;
  debouncedInView: (document?: DocumentSummaryItemType) => void;
  children: ReactNode;
}

const ItemWrapper = ({ doc, debouncedInView, children }: Props) => {
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const { syncAnnotations } = useUpdateAnnotations();

  return (
    <InView
      onChange={(visible) => {
        if (!stackAnimation) {
          syncAnnotations(true);
        }
        if (visible) {
          debouncedInView(doc);
        }
      }}
      threshold={0.06}
      delay={100}
    >
      {children}
    </InView>
  );
};

function DocumentView() {
  const { setDocumentScroll } = useInboxActions();
  const virtuosoRef = useRef<VirtuosoHandle>(null);
  const documents = useSelector((state) => state.inbox.documents);
  const { setViewedDocument } = useInboxActions();
  const { isMobile } = useDeviceHandler();

  const debouncedInView = useMemo(
    () =>
      debounce((document?: DocumentSummaryItemType) => {
        setViewedDocument(document);
      }, 200),
    []
  );

  return (
    <Flex
      w="100%"
      overflow="hidden"
      height="full"
      bg="gray.500"
      direction="column"
      alignItems="center"
    >
      <AnnotationSetter />
      <MetadataSaver />
      <ScrollHandler
        virtuosoRef={virtuosoRef}
        cancelDebouncedInView={debouncedInView.cancel}
      />
      <Virtuoso
        id="document-virtuoso-container"
        ref={virtuosoRef}
        style={{
          height: '100%',
          width: '100%',
          overflow: `hidden ${isMobile ? 'hidden' : 'auto'}`,
        }}
        components={{
          Header: RevenuePanel,
        }}
        atTopStateChange={setDocumentScroll}
        atTopThreshold={50}
        data={documents}
        computeItemKey={(_, i) => i.sutureSignRequestId}
        itemContent={(_, doc) => (
          <ItemWrapper doc={doc} debouncedInView={debouncedInView}>
            <DocumentHeader documentId={doc.sutureSignRequestId} />
            <ImageViewer documentId={doc.sutureSignRequestId} />
          </ItemWrapper>
        )}
      />
      <DocumentViewActions
        onArrowsClick={(index) => {
          virtuosoRef.current?.scrollToIndex({
            index,
            behavior: 'smooth',
            align: 'start',
          });
        }}
      />
    </Flex>
  );
}

export default memo(DocumentView);
