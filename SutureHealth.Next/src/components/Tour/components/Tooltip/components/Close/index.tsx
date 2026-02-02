import { IconButton, IconButtonProps } from '@chakra-ui/react';
import { CloseIconFA } from 'suture-theme';

export default function Arrow({
  onClick,
  ...rest
}: Omit<IconButtonProps, 'aria-label'>) {
  return (
    <IconButton
      aria-label="close"
      icon={<CloseIconFA />}
      variant="ghost"
      colorScheme="gray"
      onClick={onClick}
      ml="auto"
      fontSize="11px"
      size="xs"
      mb="12px"
      data-cy="close-tour"
      {...rest}
    />
  );
}
