import { ChangeEvent, ReactNode } from 'react';
import {
  FormControl,
  FormErrorMessage,
  FormLabel,
  FormControlProps,
  Switch,
  FormLabelProps,
  SwitchProps,
  Box,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';

interface Props<T extends FieldValues>
  extends UseControllerProps<T>,
    Omit<SwitchProps, 'defaultValue' | 'name'> {
  formControlProps?: FormControlProps;
  formLabelProps?: FormLabelProps;
  label?: string | ReactNode;
  onCustomOnChange?: (e: ChangeEvent<HTMLInputElement>) => void;
}

export default function SwitchForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  label,
  formLabelProps,
  onCustomOnChange,
  ...rest
}: Props<T>) {
  const {
    field,
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl
      isInvalid={Boolean(error)}
      data-checked={field.value ? true : undefined}
      {...formControlProps}
    >
      <Box display="flex" alignItems="center">
        <Switch
          {...field}
          {...rest}
          isChecked={field.value}
          onChange={(e) => {
            field.onChange(e);

            if (onCustomOnChange) onCustomOnChange(e);
          }}
        />
        {label && (
          <FormLabel mb={0} ml={2} {...formLabelProps}>
            {label}
          </FormLabel>
        )}
      </Box>
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
