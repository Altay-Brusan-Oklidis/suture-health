import {
  FormControl,
  FormErrorMessage,
  FormLabel,
  FormControlProps,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';
import { DynamicSelect, DynamicSelectProps } from 'suture-theme';

interface Props<T extends FieldValues>
  extends Omit<
      DynamicSelectProps,
      'value' | 'onChange' | 'defaultValue' | 'name'
    >,
    UseControllerProps<T> {
  formControlProps?: FormControlProps;
  label?: string;
}

export default function DynamicSelectForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  label,
  ...rest
}: Props<T>) {
  const {
    field: { ref, ...fieldRest },
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <DynamicSelect {...fieldRest} {...rest} />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
