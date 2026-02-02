import { IconButton, IconButtonProps } from '@chakra-ui/react';
import { ArrowLeftIconFA, ArrowRightIconFA } from 'suture-theme';

interface Props extends IconButtonProps {
  inverted?: boolean;
}

export default function Arrow({ inverted, ...rest }: Props) {
  return (
    <IconButton
      icon={inverted ? <ArrowRightIconFA /> : <ArrowLeftIconFA />}
      variant="ghost"
      colorScheme="gray"
      fontSize="20px"
      data-cy={`${inverted ? 'next' : 'prev'}-tour-step`}
      {...rest}
    />
  );
}
