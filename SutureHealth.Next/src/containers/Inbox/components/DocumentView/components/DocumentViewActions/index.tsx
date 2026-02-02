import { VStack, Tooltip, IconButton } from '@chakra-ui/react';
import {
  ArrowToTopIconFA,
  ArrowToBottomIconFA,
  Box,
  EditDocumentIconFA,
  ZoomInIcon,
  ZoomOutIcon,
  DownloadIcon,
} from 'suture-theme';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { ChevronLeftIcon } from '@chakra-ui/icons';
import useViewedDocIndex from '@hooks/useViewedDocIndex';
import isFaceToFaceDoc from '@utils/isFaceToFaceDoc';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import DocButton from './components/DocButton';
import AnnotationMenu from './components/AnnotationMenu';
import SignViewedButton from './components/SignViewedButton';
import ScrollHint from './components/ScrollHint';
import styles from './styles';
import { SaveIndicator } from './components/SaveIndicator';

interface Props {
  onArrowsClick?: (index: number) => void;
}

export default function DocumentViewActions({ onArrowsClick }: Props) {
  const { setIsEditing, zoomIn, zoomOut } = useInboxActions();
  const { syncAnnotations, isAnnotationsChanged } = useUpdateAnnotations();
  const isOpen: boolean = useSelector((state) => state.inbox.isEditing);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const documentScroll = useSelector((state) => state.inbox.documentScroll);
  const scale = useSelector((state) => state.inbox.scale);
  const documents = useSelector((state) => state.inbox.documents);
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const headerHeight = documentScroll
    ? 'var(--document-revenue-header-height)'
    : '0px';
  const docIndex = useViewedDocIndex();

  const handleClick = async () => {
    setIsEditing(!isOpen);
  };

  const docChange = async (direction: 'up' | 'down') => {
    if (isAnnotationsChanged) {
      syncAnnotations(true);
      setIsEditing(false);
    }
    if (direction === 'up') {
      onArrowsClick?.(docIndex - 1);
    } else {
      onArrowsClick?.(docIndex + 1);
    }
  };

  return (
    <Box
      position="sticky"
      width="100%"
      bottom="0"
      mt="auto"
      zIndex={4}
      opacity={stackAnimation ? 0 : 1}
      transitionDuration="normal"
      transitionProperty="opacity"
    >
      {styles}
      <Box
        position="absolute"
        bottom="0"
        height={{
          sm: `calc(var(--doc-height) - var(--document-information-header-height) - var(--document-actions-height) - 16px)`,
          xl: `calc(var(--inbox-content-height) - var(--document-information-header-height) - var(--document-actions-height) - 16px - ${headerHeight})`,
        }}
        w={{ sm: '100%', xl: 'calc(100% - 16px)' }}
        zIndex={4}
        pointerEvents="none"
        __css={{
          '> *': {
            pointerEvents: 'all',
          },
        }}
        transition="height .3s"
      >
        {viewedDocument && isFaceToFaceDoc(viewedDocument) ? null : (
          <>
            <IconButton
              position="absolute"
              left="12px"
              top={0}
              backgroundColor={isOpen ? 'gray.400' : 'cyan.500'}
              _hover={{ bg: 'cyan.600' }}
              w="55px"
              h="43px"
              ml="-12px"
              borderRadius={isOpen ? '0px 6px 0px 0px' : '0px 6px 6px 0px'}
              aria-label="edit document"
              icon={
                isOpen ? (
                  <ChevronLeftIcon color="gray.700" h="32px" w="32px" />
                ) : (
                  <EditDocumentIconFA color="white.500" w="24px" h="21.33px" />
                )
              }
              onClick={() => handleClick()}
            />
            <AnnotationMenu />
          </>
        )}
        <VStack
          position="absolute"
          right="12px"
          top={isOpen ? 'var(--document-annotation-drawer-height)' : 0}
          transition="top .3s"
        >
          <Tooltip
            label="Document not Downloadable"
            isDisabled={!isFaceToFaceDoc(viewedDocument)}
            shouldWrapChildren
          >
            <DocButton
              aria-label="download"
              icon={<DownloadIcon />}
              onClick={() =>
                window.open(
                  `/api/inbox/${viewedDocument?.sutureSignRequestId}/pdf`,
                  '_blank'
                )
              }
              isDisabled={!viewedDocument || isFaceToFaceDoc(viewedDocument)}
            />
          </Tooltip>
          <DocButton
            aria-label="zoom in"
            icon={<ZoomInIcon />}
            onClick={() => zoomIn()}
            isDisabled={scale === 1.8 || !viewedDocument}
            data-cy="zoom-in"
          />
          <DocButton
            aria-label="zoom out"
            icon={<ZoomOutIcon />}
            onClick={() => zoomOut()}
            isDisabled={scale === 0.4 || !viewedDocument}
            data-cy="zoom-out"
          />
        </VStack>
        <Box
          position="absolute"
          right="12px"
          bottom="12px"
          display="flex"
          alignItems="flex-end"
        >
          <Box display="flex" mr="12px">
            <ScrollHint />
            <SignViewedButton />
          </Box>
          <VStack className="quick-navigation">
            <SaveIndicator />
            {documents && documents.length > 0 && (
              <Box
                border="0.5px solid"
                borderColor="blackAlpha.200"
                color="gray.500"
                bg="gray.100"
                borderRadius="10px"
                fontSize="12px"
                padding="4px"
              >
                {`${docIndex + 1} of ${documents?.length}`}
              </Box>
            )}
            <DocButton
              aria-label="prev document"
              icon={<ArrowToTopIconFA />}
              isDisabled={docIndex === 0}
              onClick={() => docChange('up')}
              data-cy="prev-doc-btn"
            />
            <DocButton
              aria-label="next document"
              icon={<ArrowToBottomIconFA />}
              onClick={() => docChange('down')}
              isDisabled={
                !documents ||
                documents.length === 0 ||
                docIndex === documents.length - 1
              }
              data-cy="next-doc-btn"
            />
          </VStack>
        </Box>
      </Box>
    </Box>
  );
}
