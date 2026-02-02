import {
  FormControl,
  FormErrorMessage,
  FormControlProps,
} from '@chakra-ui/react';
import { SwitchButton, SwitchButtonProps } from 'suture-theme';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';

interface Props<T extends FieldValues>
  extends UseControllerProps<T>,
    Omit<SwitchButtonProps, 'name' | 'defaultValue' | 'value' | 'onChange'> {
  formControlProps?: FormControlProps;
  onCustomOnChange: (value: string) => void;
}

export default function SwitchButtonForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  onCustomOnChange,
  ...rest
}: Props<T>) {
  const {
    field: { ref, ...fieldRest },
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      <SwitchButton
        {...rest}
        {...fieldRest}
        width="100%"
        onChange={(value) => {
          fieldRest.onChange(value);

          if (onCustomOnChange) {
            onCustomOnChange(value);
          }
        }}
      />
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
