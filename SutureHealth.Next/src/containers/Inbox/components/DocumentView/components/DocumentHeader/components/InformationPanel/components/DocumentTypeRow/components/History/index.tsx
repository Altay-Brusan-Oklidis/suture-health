import {
  IconButton,
  useDisclosure,
  Modal,
  ModalOverlay,
} from '@chakra-ui/react';
import { ClockIconFA } from 'suture-theme';
import HistoryModal from './components/HistoryModal';

export default function History() {
  const { isOpen, onClose, onOpen } = useDisclosure();

  return (
    <>
      <IconButton
        aria-label="history"
        icon={<ClockIconFA />}
        ml="4px"
        variant="ghost"
        size="xs"
        isRound
        onClick={onOpen}
      />
      <Modal
        isOpen={isOpen}
        onClose={onClose}
        size="2xl"
        autoFocus={false}
        isCentered
      >
        <ModalOverlay />
        <HistoryModal onClose={onClose} />
      </Modal>
    </>
  );
}
