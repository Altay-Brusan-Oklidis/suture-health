import {
  FormControl,
  FormErrorMessage,
  NumberInput,
  NumberInputField,
  FormLabel,
  FormControlProps,
  NumberInputProps,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';

interface Props<T extends FieldValues> extends UseControllerProps<T> {
  formControlProps?: FormControlProps;
  placeholder?: string;
  label?: string;
  numberInputProps?: NumberInputProps;
}

export default function NumberInputForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  placeholder,
  label,
  numberInputProps,
}: Props<T>) {
  const {
    field,
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <NumberInput {...field} {...numberInputProps}>
        <NumberInputField placeholder={placeholder} />
      </NumberInput>
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
