import { Button, useToast } from '@chakra-ui/react';
import { LockIcon, SignIconFA, Toast } from 'suture-theme';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useSelector } from '@redux/hooks';
import useDocumentPermissions from '@hooks/useDocumentPermissions';
import externalSubmit from '@utils/externalSubmit';
import { FACE_FORM_ID } from '@containers/Inbox/components/DocumentView/components/ImageViewer/components/FaceToFaceElements';
import {
  useSignDocumentMutation,
  useRemoveDocsMutation,
  useGetMiscSettingsQuery,
} from '@containers/Inbox/apiReducer';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useErrorToast from '@hooks/useErrorToast';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import type { BtnProps } from '../../index';

export default function SignBtn({
  isDisabled,
  openModal,
  isFaceToFaceDoc,
}: BtnProps) {
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { canSign } = useDocumentPermissions();
  const [signDocument] = useSignDocumentMutation();
  const [removeDocs] = useRemoveDocsMutation();
  const { data: settings } = useGetMiscSettingsQuery();
  const { setCurrentAction, setIsEditing } = useInboxActions();
  const setNextDoc = useSetNextDoc();
  const showErrorToast = useErrorToast();
  const toast = useToast({ render: Toast });
  const { syncAnnotations } = useUpdateAnnotations();

  return (
    <Button
      w={{ sm: '100%', md: 'auto' }}
      isLoading={
        currentAction === 'sign' || currentAction === 'submitFaceToFace'
      }
      isDisabled={isDisabled}
      display={
        canSign || settings?.allowOtherToSignFromScreen ? 'flex' : 'none'
      }
      leftIcon={
        !canSign && viewedDocument ? (
          <LockIcon data-cy="sign-lock-icon" />
        ) : (
          <SignIconFA />
        )
      }
      onClick={async () => {
        if (isFaceToFaceDoc) {
          externalSubmit(FACE_FORM_ID);

          return;
        }

        await syncAnnotations();

        if (canSign) {
          try {
            setCurrentAction('sign');
            setIsEditing(false);

            await signDocument({
              id: viewedDocument!.sutureSignRequestId,
              signerPassword: '',
            }).unwrap();

            setNextDoc({
              animation: {
                type: 'success',
                documentId: viewedDocument!.sutureSignRequestId,
              },
              callback: () =>
                removeDocs({
                  documentIds: [viewedDocument!.sutureSignRequestId],
                }),
            });
            toast({
              title: 'Document Signed Successfully',
              status: 'success',
              duration: 2000,
            });
          } catch (error) {
            showErrorToast(error);
          } finally {
            setCurrentAction(null);
          }
        } else {
          openModal('sign');
        }
      }}
      data-cy="sign-btn"
    >
      Sign
    </Button>
  );
}
