import { useCallback, useMemo, useState } from 'react';
import { UserEditIconFA } from 'suture-theme';
import { IconButton, Modal, ModalOverlay, Tooltip } from '@chakra-ui/react';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';
import BulkModal from '@containers/Inbox/components/DocumentView/components/DocumentViewActions/components/SignViewedButton/components/BulkModal';
import useMemberRole from '@hooks/useMemberRole';
import { useSelector } from '@redux/hooks';
import { DocumentSummaryItemType } from '@utils/zodModels';
import { STACK_ANIM_DURATION } from '@lib/constants';
import { useInboxActions } from '@containers/Inbox/localReducer';
import type { BulkButtonProps } from '../index';

const ReassignIconButton = ({ tooltipDelay }: BulkButtonProps) => {
  const [isOpenModal, setIsOpenModal] = useState<boolean>(false);
  const { isSigner, isCollaboratorSigner } = useMemberRole();
  const { data: member } = useGetMemberIdentityQuery();
  const documents = useSelector((state) => state.inbox.documents);
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const viewedDocIds = useSelector((state) => state.inbox.viewedDocIds);
  const { setStackAnimation } = useInboxActions();

  const isReassignableUser = useMemo(
    () => isSigner || isCollaboratorSigner,
    [isSigner, isCollaboratorSigner]
  );

  const isReassignableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member &&
      doc.signer.memberId !== member.memberId &&
      doc.template.templateType.signerChangeAllowed &&
      !doc.isIncomplete,
    [member]
  );

  const reassignableDocs = useMemo(() => {
    if (documents?.length && isReassignableUser) {
      return documents?.filter(
        (doc) =>
          selectedDocuments.includes(doc.sutureSignRequestId.toString()) &&
          viewedDocIds.includes(doc.sutureSignRequestId) &&
          isReassignableDoc(doc)
      );
    }

    return [];
  }, [
    documents,
    selectedDocuments,
    viewedDocIds,
    isReassignableDoc,
    isReassignableUser,
  ]);

  const isDisabled = useMemo(() => {
    if (!reassignableDocs.length) {
      return true;
    }

    return false;
  }, [reassignableDocs.length]);

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
          title="Bulk Reassignment"
          approvableDocs={[]}
          signableDocs={[]}
          reassignableDocs={reassignableDocs}
          otherSignableDocs={[]}
        />
      </Modal>
      <Tooltip label="Reassign" placement="top" openDelay={tooltipDelay}>
        <IconButton
          variant="ghost"
          isDisabled={isDisabled}
          color="gray.500"
          colorScheme="gray"
          aria-label="reassign"
          icon={<UserEditIconFA color="gray.700" />}
          onClick={() => {
            setStackAnimation('fanningForSelectedDocs');
            setTimeout(() => setIsOpenModal(true), STACK_ANIM_DURATION);
          }}
          fontSize="16px"
          size="sm"
        />
      </Tooltip>
    </>
  );
};

export default ReassignIconButton;
