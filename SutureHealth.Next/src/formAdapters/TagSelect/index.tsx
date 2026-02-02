import {
  FormControl,
  FormErrorMessage,
  FormLabel,
  FormControlProps,
  FormHelperText,
} from '@chakra-ui/react';
import { useMemo } from 'react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';
import { TagSelect, GroupBase, CreatableProps } from 'suture-theme';

interface Props<T extends FieldValues> extends UseControllerProps<T> {
  formControlProps?: FormControlProps;
  label?: string;
  maxLength?: number;
  joinSymbolLength?: number;
}

export default function SelectForm<
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
  maxLength,
  joinSymbolLength = 0,
  ...rest
}: Props<T> & CreatableProps<OptionProps, IsMulti, Group>) {
  const {
    field: { ref, ...fieldRest },
    fieldState: { error },
  } = useController({ name, control, rules });
  const charactersLeft = useMemo(() => {
    if (maxLength && fieldRest.value) {
      return Math.max(
        maxLength -
          fieldRest.value.reduce(
            (acc: number, cur: { label: string; value: string }) =>
              acc + cur.value.length + joinSymbolLength,
            0
          ),
        0
      );
    }

    return maxLength;
  }, [maxLength, fieldRest.value]);

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <TagSelect {...fieldRest} {...rest} isInvalid={Boolean(error)} />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
      {maxLength && (
        <FormHelperText textAlign="right">{`Characters Left ${charactersLeft}`}</FormHelperText>
      )}
    </FormControl>
  );
}
