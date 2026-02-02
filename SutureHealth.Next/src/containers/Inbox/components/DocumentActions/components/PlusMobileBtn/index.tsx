import {
  IconButton,
  Popover,
  PopoverTrigger,
  PopoverContent,
  PopoverBody,
  VStack,
} from '@chakra-ui/react';
import { ReactNode } from 'react';
import { PlusIconFA } from 'suture-theme';

interface Props {
  children: ReactNode;
}

export default function PlusMobileBtn({ children }: Props) {
  return (
    <Popover>
      <PopoverTrigger>
        <IconButton aria-label="more" icon={<PlusIconFA />} mx={3} isRound />
      </PopoverTrigger>
      <PopoverContent w="240px">
        <PopoverBody>
          <VStack spacing={3}>{children}</VStack>
        </PopoverBody>
      </PopoverContent>
    </Popover>
  );
}
