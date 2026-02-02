import { useState, useMemo } from 'react';
import {
  IconButton,
  Modal,
  ModalContent,
  ModalOverlay,
  Tooltip,
} from '@chakra-ui/react';
import { BanIconFA } from 'suture-theme';
import RejectionModal from '@containers/Inbox/components/DocumentActions/components/RejectionModal';
import { useSelector } from '@redux/hooks';
import { STACK_ANIM_DURATION } from '@lib/constants';
import { useInboxActions } from '@containers/Inbox/localReducer';
import type { BulkButtonProps } from '../index';

const RejectIconButton = ({ tooltipDelay }: BulkButtonProps) => {
  const [isOpenModal, setIsOpenModal] = useState<boolean>(false);
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const viewedDocIds = useSelector((state) => state.inbox.viewedDocIds);
  const { setStackAnimation } = useInboxActions();
  const documents = useSelector((state) => state.inbox.documents);

  const rejectableDocs = useMemo(() => {
    if (documents?.length) {
      return documents?.filter(
        (doc) =>
          selectedDocuments.includes(doc.sutureSignRequestId.toString()) &&
          viewedDocIds.includes(doc.sutureSignRequestId)
      );
    }

    return [];
  }, [documents, selectedDocuments, viewedDocIds]);

  const isDisabled = useMemo(() => {
    if (rejectableDocs.length > 0) {
      return false;
    }

    return true;
  }, [rejectableDocs]);

  const openModal = () => {
    setStackAnimation('fanningForSelectedDocs');
    setTimeout(() => setIsOpenModal(true), STACK_ANIM_DURATION);
  };

  const onModalClose = () => {
    setStackAnimation(undefined);
    setIsOpenModal(false);
  };

  return (
    <>
      <Modal isOpen={isOpenModal} onClose={onModalClose} size="lg">
        <ModalOverlay />
        <ModalContent>
          <RejectionModal
            onClose={onModalClose}
            rejectableDocs={rejectableDocs}
          />
        </ModalContent>
      </Modal>
      <Tooltip label="Reject" placement="top" openDelay={tooltipDelay}>
        <IconButton
          isDisabled={isDisabled}
          icon={<BanIconFA />}
          onClick={openModal}
          variant="ghost"
          colorScheme="gray"
          aria-label="Sign"
          fontSize="16px"
          size="sm"
        />
      </Tooltip>
    </>
  );
};

export default RejectIconButton;
