import { useState } from 'react';
import {
  Flex,
  Modal,
  ModalOverlay,
  ModalContent,
  useToast,
} from '@chakra-ui/react';
import { useFormContext, useWatch } from 'react-hook-form';
import uniqBy from '@utils/uniqBy';
import { useSelector } from '@redux/hooks';
import useDocumentPermissions from '@hooks/useDocumentPermissions';
import type { FilterData } from '@containers/Inbox';
import SignModal from '@components/SignModal';
import { Button, Toast } from 'suture-theme';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useErrorToast from '@hooks/useErrorToast';
import useDeviceHandler from '@hooks/useDeviceHandler';
import { useInboxActions } from '@containers/Inbox/localReducer';
import {
  useSignDocumentsMutation,
  useRemoveDocsMutation,
} from '@containers/Inbox/apiReducer';

export default function SignSelected() {
  const [isOpenModal, setIsOpenModal] = useState<boolean>(false);
  const documents = useSelector((state) => state.inbox.documents);
  const { canSign } = useDocumentPermissions();
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const { setCurrentAction, setSelectedDocuments } = useInboxActions();
  const { control } = useFormContext<FilterData>();
  const documentProcessStatus = useWatch({
    control,
    name: 'documentProcessStatus',
  });
  const toast = useToast({ render: Toast });
  const [signDocuments] = useSignDocumentsMutation();
  const showErrorToast = useErrorToast();
  const setNextDoc = useSetNextDoc();
  const { isMobile, isDesktop } = useDeviceHandler();
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const [removeDocs] = useRemoveDocsMutation();

  const openModal = () => {
    setIsOpenModal(true);
  };

  const onModalClose = () => {
    setIsOpenModal(false);
  };

  return (
    <>
      <Modal isOpen={isOpenModal} onClose={onModalClose} size="lg">
        <ModalOverlay />
        <ModalContent>
          <SignModal onClose={onModalClose} signMultipleDocuments />
        </ModalContent>
      </Modal>
      {((isMobile && documentProcessStatus === 'needssignature') ||
        isDesktop) && (
        <Flex
          bg="white"
          width={{ sm: '100%', xl: 'var(--document-list-width)' }}
          justify="center"
          align="center"
          minHeight="var(--document-actions-height)"
        >
          {false &&
            documentProcessStatus === 'needssignature' && ( // TODO: remove false
              <Button
                isLoading={currentAction === 'signSelected'}
                isDisabled={!selectedDocuments.length || Boolean(currentAction)}
                onClick={async () => {
                  const isDocumentsFromOneSigner =
                    uniqBy(
                      documents!.filter((i) =>
                        selectedDocuments.includes(
                          i.sutureSignRequestId.toString()
                        )
                      ),
                      (i) => i.signer.memberId
                    ).length === 1;

                  if (!isDocumentsFromOneSigner) {
                    toast({
                      title:
                        'To bulk sign documents, please filter by the intended signer',
                      status: 'info',
                      duration: 6000,
                    });
                  } else if (canSign) {
                    setCurrentAction('signSelected');

                    try {
                      await signDocuments({
                        ids: selectedDocuments,
                        signerPassword: '',
                      }).unwrap();

                      setNextDoc({
                        selectedDocuments,
                        animation: {
                          type: 'success',
                          documentId: viewedDocument!.sutureSignRequestId,
                        },
                        callback: () =>
                          removeDocs({
                            documentIds: selectedDocuments.map((i) =>
                              parseInt(i, 10)
                            ),
                          }),
                      });
                      setSelectedDocuments([]);
                      toast({
                        title: 'Documents Signed Successfully',
                        status: 'success',
                        duration: 2000,
                      });
                    } catch (error) {
                      showErrorToast(error);
                    } finally {
                      setCurrentAction(null);
                    }
                  } else {
                    openModal();
                  }
                }}
              >
                Sign Selected
              </Button>
            )}
        </Flex>
      )}
    </>
  );
}
