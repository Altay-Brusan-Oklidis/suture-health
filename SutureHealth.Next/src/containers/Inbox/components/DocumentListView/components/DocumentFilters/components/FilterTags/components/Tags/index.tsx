import { useMemo } from 'react';
import { Tag, TagLabel, Box, TagLeftIcon, As } from '@chakra-ui/react';
import { CloseIconFA, TruncatedText } from 'suture-theme';
import { useFormContext, useWatch } from 'react-hook-form';
import dayjs from 'dayjs';
import type { FilterData, FilterMenuFilters } from '@containers/Inbox';
import type { Option } from '@formAdapters/Checkbox';

interface CommonProps {
  icon?: As;
}

interface DatePickerProps extends CommonProps {
  startDateName: keyof FilterMenuFilters;
  endDateName: keyof FilterMenuFilters;
  name?: never;
  allOptions?: never;
}

interface CheckboxProps extends CommonProps {
  startDateName?: never;
  endDateName?: never;
  name: keyof FilterMenuFilters;
  allOptions?: Option[];
}

type Props = DatePickerProps | CheckboxProps;

export default function Tags({
  name,
  allOptions,
  startDateName,
  endDateName,
  icon,
}: Props) {
  const { control, setValue } = useFormContext<FilterData>();
  const [firstValue, secondValue] = useWatch({
    control,
    name: name
      ? [name]
      : ([startDateName, endDateName] as (keyof FilterData)[]),
  });

  const selectedOptions = useMemo(
    () =>
      allOptions
        ? allOptions.reduce<Option[]>((acc, cur) => {
            if ('options' in cur) {
              return cur.value.every((i) =>
                (firstValue as string[]).includes(i)
              )
                ? [...acc, cur]
                : [
                    ...acc,
                    ...cur.options.filter((i) =>
                      (firstValue as string[]).includes(i.value.toString())
                    ),
                  ];
            }

            return (firstValue as string[]).includes(cur.value.toString())
              ? [...acc, cur]
              : acc;
          }, [])
        : [],
    [firstValue, allOptions]
  );

  const renderTag = (label: string, value?: string | string[]) => (
    <Tag
      variant="outline"
      px="8px"
      height="20px"
      minHeight="20px"
      key={value?.toString() || label}
    >
      {icon && <TagLeftIcon as={icon} />}
      <TagLabel>
        <TruncatedText
          text={label}
          maxW="94px"
          fontSize="12px"
          fontWeight={500}
        />
      </TagLabel>
      <Box
        width="12px"
        height="12px"
        cursor="pointer"
        display="flex"
        ml="4px"
        alignItems="center"
        justifyContent="center"
        onClick={() => {
          if (startDateName && endDateName) {
            setValue(startDateName, undefined);
            setValue(endDateName, undefined);
          } else {
            setValue(
              name!,
              (firstValue! as unknown as string[]).filter((val) =>
                typeof value === 'string'
                  ? val !== value
                  : !value?.includes(val)
              )
            );
          }
        }}
      >
        <CloseIconFA fontSize="10px" />
      </Box>
    </Tag>
  );

  if (startDateName && endDateName && (firstValue || secondValue)) {
    const dateFormat = 'MM/DD/YYYY';
    const dates = [];

    if (firstValue) {
      dates.push(dayjs(firstValue as string).format(dateFormat));
    }
    if (secondValue) {
      dates.push(dayjs(secondValue as string).format(dateFormat));
    }

    return renderTag(dates.join('-'));
  }

  return <>{selectedOptions.map((i) => renderTag(i.label, i.value))}</>;
}
