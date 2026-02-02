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
import { AsyncAutocomplete, GroupBase, AsyncProps } from 'suture-theme';

interface Props<T extends FieldValues> extends UseControllerProps<T> {
  formControlProps?: FormControlProps;
  label?: string;
}

export default function AsyncAutocompleteForm<
  T extends FieldValues,
  OptionProps,
  IsMulti extends boolean,
  Group extends GroupBase<OptionProps>
>({
  name,
  control,
  rules,
  formControlProps,
  label,
  ...rest
}: Props<T> & AsyncProps<OptionProps, IsMulti, Group>) {
  const {
    field: { ref, ...fieldRest },
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <AsyncAutocomplete {...fieldRest} {...rest} isInvalid={Boolean(error)} />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
