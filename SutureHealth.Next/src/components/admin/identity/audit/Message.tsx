import { Box, Heading, Text } from '@chakra-ui/react';
import { useEffect } from 'react';
import useAccountAuditMessage from 'hooks/useAccountAuditMessage';
import { AccountAuditMessage as AccountAuditMessageType } from './types';

// by seperating the class into a type and hook, we create a more readable/declarative way of writing this
// I put the type in a types file and the hook in a hooks dir for the sake of the example as that is how we will be doing this for a production build

export default function AccountAuditMessage({
  userName,
  auditEvent,
  timestamp,
}: AccountAuditMessageType) {
  const { setUserName, setAuditEvent, setTimestamp } = useAccountAuditMessage();

  useEffect(() => {
    setUserName(userName);
    setAuditEvent(auditEvent);
    setTimestamp(timestamp);
  }, [auditEvent]);

  return (
    <Box>
      <Heading>{userName}</Heading>
      <Text as="span">{auditEvent}</Text>
      <Text as="span">{timestamp.toISOString()}</Text>
    </Box>
  );
}
