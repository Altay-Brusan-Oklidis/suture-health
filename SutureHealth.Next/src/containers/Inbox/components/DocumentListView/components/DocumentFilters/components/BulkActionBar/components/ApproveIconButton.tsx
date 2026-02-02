import { useCallback, useMemo, useState } from 'react';
import { IconButton, Modal, ModalOverlay, Tooltip } from '@chakra-ui/react';
import { CheckCircleIconFA } from 'suture-theme';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';
import BulkModal from '@containers/Inbox/components/DocumentView/components/DocumentViewActions/components/SignViewedButton/components/BulkModal';
import useMemberRole from '@hooks/useMemberRole';
import { useSelector } from '@redux/hooks';
import { DocumentSummaryItemType } from '@utils/zodModels';
import { STACK_ANIM_DURATION } from '@lib/constants';
import { useInboxActions } from '@containers/Inbox/localReducer';
import type { BulkButtonProps } from '../index';

const ApproveIconButton = ({ tooltipDelay }: BulkButtonProps) => {
  const [isOpenModal, setIsOpenModal] = useState<boolean>(false);
  const { data: member } = useGetMemberIdentityQuery();
  const documents = useSelector((state) => state.inbox.documents);
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const viewedDocIds = useSelector((state) => state.inbox.viewedDocIds);
  const { setStackAnimation } = useInboxActions();
  const { isCollaboratorSigner, isCollaborator, isAssistant } = useMemberRole();

  const isApprovableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member &&
      doc.signer.memberId !== member.memberId &&
      !doc.approvals.find(
        (approval) => approval.approver.memberId === member.memberId
      ),
    [member]
  );

  const isApprovableUser = useMemo(
    () => isAssistant || isCollaborator || isCollaboratorSigner,
    [isAssistant, isCollaborator, isCollaboratorSigner]
  );

  const approvableDocs = useMemo(() => {
    if (documents?.length && isApprovableUser) {
      return documents.filter(
        (doc) =>
          selectedDocuments.includes(doc.sutureSignRequestId.toString()) &&
          viewedDocIds.includes(doc.sutureSignRequestId) &&
          isApprovableDoc(doc)
      );
    }

    return [];
  }, [
    documents,
    selectedDocuments,
    isApprovableDoc,
    isApprovableUser,
    viewedDocIds,
  ]);

  const isDisabled = useMemo(() => {
    if (!approvableDocs.length) {
      return true;
    }

    return false;
  }, [approvableDocs.length]);

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
        <BulkModal
          onModalClose={onModalClose}
          title="Bulk Approve"
          approvableDocs={approvableDocs}
          signableDocs={[]}
          reassignableDocs={[]}
          otherSignableDocs={[]}
        />
      </Modal>
      <Tooltip label="Approve" placement="top" openDelay={tooltipDelay}>
        <IconButton
          isDisabled={isDisabled}
          icon={<CheckCircleIconFA />}
          onClick={openModal}
          variant="ghost"
          colorScheme="gray"
          aria-label="approve"
          fontSize="16px"
          size="sm"
        />
      </Tooltip>
    </>
  );
};

export default ApproveIconButton;
