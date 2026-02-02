import { useRef, useState } from 'react';
import { Modal, ModalOverlay, Flex, ModalContent } from '@chakra-ui/react';
import { RequestDocumentType } from '@utils/zodModels';
import ReassignModal from '@components/ReassignModal';
import DocumentTypeRow from './components/DocumentTypeRow';
import PatientRow from './components/PatientRow';
import SignerRow from './components/SignerRow';
import EffectiveSection from './components/EffectiveSection';

type Props = {
  document: RequestDocumentType;
};

export default function InformationPanel({ document }: Props) {
  const [isReassignModalOpen, setIsReassignModalOpen] =
    useState<boolean>(false);
  const ref = useRef<HTMLDivElement>(null);

  if (!document || !document.document) return <></>;

  const { patient } = document.document;

  const openReassignModal = () => setIsReassignModalOpen(true);

  const onReassignModalClose = () => setIsReassignModalOpen(false);

  return (
    <>
      <Modal
        isOpen={isReassignModalOpen}
        onClose={onReassignModalClose}
        size="lg"
        finalFocusRef={ref}
      >
        <ModalOverlay />
        <ModalContent>
          <ReassignModal onClose={onReassignModalClose} />
        </ModalContent>
      </Modal>
      <Flex
        bg="gray.50"
        h="var(--document-information-header-height)"
        border="0.8px solid"
        borderColor="gray.400"
        textOverflow="ellipsis"
        paddingTop={2}
        px={4}
        direction={{ sm: 'column', xl: 'row' }}
      >
        <Flex direction="column" flex={1} overflow="hidden">
          <DocumentTypeRow document={document} />
          <Flex mt="6px">
            <Flex direction="column" overflow="hidden">
              <PatientRow patient={patient} />
              <SignerRow
                document={document}
                openReassigndModal={openReassignModal}
              />
            </Flex>
            <EffectiveSection document={document} />
          </Flex>
        </Flex>
      </Flex>
    </>
  );
}
