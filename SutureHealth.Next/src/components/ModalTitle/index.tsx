import { ReactNode } from 'react';
import { ModalHeader, Text, Flex } from '@chakra-ui/react';

interface Props {
  circleColor?: string;
  icon: ReactNode;
  title: ReactNode;
}

export default function ModalTitle({
  circleColor = 'green.100',
  icon,
  title,
}: Props) {
  return (
    <ModalHeader display="flex" pb={0} dir="row" alignItems="center">
      <Flex
        minWidth="40px"
        minHeight="40px"
        borderRadius="50%"
        mr={4}
        bg={circleColor}
        alignItems="center"
        justify="center"
        color="white"
      >
        {icon}
      </Flex>
      <Text
        fontSize="lg"
        fontWeight="medium"
        lineHeight="28px"
        color="gray.900"
        width="100%"
      >
        {title}
      </Text>
    </ModalHeader>
  );
}
