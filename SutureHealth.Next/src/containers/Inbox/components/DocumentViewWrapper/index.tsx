import { ReactNode } from 'react';
import { Flex } from '@chakra-ui/react';
import { useSelector } from '@redux/hooks';

interface Props {
  children: ReactNode;
}

export default function DocumentViewWrapper({ children }: Props) {
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);

  return (
    <Flex
      position={{ sm: 'fixed', md: 'static' }}
      top={0}
      left={viewedDocument ? 0 : '100%'}
      transition="left .3s"
      zIndex={{ sm: 2, xl: 1 }}
      w={{ sm: '100vw', md: '100%' }}
      h={{ sm: 'var(--doc-height)', md: 'auto' }}
      flexDirection="column"
    >
      {children}
    </Flex>
  );
}
