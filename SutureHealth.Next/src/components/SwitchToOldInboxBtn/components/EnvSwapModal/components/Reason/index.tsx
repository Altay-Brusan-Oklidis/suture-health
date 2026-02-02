import { Stack, Box } from '@chakra-ui/react';
import {
  useController,
  FieldValues,
  UseControllerProps,
} from 'react-hook-form';
import { PlusIcon } from 'suture-theme';

interface Props<T extends FieldValues> extends UseControllerProps<T> {
  value: string;
}

export default function Reason<T extends FieldValues>({
  value,
  name,
  control,
}: Props<T>) {
  const { field } = useController({ name, control });

  return (
    <Stack spacing={2} mb={4}>
      <Box
        alignItems="center"
        alignSelf="self-start"
        display="flex"
        onClick={() => {
          field.onChange(field.value ? `${field.value} ${value}` : value);
        }}
        cursor="pointer"
        userSelect="none"
        data-cy={name}
      >
        <PlusIcon mr={2} fontSize="20px" color="blue.500" />
        {value}
      </Box>
    </Stack>
  );
}
