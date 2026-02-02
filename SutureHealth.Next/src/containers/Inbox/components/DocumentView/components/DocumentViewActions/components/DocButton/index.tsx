import { IconButton, IconButtonProps } from '@chakra-ui/react';

export default function DocButton(props: IconButtonProps) {
  return (
    <IconButton
      size="lg"
      border="0.5px solid"
      borderColor="blackAlpha.300"
      color="blue.500"
      colorScheme="gray"
      fontSize="24px"
      borderRadius="full"
      {...props}
    />
  );
}
