import { Box } from '@chakra-ui/react';
import React from 'react';
import AccountAuditMessage from './Message';
import { AccountAuditMessage as AccountAuditMessageType } from './types';

type AuditWindowProps = {
  audit: AccountAuditMessageType[];
};

export default function AccountAuditWindow({ audit }: AuditWindowProps) {
  return (
    <Box>
      {audit.map((message: AccountAuditMessageType) => (
        <AccountAuditMessage
          key={message.timestamp.toISOString()}
          userName={message.userName}
          auditEvent={message.auditEvent}
          timestamp={message.timestamp}
        />
      ))}
    </Box>
  );
}
