import { ReactElement, useMemo } from 'react';
import {
  AccordionButton,
  AccordionItem,
  AccordionPanel,
  Divider,
  Button,
  HStack,
  Tag,
  TagLabel,
  Box,
  Text,
} from '@chakra-ui/react';
import {
  ChevronDownIconFA,
  ChevronUpIconFA,
  CloseIconFA,
  TruncatedText,
} from 'suture-theme';
import { useFormContext, useWatch } from 'react-hook-form';
import type { FilterData } from '@containers/Inbox';
import DateRangePicker from '@formAdapters/DateRangePicker';
import Checkbox, { Option } from '@formAdapters/Checkbox';

interface CommonProps {
  label: string;
  withSeeMore?: boolean;
  onSeeMoreClick?: (page: number) => void;
}

interface DatePickerProps extends CommonProps {
  startDateName: keyof FilterData;
  endDateName: keyof FilterData;
  icon: ReactElement;
  name?: never;
  options?: never;
  allOptions?: never;
}

interface CheckboxProps extends CommonProps {
  startDateName?: never;
  endDateName?: never;
  icon?: never;
  name: keyof FilterData;
  options?: Option[];
  allOptions?: Option[];
}

type Props = DatePickerProps | CheckboxProps;

export default function FilterAccordion({
  label,
  name,
  options,
  allOptions,
  startDateName,
  endDateName,
  icon,
  withSeeMore,
  onSeeMoreClick,
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

  const isSelectAllDisabled = useMemo(
    () =>
      (firstValue as string[])?.length > 0 &&
      options?.every((i) =>
        typeof i.value === 'string'
          ? (firstValue as string[])?.find((val) => val === i.value)
          : i.value.every((option) =>
              (firstValue as string[])?.includes(option)
            )
      ),
    [firstValue, options]
  );

  if (name && (!options || options.length === 0)) return null;

  return (
    <>
      <Divider my="18px" />
      <AccordionItem mb="18px" border="none">
        {({ isExpanded }) => (
          <>
            <Box display="flex" alignItems="center">
              {!name ? (
                <>
                  <AccordionButton display="none" />
                  <Text color="gray.700" fontWeight="semibold">
                    {label}
                  </Text>
                  <Box ml="12px" color="gray.500" display="inline-flex">
                    {icon}
                  </Box>
                </>
              ) : (
                <AccordionButton
                  color="gray.700"
                  fontWeight="semibold"
                  p={0}
                  _hover={{ bg: 'none' }}
                >
                  {label}
                  <Box ml="12px">
                    {isExpanded ? <ChevronUpIconFA /> : <ChevronDownIconFA />}
                  </Box>
                </AccordionButton>
              )}
              <HStack ml="auto" onClick={(e) => e.preventDefault()}>
                {name && (
                  <Button
                    size="xs"
                    variant="ghost"
                    onClick={() => {
                      setValue(
                        name,
                        options?.flatMap((i) =>
                          Array.isArray(i.value)
                            ? i.value.map((val) => val.toString())
                            : i.value.toString()
                        )
                      );
                    }}
                    isDisabled={isSelectAllDisabled}
                  >
                    Select All
                  </Button>
                )}
                <Button
                  size="xs"
                  variant="ghost"
                  isDisabled={
                    name
                      ? !firstValue || (firstValue as string[]).length === 0
                      : !firstValue || !secondValue
                  }
                  onClick={() => {
                    if (name) {
                      setValue(name, []);
                    } else if (startDateName && endDateName) {
                      setValue(startDateName as keyof FilterData, undefined);
                      setValue(endDateName as keyof FilterData, undefined);
                    }
                  }}
                >
                  Clear All
                </Button>
              </HStack>
            </Box>
            <AccordionPanel p={0} pt={name ? '16px' : 0}>
              {Boolean(selectedOptions.length) && (
                <Box
                  mb="16px"
                  display="flex"
                  flexWrap="wrap"
                  rowGap="4px"
                  columnGap="4px"
                >
                  {selectedOptions.map((i) => (
                    <Tag key={'options' in i ? i.label : i.value}>
                      <TagLabel>
                        <TruncatedText text={i.label} maxW="130px" />
                      </TagLabel>
                      <Box
                        width="12px"
                        height="12px"
                        cursor="pointer"
                        display="flex"
                        ml="4px"
                        alignItems="center"
                        justifyContent="center"
                        onClick={() =>
                          setValue(
                            name!,
                            (firstValue! as unknown as string[]).filter((val) =>
                              typeof i.value === 'string'
                                ? val !== i.value
                                : !i.value?.includes(val)
                            )
                          )
                        }
                      >
                        <CloseIconFA fontSize="10px" />
                      </Box>
                    </Tag>
                  ))}
                </Box>
              )}
              {startDateName && endDateName && (
                <DateRangePicker
                  control={control}
                  startDateName={startDateName}
                  endDateName={endDateName}
                  formControlProps={{ mt: '8px' }}
                />
              )}
              {name && (
                <Checkbox
                  name={name}
                  control={control}
                  options={options}
                  fontSize="16px"
                  lineHeight="24px"
                  withHoverEffect
                  borderColor="gray.500"
                  withOnlyBtn
                  _checked={{
                    borderColor: 'gray.700',
                  }}
                  truncateLabel
                  withSeeMore={withSeeMore}
                  onSeeMoreClick={onSeeMoreClick}
                  __css={{
                    '.chakra-checkbox__control[data-checked], .chakra-checkbox__control[data-indeterminate]':
                      {
                        bg: 'none',
                        color: 'gray.700',
                        borderColor: 'gray.700',
                        _hover: {
                          bg: 'gray.200',
                          borderColor: 'gray.700',
                        },
                      },
                  }}
                />
              )}
            </AccordionPanel>
          </>
        )}
      </AccordionItem>
    </>
  );
}
