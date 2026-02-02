import { Flex, Text } from '@chakra-ui/react';
import dayjs from 'dayjs';
import type { AnnotationElementProps } from '../../index';

const DateSignedAnnotation = ({ value }: AnnotationElementProps) => (
  <Flex direction="column" justifyContent="center" alignItems="center">
    <Flex
      direction="column"
      justifyContent="space-evenly"
      alignItems="left"
      p="6px"
      w="100%"
      h="100%"
      backgroundColor="rgba(42, 173, 250, .15)"
    >
      <Text fontSize="12px" lineHeight="16px" textColor="gray.600">
        {dayjs((value as string) || undefined).format('M/DD/YYYY')}
      </Text>
      <Text fontSize="11px" lineHeight="16px" textColor="gray.600">
        {dayjs((value as string) || undefined).format('HH:mm:ss')}
      </Text>
    </Flex>
  </Flex>
);

export default DateSignedAnnotation;
