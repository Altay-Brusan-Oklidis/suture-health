import {
  ModalFooter,
  ModalBody,
  Text,
  Modal,
  ModalOverlay,
  ModalContent,
} from '@chakra-ui/react';
import { Button, ExclamationTriangleIconFA } from 'suture-theme';
import ModalTitle from '@components/ModalTitle';

interface Props {
  isOpen: boolean;
  onClose: () => void;
}

export default function PasswordErrorModal({ isOpen, onClose }: Props) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} size="md" isCentered>
      <ModalOverlay />
      <ModalContent data-cy="sign-error-modal">
        <ModalTitle
          circleColor="red.100"
          title="Invalid Password"
          icon={<ExclamationTriangleIconFA color="red.600" />}
        />
        <ModalBody ml="56px">
          <Text color="gray.500" fontSize="sm">
            The signers password has expired or was never registered. The signer
            must reset their password using the &quot;Forgot Password&quot; link
            on the login page.
          </Text>
        </ModalBody>
        <ModalFooter>
          <Button onClick={onClose} data-cy="close-sign-error-modal">
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}
