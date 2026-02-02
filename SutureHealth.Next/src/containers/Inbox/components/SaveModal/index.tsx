import { useEffect } from 'react';
import {
  Modal,
  ModalBody,
  ModalContent,
  ModalFooter,
  Text,
  ModalOverlay,
  useDisclosure,
} from '@chakra-ui/react';
import { useRouter } from 'next/router';
import { Button, CloudArrowUpFA } from 'suture-theme';
import ModalTitle from '@components/ModalTitle';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { useSelector } from '@redux/hooks';
import { UrlValues, FADE_OUT_ANIM_DURATION } from '@lib/constants';
import useAnnotationValidation from '@hooks/useAnnotationValidation';
import useSetNextDoc from '@hooks/useSetNextDoc';
import { useInboxActions } from '@containers/Inbox/localReducer';

interface BaseProps {
  isOpen: boolean;
  onClose: () => void;
}

interface SaveModalProps extends BaseProps {
  location?: never;
  isLeaveAction?: never;
}

interface LeaveModalProps extends BaseProps {
  location: UrlValues;
  isLeaveAction: boolean;
}

type Props = SaveModalProps | LeaveModalProps;

const SaveModal = ({
  isOpen: triggerOpen,
  onClose: onCloseModal,
  location,
  isLeaveAction,
}: Props) => {
  const router = useRouter();
  const { isOpen, onClose, onOpen } = useDisclosure();
  const { isMovedTo } = useAnnotationValidation();
  const { syncAnnotations } = useUpdateAnnotations();
  const isAnnotationsUpdating = useSelector(
    (state) => state.inbox.isAnnotationsUpdating
  );
  const { setStackAnimation, setCurrentPageData } = useInboxActions();
  const setNextDoc = useSetNextDoc();

  const triggerClose = (withAnimation = false) => {
    onClose();
    onCloseModal();

    if (withAnimation) {
      setTimeout(() => setStackAnimation('fadeOut'), 100);
      setTimeout(() => {
        setStackAnimation(undefined);
        setNextDoc();
      }, FADE_OUT_ANIM_DURATION);
    } else {
      setStackAnimation(undefined);
    }
  };

  useEffect(() => {
    if (triggerOpen) {
      setStackAnimation('stackedLeftDown');
      setTimeout(() => onOpen(), 400); // 100ms + transitionDuration of the ChakraBox in the ImageViewer
    }
  }, [triggerOpen]);

  const handleClick = async (save: boolean) => {
    if (save) {
      await syncAnnotations(true);
      triggerClose(true);

      if (isLeaveAction) {
        router.push(location);
      }
    } else {
      setCurrentPageData({ pageNumber: 0 }); // to avoid the beforeunload listener
      triggerClose();
      if (isLeaveAction) router.push(location);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={triggerClose} isCentered>
      <ModalOverlay />
      <ModalContent>
        <ModalTitle
          circleColor="cyan.500"
          icon={<CloudArrowUpFA h="24px" w="24px" />}
          title="Are You Done?"
        />
        <ModalBody>
          <Text fontSize="14px" color="gray.500" mt={2} fontWeight={500}>
            When saved, the document will be removed from your current view &
            moved to the {isMovedTo} list
          </Text>
        </ModalBody>
        <ModalFooter>
          <Button
            variant="ghost"
            onClick={() => triggerClose()}
            colorScheme="gray"
          >
            Cancel
          </Button>
          {isLeaveAction && (
            <Button
              variant="outline"
              ml="2"
              onClick={() => handleClick(false)}
              colorScheme="blue"
            >
              Leave without Saving
            </Button>
          )}
          <Button
            ml="2"
            onClick={() => handleClick(true)}
            leftIcon={<CloudArrowUpFA h="24px" w="24px" />}
            isDisabled={isAnnotationsUpdating}
            isLoading={isAnnotationsUpdating}
          >
            Save
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};

export default SaveModal;
