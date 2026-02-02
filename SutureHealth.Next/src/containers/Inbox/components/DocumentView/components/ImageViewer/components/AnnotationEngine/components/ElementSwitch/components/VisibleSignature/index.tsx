import { Flex, Box } from '@chakra-ui/react';
import { SignIconFA } from 'suture-theme';

const VisibleSignatureAnnotation = () => (
  <Flex w="100%" h="100%" backgroundColor="rgba(42, 173, 250, .15)">
    <Flex
      direction="column"
      justifyContent="space-around"
      alignItems="left"
      opacity={1}
      p="8px"
      w="100%"
      h="100%"
    >
      <SignIconFA />
      <Box borderTop="1px solid #718096" w="100%" />
    </Flex>
  </Flex>
);

export default VisibleSignatureAnnotation;
