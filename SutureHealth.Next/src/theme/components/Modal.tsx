// eslint-disable-next-line
import { modalAnatomy as parts } from '@chakra-ui/anatomy';
import { createMultiStyleConfigHelpers } from '@chakra-ui/react';

const { definePartsStyle, defineMultiStyleConfig } =
  createMultiStyleConfigHelpers(parts.keys);

const baseStyle = definePartsStyle({
  footer: {
    px: 6,
    py: 6,
  },
});

export default defineMultiStyleConfig({
  baseStyle,
});
