import { Button } from '@chakra-ui/react';
import { BanIconFA } from 'suture-theme';
import type { BtnProps } from '../../index';

export default function RejectBtn({ isDisabled, openModal }: BtnProps) {
  return (
    <Button
      isDisabled={isDisabled}
      onClick={() => openModal('reject')}
      data-cy="reject-btn"
      leftIcon={<BanIconFA />}
      w={{ sm: '100%', md: 'auto' }}
      colorScheme="red"
    >
      Reject
    </Button>
  );
}
