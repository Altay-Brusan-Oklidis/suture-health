import { useCallback, useMemo, useState } from 'react';
import { SignIconFA } from 'suture-theme';
import { IconButton, Modal, ModalOverlay, Tooltip } from '@chakra-ui/react';
import {
  useGetMemberIdentityQuery,
  useGetMiscSettingsQuery,
} from '@containers/Inbox/apiReducer';
import useMemberRole from '@hooks/useMemberRole';
import { DocumentSummaryItemType } from '@utils/zodModels';
import { useSelector } from '@redux/hooks';
import BulkModal from '@containers/Inbox/components/DocumentView/components/DocumentViewActions/components/SignViewedButton/components/BulkModal';
import isSavedForLaterDoc from '@utils/isSavedForLaterDoc';
import { STACK_ANIM_DURATION } from '@lib/constants';
import { useInboxActions } from '@containers/Inbox/localReducer';
import type { BulkButtonProps } from '../index';

const SignIconButton = ({ tooltipDelay }: BulkButtonProps) => {
  const [isOpenModal, setIsOpenModal] = useState<boolean>(false);
  const { data: member } = useGetMemberIdentityQuery();
  const { isSigner, isCollaboratorSigner } = useMemberRole();
  const { data: settings } = useGetMiscSettingsQuery();
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const documents = useSelector((state) => state.inbox.documents);
  const viewedDocIds = useSelector((state) => state.inbox.viewedDocIds);
  const { setStackAnimation } = useInboxActions();
  const viewedDocs = useMemo(() => {
    if (documents) {
      return documents.filter(
        (i) =>
          selectedDocuments.includes(i.sutureSignRequestId.toString()) &&
          viewedDocIds.includes(i.sutureSignRequestId)
      );
    }

    return [];
  }, [documents, viewedDocIds, selectedDocuments]);

  const isSignableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member &&
      doc.signer.memberId === member.memberId &&
      !doc.isIncomplete &&
      !isSavedForLaterDoc(doc),
    [member]
  );

  const isSignableUser = useMemo(
    () => isSigner || isCollaboratorSigner,
    [isSigner, isCollaboratorSigner]
  );

  const isOtherSignableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member && doc.signer.memberId !== member.memberId && !doc.isIncomplete,
    [member]
  );

  const isOtherSignableUser = useMemo(
    () =>
      isSigner || isCollaboratorSigner || settings?.allowOtherToSignFromScreen,
    [isSigner, isCollaboratorSigner, settings?.allowOtherToSignFromScreen]
  );

  const signableDocs = useMemo(() => {
    if (viewedDocs.length && isSignableUser) {
      return viewedDocs.filter(isSignableDoc);
    }

    return [];
  }, [viewedDocs, isSignableUser, isSignableDoc]);

  const otherSignableDocs = useMemo(() => {
    if (viewedDocs.length && isOtherSignableUser) {
      return viewedDocs.filter(isOtherSignableDoc);
    }

    return [];
  }, [viewedDocs, isOtherSignableUser, isOtherSignableDoc]);

  const isDisabled = useMemo(() => {
    if (!signableDocs.length && !otherSignableDocs.length) {
      return true;
    }

    return false;
  }, [otherSignableDocs.length, signableDocs.length]);

  const openModal = () => {
    setStackAnimation('fanningForSelectedDocs');
    setTimeout(() => setIsOpenModal(true), STACK_ANIM_DURATION);
  };

  const onModalClose = () => {
    setStackAnimation(undefined);
    setIsOpenModal(false);
  };

  return isSignableUser || isOtherSignableUser ? (
    <>
      <Modal isOpen={isOpenModal} onClose={onModalClose} size="lg">
        <ModalOverlay />
        <BulkModal
          onModalClose={onModalClose}
          title="Bulk Sign"
          approvableDocs={[]}
          signableDocs={signableDocs}
          reassignableDocs={[]}
          otherSignableDocs={otherSignableDocs}
        />
      </Modal>
      <Tooltip label="Sign" placement="top" openDelay={tooltipDelay}>
        <IconButton
          isDisabled={isDisabled}
          icon={<SignIconFA />}
          onClick={openModal}
          variant="ghost"
          colorScheme="gray"
          aria-label="Sign"
          fontSize="16px"
          size="sm"
        />
      </Tooltip>
    </>
  ) : null;
};

export default SignIconButton;
