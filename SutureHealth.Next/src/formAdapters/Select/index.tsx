import {
  FormControl,
  FormErrorMessage,
  FormLabel,
  FormControlProps,
  Select,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';

type Option = {
  value: string | number;
  label: string;
};

interface Props<T extends FieldValues> extends UseControllerProps<T> {
  formControlProps?: FormControlProps;
  placeholder?: string;
  label?: string;
  options: Option[];
  onCustomOnChange?: (value: Option['value']) => void;
}

export default function SelectForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  placeholder,
  label,
  options,
  onCustomOnChange,
}: Props<T>) {
  const {
    field,
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <Select
        {...field}
        placeholder={placeholder}
        onChange={(e) => {
          if (onCustomOnChange) {
            onCustomOnChange(e.target.value);
          }
          field.onChange(e);
        }}
      >
        {options.map((i) => (
          <option value={i.value} key={i.value}>
            {i.label}
          </option>
        ))}
      </Select>
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
