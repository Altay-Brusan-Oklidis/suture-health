import { useMemo } from 'react';
import { Button } from '@chakra-ui/react';
import { LockIcon, DollarIconFA } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import useDocumentPermissions from '@hooks/useDocumentPermissions';
import useMemberRole from '@hooks/useMemberRole';
import { useGetMiscSettingsQuery } from '@containers/Inbox/apiReducer';
import useAnnotationValidation from '@hooks/useAnnotationValidation';
import type { BtnProps } from '../../index';

export default function CPOBtn({ isDisabled, openModal }: BtnProps) {
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { canSign } = useDocumentPermissions();
  const { data: settings } = useGetMiscSettingsQuery();
  const { isSigner, isCollaboratorSigner, isCollaborator, isAssistant } =
    useMemberRole();
  const { isMovedTo } = useAnnotationValidation();
  const btnText = useMemo(() => {
    if (viewedDocument?.isIncomplete) {
      return 'Approve | CPO';
    }

    if (isSigner) {
      return 'Sign | CPO';
    }

    if (
      isCollaboratorSigner ||
      isCollaborator ||
      isAssistant ||
      settings?.allowOtherToSignFromScreen
    ) {
      return canSign ? 'Sign | CPO' : 'Sign/Approve | CPO';
    }

    return '';
  }, [
    isSigner,
    isCollaboratorSigner,
    isCollaborator,
    isAssistant,
    settings?.allowOtherToSignFromScreen,
    canSign,
    viewedDocument,
  ]);

  return (
    <Button
      w={{ sm: '100%', md: 'auto' }}
      isLoading={currentAction === 'submitCPOFaceToFace'}
      isDisabled={
        isDisabled ||
        (btnText.indexOf('Sign') !== -1 &&
          (viewedDocument?.isIncomplete === true || isMovedTo !== null))
      }
      variant="outline"
      leftIcon={
        !canSign &&
        settings?.allowOtherToSignFromScreen &&
        viewedDocument &&
        !viewedDocument.isIncomplete ? (
          <LockIcon color="blue.500" />
        ) : (
          <DollarIconFA />
        )
      }
      onClick={async () => {
        openModal('cpo');
      }}
      data-cy="sign-with-cpo"
    >
      {btnText}
    </Button>
  );
}
