import { FormControl, FormLabel, FormControlProps } from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';
import dayjs from 'dayjs';
import { DateRangePicker } from 'suture-theme';

interface Props<T extends FieldValues>
  extends Omit<UseControllerProps<T>, 'name'> {
  startDateName: UseControllerProps<T>['name'];
  endDateName: UseControllerProps<T>['name'];
  formControlProps?: FormControlProps;
  label?: string;
}

export default function DateRangePickerForm<T extends FieldValues>({
  startDateName,
  endDateName,
  control,
  rules,
  formControlProps,
  label,
}: Props<T>) {
  const { field: startDateField } = useController({
    name: startDateName,
    control,
    rules,
  });
  const { field: endDateField } = useController({
    name: endDateName,
    control,
    rules,
  });

  return (
    <FormControl {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <DateRangePicker
        startDate={
          startDateField.value ? new Date(startDateField.value) : undefined
        }
        endDate={endDateField.value ? new Date(endDateField.value) : undefined}
        onStartDateChange={(val) => {
          startDateField.onChange(val ? dayjs(val).format() : val);
        }}
        onEndDateChange={(val) => {
          endDateField.onChange(val ? dayjs(val).format() : val);
        }}
      />
    </FormControl>
  );
}
