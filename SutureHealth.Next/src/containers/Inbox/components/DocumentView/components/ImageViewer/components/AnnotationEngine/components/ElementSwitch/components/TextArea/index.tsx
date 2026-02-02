import { Flex, Textarea } from '@chakra-ui/react';
import type { AnnotationElementProps } from '../../index';

const TextAreaAnnotation = ({ value, onChange }: AnnotationElementProps) => (
  <Flex w="100%" h="100%">
    <Textarea
      zIndex={3}
      onChange={(e) => onChange(e.target.value)}
      p="4px"
      fontSize="12px"
      lineHeight="16px"
      textColor="rgba(0, 0, 0, 0.6)"
      backgroundColor={
        value ? 'rgba(250, 240, 137, .3)' : 'rgba(194, 44, 39, .2)'
      }
      w="100%"
      minH="100%"
      variant="unstyled"
      placeholder="Enter Text"
      defaultValue={value as string}
      borderRadius={0}
      resize="none"
    />
  </Flex>
);

export default TextAreaAnnotation;
