import { useEffect, useRef, useMemo, useCallback } from 'react';
import { Box } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import { Skeleton, chakra, shouldForwardProp } from '@chakra-ui/react';
import { useSwipeable } from 'react-swipeable';
import Panzoom, { PanzoomObject } from '@panzoom/panzoom';
import { InView } from 'react-intersection-observer';
import { motion, isValidMotionProp, MotionProps } from 'framer-motion';
import useSetNextDoc from '@hooks/useSetNextDoc';
import { useInboxActions } from '@containers/Inbox/localReducer';
import isFaceToFaceDoc from '@utils/isFaceToFaceDoc';
import round from '@utils/round';
import { BASE_SCALE } from '@lib/constants';
import AnnotationEngine from './components/AnnotationEngine';
import FaceToFaceElements from './components/FaceToFaceElements';
import styles from './styles';

const ChakraBox = chakra(motion.div, {
  shouldForwardProp: (prop) =>
    isValidMotionProp(prop) || shouldForwardProp(prop),
});

type ViewerProps = {
  documentId: number;
};

const ImageViewerSkeleton = () => (
  <Box ml="auto" mr="auto" width={826} overflow="hidden">
    <Skeleton width="100%" height={1169} my={4} />
  </Box>
);

const ImageViewer = ({ documentId }: ViewerProps) => {
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const doc = useMemo(
    () => documentsObj[documentId],
    [documentsObj, documentId]
  );
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const firstSelectedDoc = useMemo(() => {
    if (selectedDocuments.length) {
      const selectedDoc = documentsObj[selectedDocuments[0]!];

      if (selectedDoc !== 'loading' && selectedDoc) {
        return selectedDoc;
      }
    }

    return undefined;
  }, [documentsObj, selectedDocuments]);
  const selectedPagesCount = useMemo(() => {
    if (selectedDocuments.length && documentsObj) {
      return selectedDocuments.reduce((acc, cur) => {
        const curDoc = documentsObj[cur];

        if (curDoc && curDoc !== 'loading') {
          return acc + curDoc.pages.length;
        }

        return acc + 1; // if selected document isn't loaded
      }, 0);
    }

    return 0;
  }, [documentsObj, selectedDocuments]);
  const scale = useSelector((state) => state.inbox.scale);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const animation = useSelector((state) => state.inbox.animation);
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const totalViewedDocsLength = useSelector(
    (state) => state.inbox.totalViewedDocsLength
  );
  const { setViewedDocument, setCurrentPageData } = useInboxActions();
  const containerRef = useRef<HTMLDivElement>(null);
  const panzoom = useRef<PanzoomObject | null>(null);
  const setNextDoc = useSetNextDoc();
  const withStackAnimation = useMemo(
    () => stackAnimation && viewedDocument?.sutureSignRequestId === documentId,
    [stackAnimation, viewedDocument, documentId]
  );
  const handlers = useSwipeable({
    onSwiped: (event) => {
      switch (event.dir) {
        case 'Up': {
          setNextDoc();
          break;
        }

        case 'Right': {
          setViewedDocument(undefined);
          break;
        }

        default:
          break;
      }
    },
    delta: 100,
    swipeDuration: 200,
  });
  const isFaceToFaceDocument = useMemo(
    () => doc && doc !== 'loading' && isFaceToFaceDoc(doc.document),
    [doc]
  );

  useEffect(() => {
    panzoom.current = Panzoom(containerRef.current!, {
      cursor: 'default',
      disableZoom: true,
      handleStartEvent: (event: Event) => {
        event.stopPropagation();
      },
    });
  }, []);

  useEffect(() => {
    panzoom.current?.zoom(scale, { animate: true, force: true });
  }, [scale]);

  useEffect(() => {
    if (
      doc !== 'loading' &&
      doc?.document.sutureSignRequestId !== viewedDocument?.sutureSignRequestId
    ) {
      panzoom.current?.reset();
    }
  }, [viewedDocument, doc]);

  const getAnimate = useCallback(
    (index: number): MotionProps['animate'] => {
      switch (stackAnimation) {
        case 'fanning':
        case 'fanningForSelectedDocs': {
          return {
            rotate: 2 * index,
          };
        }

        case 'stackedLeftDown':
        case 'fadeOut': {
          return {
            x: 10 * index,
            y: -10 * index,
          };
        }

        default:
          break;
      }

      return undefined;
    },
    [stackAnimation]
  );

  return (
    <>
      {styles}
      <Box
        minHeight="calc(var(--inbox-content-height) - var(--document-information-header-height) - var(--document-actions-height))"
        height={withStackAnimation ? 'var(--inbox-content-height)' : 'auto'}
        transform={`${
          documentId === animation?.documentId
            ? `rotate(${
                animation.type === 'rejected' ? '-' : ''
              }15deg) translateX(${
                animation.type === 'rejected' ? '-' : ''
              }100%)`
            : ''
        }`}
        transition={`${
          documentId === animation?.documentId ? 'all .5s' : 'unset'
        }`}
        transitionDelay=".5s"
        display="flex"
        justifyContent="center"
        {...handlers}
      >
        <Box
          ref={containerRef}
          width={
            withStackAnimation && doc !== 'loading' && !isFaceToFaceDocument
              ? '100%'
              : 'auto'
          }
          padding={
            withStackAnimation && doc !== 'loading' && !isFaceToFaceDocument
              ? '104px 0px 84px'
              : '0'
          }
        >
          {doc === 'loading' || doc === null || doc === undefined ? (
            <ImageViewerSkeleton />
          ) : isFaceToFaceDocument ? (
            <Box width={826}>
              <FaceToFaceElements doc={doc} />
            </Box>
          ) : (
            <Box
              data-cy="doc-view"
              display="flex"
              alignItems="center"
              flexDirection="column"
              height={withStackAnimation ? '100%' : 'auto'}
              className={
                stackAnimation === 'fadeOut' ? 'save-fade-out' : undefined
              }
              position="relative"
            >
              <Box position="absolute" height="100%">
                {(stackAnimation === 'fanningForSelectedDocs' &&
                firstSelectedDoc
                  ? firstSelectedDoc
                  : doc
                ).pages
                  .slice(0, 1)
                  .map((page) => (
                    <Box
                      key={`${doc.document.sutureSignRequestId}-${page.pageNumber}`}
                      height="100%"
                      position="relative"
                      display={withStackAnimation ? 'block' : 'none'}
                    >
                      {Array.from({
                        length: Math.min(
                          5,
                          stackAnimation === 'fanning'
                            ? totalViewedDocsLength
                            : stackAnimation === 'fanningForSelectedDocs'
                            ? selectedPagesCount
                            : doc.pages.length
                        ),
                      }).map((_, key, tempImgs) => (
                        <ChakraBox
                          key={key}
                          animate={getAnimate(key)}
                          position={key === 0 ? 'static' : 'absolute'}
                          top={0}
                          width="100%"
                          height="100%"
                          transformOrigin="right bottom"
                          transitionProperty="transform"
                          transitionDuration=".3s"
                          filter="drop-shadow(0px 1px 3px rgba(0, 0, 0, 0.1)) drop-shadow(0px 1px 2px rgba(0, 0, 0, 0.06))"
                        >
                          {/* eslint-disable-next-line @next/next/no-img-element */}
                          <img
                            src={`data:image/png;base64,${page.base64 || ''}`}
                            alt="document"
                            style={{
                              objectFit: 'contain',
                              transitionDuration: '.3s',
                              width: '100%',
                              height: '100%',
                              filter: `${
                                key !== tempImgs.length - 1
                                  ? 'brightness(0) invert(1)'
                                  : ''
                              }`,
                            }}
                          />
                        </ChakraBox>
                      ))}
                    </Box>
                  ))}
              </Box>
              {doc.pages.map((page) => (
                <InView
                  key={`${doc.document.sutureSignRequestId}-${page.pageNumber}`}
                  onChange={(visible) => {
                    if (visible) {
                      setCurrentPageData({
                        pageNumber: page.pageNumber,
                        documentReqId: documentId,
                        dimensions: {
                          width: round(page.dimensions.width * BASE_SCALE, 0),
                          height: round(page.dimensions.height * BASE_SCALE, 0),
                        },
                      });
                    }
                  }}
                  threshold={0.2}
                  skip={Boolean(stackAnimation)}
                >
                  <Box
                    backgroundImage={`data:image/png;base64,${
                      page.base64 || ''
                    }`}
                    width={round(page.dimensions.width * BASE_SCALE, 0)}
                    height={round(page.dimensions.height * BASE_SCALE, 0)}
                    my={4}
                    data-smartlook="ignore"
                    backgroundSize="contain"
                    visibility={withStackAnimation ? 'hidden' : 'visible'}
                  >
                    {isFaceToFaceDoc(doc.document) ? null : (
                      <AnnotationEngine
                        document={doc}
                        pageNumber={page.pageNumber}
                        documentReqId={doc.document.sutureSignRequestId}
                        width={page.dimensions.width}
                        height={page.dimensions.height}
                      />
                    )}
                  </Box>
                </InView>
              ))}
            </Box>
          )}
        </Box>
      </Box>
    </>
  );
};

export default ImageViewer;
