import { HStack, Text, VStack } from '@chakra-ui/react';
import { CheckCircleIcon } from 'suture-theme';

interface Props {
  textChunks: Array<string>;
}

export default function CheckText({ textChunks }: Props) {
  const lines = textChunks.map((text, index) => (
    <HStack key={index}>
      <CheckCircleIcon
        opacity={index ? 0 : 1}
        color="blue.500"
        width="17px"
        height="17px"
      />
      <Text fontSize="16px" fontWeight={400} color="gray.900">
        {text}
      </Text>
    </HStack>
  ));

  return (
    <>
      <VStack alignItems="left" spacing="0">
        {lines}
      </VStack>
    </>
  );
}
