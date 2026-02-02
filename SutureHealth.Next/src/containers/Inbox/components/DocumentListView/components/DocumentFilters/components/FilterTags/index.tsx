import { useState, useRef, useEffect } from 'react';
import { Box, Button } from '@chakra-ui/react';
import {
  ChevronDownIconFA,
  ChevronUpIconFA,
  InboxIconFA,
  SignIconFA,
  CalendarPlusIconFA,
  FileIconFA,
  FileCheckIconFA,
  UserIconFA,
  EnvelopeIconFA,
} from 'suture-theme';
import { useSelector } from '@redux/hooks';
import Tags from './components/Tags';

export default function FilterTags() {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [isVisible, setIsVisible] = useState<boolean>(false);
  const [isMoreBtnVisible, setIsMoreBtnVisible] = useState<boolean>(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const filterOptions = useSelector((state) => state.inbox.filterOptions);
  const allPatientOptions = useSelector(
    (state) => state.inbox.allPatientOptions
  );

  useEffect(() => {
    if (containerRef.current) {
      new ResizeObserver(() => {
        if (containerRef.current) {
          setIsVisible(containerRef.current.clientWidth > 0);
          setIsMoreBtnVisible(containerRef.current.scrollHeight > 20);
        }
      }).observe(containerRef.current);
    }
  }, [containerRef.current]);

  return (
    <Box
      display="flex"
      opacity={isVisible ? 1 : 0}
      mb={isVisible ? '12px' : 0}
      height={isVisible ? 'auto' : 0}
    >
      <Box
        display="flex"
        flexWrap="wrap"
        rowGap="8px"
        columnGap="4px"
        ref={containerRef}
        overflow="hidden"
        height={isOpen ? 'auto' : '20px'}
      >
        <Tags
          startDateName="dateReceivedStart"
          endDateName="dateReceivedEnd"
          icon={InboxIconFA}
        />
        <Tags
          startDateName="effectiveDateStart"
          endDateName="effectiveDateEnd"
          icon={CalendarPlusIconFA}
        />
        <Tags name="isResent" allOptions={filterOptions?.isResent} />
        <Tags
          name="templateTypeIds"
          allOptions={filterOptions?.templateTypes}
          icon={FileIconFA}
        />
        <Tags
          name="signerIds"
          allOptions={filterOptions?.signers}
          icon={SignIconFA}
        />
        <Tags
          name="collaboratorIds"
          allOptions={filterOptions?.collaborators}
          icon={FileCheckIconFA}
        />
        <Tags
          name="patientIds"
          allOptions={allPatientOptions}
          icon={UserIconFA}
        />
        <Tags
          name="submitterIds"
          allOptions={filterOptions?.senderOrganizations}
          icon={EnvelopeIconFA}
        />
      </Box>
      {isMoreBtnVisible && (
        <Button
          leftIcon={isOpen ? <ChevronUpIconFA /> : <ChevronDownIconFA />}
          onClick={() => setIsOpen((prev) => !prev)}
          variant="ghost"
          size="xs"
          height="20px"
          minW="60px"
        >
          {isOpen ? 'Less' : 'More'}
        </Button>
      )}
    </Box>
  );
}
