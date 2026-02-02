import { extendTheme } from '@chakra-ui/react';
import { theme } from 'suture-theme';
import components from './components';

const sutureTheme = extendTheme({
  ...theme,
  components: {
    ...theme.components,
    ...components,
  },
});

export default sutureTheme;
