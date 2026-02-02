import {
  FormControl,
  FormErrorMessage,
  FormLabel,
  FormControlProps,
  Stack,
  StackDirection,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';
import { RadioGroup, Radio } from 'suture-theme';

type Option = {
  value: string | number;
  label: string;
};

interface Props<T extends FieldValues> extends UseControllerProps<T> {
  formControlProps?: FormControlProps;
  label?: string;
  options: Option[];
  direction?: StackDirection;
}

export default function RadioForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  label,
  options,
  direction = 'row',
}: Props<T>) {
  const {
    field,
    fieldState: { error },
  } = useController({ name, control, rules });

  return (
    <FormControl isInvalid={Boolean(error)} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <RadioGroup {...field}>
        <Stack direction={direction}>
          {options.map((i) => (
            <Radio
              value={i.value as string}
              key={i.value}
              isInvalid={Boolean(error)}
              name={name}
            >
              {i.label}
            </Radio>
          ))}
        </Stack>
      </RadioGroup>
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
