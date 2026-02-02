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
import { DatePicker } from 'suture-theme';

interface Props<T extends FieldValues> extends UseControllerProps<T> {
  formControlProps?: FormControlProps;
  placeholder?: string;
  label?: string;
}

export default function DatePickerForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  placeholder,
  label,
}: Props<T>) {
  const {
    field: { ref, ...rest },
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <DatePicker
        {...rest}
        selected={rest.value}
        calendarIconColor="gray.700"
        placeholderText={placeholder}
        isInvalid={Boolean(error)}
      />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
