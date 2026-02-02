import { useState } from 'react';
import { AccountAuditMessage } from '@components/admin/identity/audit/types';

const useAccountAuditMessage = () => {
  const [userName, setUserName] = useState<string>('');
  const [auditEvent, setAuditEvent] = useState<string>('');
  const [timestamp, setTimestamp] = useState<Date>(new Date());

  const data: AccountAuditMessage = { userName, auditEvent, timestamp };

  return { data, setUserName, setAuditEvent, setTimestamp };
};

export default useAccountAuditMessage;
