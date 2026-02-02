import {
  HStack,
  Modal,
  ModalBody,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Text,
  VStack,
} from '@chakra-ui/react';
import { Button } from 'suture-theme';
import { useUpdateInboxMarketingSubscriptionMutation } from '@containers/Inbox/apiReducer';
import CheckText from './components/CheckText';
import Illustration from './components/Illustration';

interface Props {
  onClose: () => void;
  isOpen: boolean;
  organizationId: number;
  logoSrc?: string;
}

export default function InboxMarketingModal({
  onClose,
  isOpen,
  organizationId,
  logoSrc,
}: Props) {
  const [updateSubscription, { isLoading: isUpdateSubscriptionLoading }] =
    useUpdateInboxMarketingSubscriptionMutation();

  const onSubscribe = async () => {
    await updateSubscription({ organizationId, active: true }).unwrap();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="5xl">
      <ModalOverlay />
      <ModalContent>
        <ModalHeader mt="8">
          <VStack alignItems="center">
            <Text fontSize="48px" fontWeight={600} color="blue.500">
              Turn Documents into Marketing
            </Text>
            <Text fontSize="20px" fontWeight={400} color="gray.500">
              Display your logo with the documents you send for signature
            </Text>
          </VStack>
        </ModalHeader>
        <ModalBody mt="4" mb="4">
          <VStack>
            <HStack mb="4">
              <Illustration logoSrc={logoSrc} />
              <VStack alignItems="left" maxWidth={250} spacing="5">
                <CheckText
                  textChunks={['Stay top of mind with', 'referral sources']}
                />
                <CheckText
                  textChunks={[
                    'Build your brand, enhancing',
                    'trust and credibility',
                  ]}
                />
                <CheckText
                  textChunks={[
                    'Make your documents',
                    'stand out from the rest',
                  ]}
                />
              </VStack>
            </HStack>
            <VStack alignItems="center">
              <Text fontSize="32px" fontWeight={700} color="gray.900">
                Only $100/month
              </Text>
              <Text fontSize="14px" fontWeight={400} color="gray.500">
                The cost will be automatically added to your monthly bill and
                you can unsubscribe at any time
              </Text>
            </VStack>
          </VStack>
        </ModalBody>
        <ModalFooter justifyContent="center" mb="8">
          <HStack spacing="4">
            <Button onClick={onClose} variant="outline">
              No, thanks
            </Button>
            <Button
              isLoading={isUpdateSubscriptionLoading}
              onClick={onSubscribe}
              variant="solid"
            >
              Yes, subscribe
            </Button>
          </HStack>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}
