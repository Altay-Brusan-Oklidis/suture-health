import { useMemo } from 'react';
import {
  FormControl,
  FormErrorMessage,
  Textarea,
  FormLabel,
  FormControlProps,
  FormHelperText,
  TextareaProps,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';

interface Props<T extends FieldValues>
  extends UseControllerProps<T>,
    Omit<TextareaProps, 'name' | 'defaultValue'> {
  formControlProps?: FormControlProps;
  placeholder?: string;
  label?: string;
  maxLength?: number;
}

export default function TextareaForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  placeholder,
  label,
  maxLength,
  ...rest
}: Props<T>) {
  const {
    field,
    fieldState: { error },
  } = useController({ name, control, rules });
  const charactersLeft = useMemo(() => {
    if (maxLength && field.value) {
      return Math.max(maxLength - field.value.length, 0);
    }

    return maxLength;
  }, [maxLength, field.value]);

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <Textarea {...field} placeholder={placeholder} {...rest} />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
      {maxLength && (
        <FormHelperText textAlign="right">{`Characters Left ${charactersLeft}`}</FormHelperText>
      )}
    </FormControl>
  );
}
