import { useState, useMemo, ReactNode } from 'react';
import {
  Flex,
  HStack,
  Modal,
  ModalOverlay,
  ModalContent,
} from '@chakra-ui/react';
import { useSelector } from '@redux/hooks';
import useDeviceHandler from '@hooks/useDeviceHandler';
import useAnnotationValidation from '@hooks/useAnnotationValidation';
import SignModal from '@components/SignModal';
import isFaceToFaceDoc from '@utils/isFaceToFaceDoc';
import ReassignModal from '@components/ReassignModal';
import RejectModal from './components/RejectionModal';
import CPOModal from './components/CPOModal';
import RejectBtn from './components/RejectBtn';
import SaveForLaterBtn from './components/SaveForLaterBtn';
import CPOBtn from './components/CPOBtn';
import SignBtn from './components/SignBtn';
import ApproveBtn from './components/ApproveBtn';
import PlusMobileBtn from './components/PlusMobileBtn';

export type ModalType = 'sign' | 'reject' | 'cpo' | 'reassign';

export interface BtnProps {
  isDisabled: boolean;
  openModal: (type: ModalType) => void;
  isFaceToFaceDoc?: boolean;
}

export default function DocumentActions() {
  const [isOpenModal, setIsOpenModal] = useState<boolean>(false);
  const [modalType, setModalType] = useState<ModalType>('sign');
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const viewedDocIds = useSelector((state) => state.inbox.viewedDocIds);
  const isFaceToFaceDocument = isFaceToFaceDoc(viewedDocument);
  const { isMobile, isTablet, isDesktop } = useDeviceHandler();
  const { isMovedTo } = useAnnotationValidation();

  const isActionBtnDisabled = useMemo(
    () =>
      Boolean(currentAction) ||
      !viewedDocument ||
      !viewedDocIds.includes(viewedDocument.sutureSignRequestId),
    [currentAction, viewedDocIds, viewedDocument]
  );

  const isSignBtnDisabled = useMemo(
    () =>
      isActionBtnDisabled ||
      viewedDocument?.isIncomplete === true ||
      isMovedTo !== null,
    [isActionBtnDisabled, isMovedTo, viewedDocument?.isIncomplete]
  );

  const isSaveForLaterBtnDisabled = useMemo(
    () => Boolean(currentAction) || !viewedDocument,
    [currentAction, viewedDocument]
  );

  const onModalClose = () => {
    setIsOpenModal(false);
  };

  const modalContent: ReactNode = useMemo(
    () =>
      ({
        signSelected: (
          <SignModal onClose={onModalClose} signMultipleDocuments />
        ),
        sign: <SignModal onClose={onModalClose} />,
        reject: <RejectModal onClose={onModalClose} />,
        cpo: <CPOModal onClose={onModalClose} />,
        reassign: <ReassignModal onClose={onModalClose} withApprove />,
      }[modalType]),
    [modalType]
  );

  const modalSize = useMemo(() => {
    switch (modalType) {
      case 'reject':
        return '3xl';
      case 'sign':
        return 'lg';
      case 'cpo':
        return '2xl';
      default:
        return 'xl';
    }
  }, [modalType]);

  const openModal = (type: ModalType) => {
    setIsOpenModal(true);
    setModalType(type);
  };

  return (
    <>
      <Modal isOpen={isOpenModal} onClose={onModalClose} size={modalSize}>
        <ModalOverlay />
        <ModalContent>{modalContent}</ModalContent>
      </Modal>
      <Flex
        bg="gray.50"
        flex={1}
        px={{ sm: 6, md: 4 }}
        align="center"
        display={
          stackAnimation
            ? 'none'
            : {
                sm: viewedDocument ? 'flex' : 'none',
                md: 'flex',
                xl: 'flex',
              }
        }
        height="var(--document-actions-height)"
      >
        {(isDesktop || isTablet) && (
          <>
            <RejectBtn isDisabled={isActionBtnDisabled} openModal={openModal} />
            <HStack ml="auto" spacing={2}>
              <SaveForLaterBtn isDisabled={isSaveForLaterBtnDisabled} />
              <CPOBtn isDisabled={isActionBtnDisabled} openModal={openModal} />
              <SignBtn
                isDisabled={isSignBtnDisabled}
                openModal={openModal}
                isFaceToFaceDoc={isFaceToFaceDocument}
              />
              <ApproveBtn
                isDisabled={isActionBtnDisabled}
                openModal={openModal}
              />
            </HStack>
          </>
        )}
        {isMobile && (
          <Flex w="100%">
            <RejectBtn isDisabled={isActionBtnDisabled} openModal={openModal} />
            <PlusMobileBtn>
              <SaveForLaterBtn isDisabled={isSaveForLaterBtnDisabled} />
              <CPOBtn
                isDisabled={isActionBtnDisabled}
                openModal={openModal}
                isFaceToFaceDoc={isFaceToFaceDocument}
              />
            </PlusMobileBtn>
            <SignBtn
              isDisabled={isSignBtnDisabled}
              openModal={openModal}
              isFaceToFaceDoc={isFaceToFaceDocument}
            />
          </Flex>
        )}
      </Flex>
    </>
  );
}
