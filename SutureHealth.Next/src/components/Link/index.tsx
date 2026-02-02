import NextLink from 'next/link';
import { Link, LinkProps } from '@chakra-ui/react';

export default function CustomNextLink(props: LinkProps) {
  return (
    <Link as={NextLink} {...props}>
      {props.children}
    </Link>
  );
}
