import { Button, useDisclosure } from '@chakra-ui/react';
import { ExchangeIconFA } from 'suture-theme';
import { useGetMiscSettingsQuery } from '@containers/Inbox/apiReducer';
import EnvSwapModal from './components/EnvSwapModal';

export default function SwitchToOldInboxBtn() {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { data: settings } = useGetMiscSettingsQuery();

  return (
    <>
      {settings?.showNewInbox ? (
        <>
          <Button
            variant="outline"
            size="xs"
            leftIcon={<ExchangeIconFA />}
            mr={2}
            onClick={() => onOpen()}
            data-cy="switch-to-old-inbox"
          >
            Switch to old inbox
          </Button>
          <EnvSwapModal isOpen={isOpen} onOpen={onOpen} onClose={onClose} />
        </>
      ) : null}
    </>
  );
}
