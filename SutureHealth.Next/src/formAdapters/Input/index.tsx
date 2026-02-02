import {
  FormControl,
  FormErrorMessage,
  Input,
  FormLabel,
  FormControlProps,
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
  type?: React.HTMLInputTypeAttribute;
}

export default function InputForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  placeholder,
  label,
  type = 'text',
}: Props<T>) {
  const {
    field,
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <Input {...field} placeholder={placeholder} type={type} />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
