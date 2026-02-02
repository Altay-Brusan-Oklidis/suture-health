import { Box } from '@chakra-ui/react';
import Link from 'next/link';

export default function Logo() {
  return (
    <Box mx={5} alignSelf="center">
      <Link href="/">
        <img
          src="https://www.suturesign.com/hs-fs/hubfs/SutureHealth-Logo-BL-600.png?width=800&height=169&name=SutureHealth-Logo-BL-600.png"
          width="200px"
          height="41.26px"
          placeholder="blur"
        />
      </Link>
    </Box>
  );
}
