import { useBreakpointValue } from '@chakra-ui/react';

export default function useDeviceHandler() {
  const isMobile = useBreakpointValue({ sm: true, md: false, xl: false });
  const isTablet = useBreakpointValue({ sm: false, md: true, xl: false });

  return {
    isMobile,
    isTablet,
    isDesktop: isMobile === false && isTablet === false, // don't use !isMobile, the value should be undefined in ssr
  };
}
