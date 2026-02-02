import { Flex } from '@chakra-ui/react';
import { IframeUrlValues } from '@lib/constants';

interface Props {
  pathname: IframeUrlValues;
}

export default function Iframe({ pathname }: Props) {
  const url = `${pathname}?contentOnly=true`;

  return (
    <Flex justify="center" h="calc(100vh - var(--navbar-height))">
      <iframe
        src={url}
        style={{
          width: '100%',
          height: '100%',
        }}
      />
    </Flex>
  );
}
