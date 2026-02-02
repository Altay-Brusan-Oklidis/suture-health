import { useEffect, useRef } from 'react';
import { Fade, Box } from '@chakra-ui/react';
import Lottie from 'lottie-react';
import scrollDown from '@public/animations/scrollDown.json';
import { useSelector } from '@redux/hooks';
import { DocumentSummaryItemType } from '@utils/zodModels';
import { useInboxActions } from '@containers/Inbox/localReducer';

export default function ScrollHint() {
  const documents = useSelector((state) => state.inbox.documents);
  const scrollHintVisible = useSelector(
    (state) => state.inbox.scrollHintVisible
  );
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const prevViewedDocument = useRef<DocumentSummaryItemType | null>(null);
  const { setScrollHintVisible } = useInboxActions();

  useEffect(() => {
    if (viewedDocument) {
      if (
        prevViewedDocument.current &&
        prevViewedDocument.current.sutureSignRequestId !==
          viewedDocument.sutureSignRequestId
      ) {
        if (scrollHintVisible) setScrollHintVisible(false);
      } else if (!scrollHintVisible && documents && documents?.length > 1) {
        setScrollHintVisible(true);
      }

      if (
        prevViewedDocument.current?.sutureSignRequestId !==
        viewedDocument.sutureSignRequestId
      ) {
        prevViewedDocument.current = viewedDocument;
      }
    }
  }, [viewedDocument, documents]);

  return (
    <Fade in={scrollHintVisible}>
      <Box className="scroll-hint-plug" />
      <Box
        width="44px"
        height="64px"
        borderRadius="10px"
        backdropFilter="blur(1px)"
        background="rgba(247, 250, 252, 0.85)"
        boxShadow="0px 4px 4px rgba(0, 0, 0, 0.25), -6px 5px 6px -1px rgba(0, 0, 0, 0.1);
            backdrop-filter: blur(1px)"
        display="flex"
        alignItems="center"
        justifyContent="center"
        pointerEvents="none"
        className="scroll-hint"
      >
        <Lottie
          animationData={scrollDown}
          style={{
            width: '108px',
            position: 'absolute',
          }}
          autoplay
        />
      </Box>
    </Fade>
  );
}
