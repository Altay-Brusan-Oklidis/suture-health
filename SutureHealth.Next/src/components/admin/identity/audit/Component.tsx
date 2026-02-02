import { useState, useEffect, useRef } from 'react';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Option } from '@swan-io/boxed';
import { Box, Text } from '@chakra-ui/react';
import AccountAuditWindow from './Window';
import { AccountAuditMessage } from './types';

export const AccountAudit = () => {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [audit, setAudit] = useState<AccountAuditMessage[]>([]);
  const latestAudit = useRef<AccountAuditMessage[]>(null);

  const currentAuditRef = Option.fromNullable(latestAudit.current).match({
    Some: (auditObj) => auditObj,
    None: () => [],
  });

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl(
        `https://${process.env.NEXT_PUBLIC_WEBHOST}:${process.env.NEXT_PUBLIC_WEBPORT}/identity/accounts/auditing`
      )
      .withAutomaticReconnect()
      .build();
    setAudit(currentAuditRef);
    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, [currentAuditRef]);

  // this pattern allows us to wait for the connection without a useEffect hook and gives the user a loading state message while they wait
  // while I do hate using effect hooks, I really just wanted to show you this cool pattern. Tbh that was probably fine.
  return Option.fromNullable(connection).match({
    Some: (connectionObj) => {
      connectionObj
        .start()
        .then(() => {
          console.log('Connected!');
          connectionObj.on('ReceiveMessage', (message) => {
            if (latestAudit.current != null) {
              const updatedAudits = [...latestAudit.current];
              updatedAudits.push(message);
              setAudit(updatedAudits);
            }
          });
        })
        .catch((e) => console.log('Connection failed: ', e));

      return (
        <Box>
          <AccountAuditWindow audit={audit} />
        </Box>
      );
    },
    None: () => (
      <Box>
        <Text>Loading...</Text>
      </Box>
    ),
  });
};
