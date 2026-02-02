import { useMemo } from 'react';
import {
  FormControl,
  FormErrorMessage,
  FormLabel,
  FormControlProps,
  FormHelperText,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';
import { TextareaSelect, TextareaSelectProps } from 'suture-theme';

// @ts-ignore TODO: this is causing a build error
interface Props<T extends FieldValues>
  extends UseControllerProps<T>,
    Omit<TextareaSelectProps, 'value' | 'onChange' | 'name'> {
  formControlProps?: FormControlProps;
  label?: string;
  maxLength?: number;
}

export default function TextareaSelectForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  label,
  maxLength,
  ...rest
}: Props<T>) {
  const {
    field: { ref, ...fieldRest },
    fieldState: { error },
  } = useController({ name, control, rules });
  const charactersLeft = useMemo(() => {
    if (maxLength && fieldRest.value) {
      return Math.max(maxLength - fieldRest.value.length, 0);
    }

    return maxLength;
  }, [maxLength, fieldRest.value]);

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <TextareaSelect {...rest} {...fieldRest} isInvalid={Boolean(error)} />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
      {maxLength && (
        <FormHelperText textAlign="right">{`Characters Left ${charactersLeft}`}</FormHelperText>
      )}
    </FormControl>
  );
}
