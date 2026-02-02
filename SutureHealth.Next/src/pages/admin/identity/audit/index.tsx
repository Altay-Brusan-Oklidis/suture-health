import { Heading } from '@chakra-ui/react';
import { AccountAudit } from '@components/admin/identity/audit/Component';

export default function Audit() {
  return (
    <>
      <Heading>Authentication Audit</Heading>
      <AccountAudit />
    </>
  );
}
