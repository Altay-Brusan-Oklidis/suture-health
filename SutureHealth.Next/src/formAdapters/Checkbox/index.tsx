import { useCallback, useMemo, useState } from 'react';
import {
  FormControl,
  FormErrorMessage,
  FormLabel,
  FormControlProps,
  Stack,
  StackDirection,
  CheckboxProps,
  StyleProps,
  Box,
  Button,
  Text,
} from '@chakra-ui/react';
import {
  FieldValues,
  useController,
  UseControllerProps,
} from 'react-hook-form';
import {
  CheckboxGroup,
  Checkbox,
  Badge,
  TruncatedText,
  DocStatus,
  getDocStatusColor,
  InboxInIconFA,
} from 'suture-theme';
import dayjs from 'dayjs';
import CheckboxSkeleton from '@components/CheckboxSkeleton';

export type SimpleOtion = {
  value: string;
  label: string;
  badgeCount?: number;
  date?: string;
  status?: DocStatus;
  documentCount?: number;
};

type NestedOption = Omit<SimpleOtion, 'value'> & {
  value: string[];
  options: SimpleOtion[];
};

export type Option = SimpleOtion | NestedOption;

interface Props<T extends FieldValues>
  extends UseControllerProps<T>,
    Omit<CheckboxProps, 'defaultValue' | 'name'> {
  formControlProps?: FormControlProps;
  label?: string;
  options?: Option[];
  customOnChange?: (value: string[]) => void;
  skeletonCount?: number;
  direction?: StackDirection;
  withHoverEffect?: boolean;
  withOnlyBtn?: boolean;
  truncateLabel?: boolean;
  withSeeMore?: boolean;
  onSeeMoreClick?: (page: number) => void;
}

const OPTIONS_TO_SHOW = 10;
const NESTED_OPTIONS_TO_SHOW = 20;

export default function CheckboxForm<T extends FieldValues>({
  name,
  control,
  rules,
  formControlProps,
  customOnChange,
  label,
  options,
  skeletonCount = 5,
  direction = 'column',
  withHoverEffect,
  withOnlyBtn,
  truncateLabel,
  withSeeMore,
  onSeeMoreClick,
  ...checkboxRest
}: Props<T>) {
  const [optionsToShow, setOptionsToShow] = useState<number>(OPTIONS_TO_SHOW);
  const [nestedOptionsToShow, setNestedOptionsToShow] = useState<number>(
    NESTED_OPTIONS_TO_SHOW
  );
  const {
    field: { ref, ...rest },
    fieldState: { error },
  } = useController({ name, control, rules });
  const hoverStyles = useMemo(() => {
    if (withHoverEffect) {
      return {
        px: '4px',
        py: '8px',
        _hover: {
          bg: 'gray.100',
          // don't use role="group" for hover
          '.checkbox-only': {
            opacity: 1,
          },
        },
        borderRadius: '8px',
        transitionProperty: 'common',
        transitionDuration: 'normal',
      } as StyleProps;
    }

    return {};
  }, [withHoverEffect]);

  const isAllOptionsChecked = useCallback(
    (childValues: string[]) => {
      let counter = 0;

      childValues.forEach((i) => {
        if (rest.value.includes(i.toString())) {
          counter += 1;
        }
      });

      return childValues.length !== 0 && childValues.length === counter;
    },
    [rest.value]
  );

  const isIndeterminate = useCallback(
    (childValues: string[]) => {
      let counter = 0;

      childValues.forEach((i) => {
        if (rest.value.includes(i.toString())) {
          counter += 1;
        }
      });

      return counter !== 0 && counter !== childValues.length;
    },
    [rest.value]
  );

  const renderCheckbox = useCallback(
    (option: Option) => {
      const isParent = 'options' in option;
      const props: CheckboxProps = isParent
        ? {
            isChecked: isAllOptionsChecked(option.value),
            isIndeterminate: isIndeterminate(option.value),
            onChange: () => {
              if (isAllOptionsChecked(option.value)) {
                rest.onChange(
                  (rest.value as string[]).filter(
                    (val) => !option.value.includes(val)
                  )
                );
              } else {
                rest.onChange([...rest.value, ...option.value]);
              }
            },
            key: option.label,
          }
        : {
            value: option.value,
          };
      const daysDiff = dayjs().diff(dayjs(option.date), 'days');

      return (
        <Checkbox
          key={option.value.toString()}
          id={option.value.toString()}
          isInvalid={Boolean(error)}
          name={name}
          fontWeight={isParent ? 'medium' : 'normal'}
          position="relative"
          display="flex"
          alignItems="center"
          cursor="pointer"
          sx={
            truncateLabel
              ? {
                  '.chakra-checkbox__label': {
                    overflow: 'hidden',
                    ...(withOnlyBtn && { paddingRight: '52px' }), // only btn width
                  },
                }
              : undefined
          }
          {...checkboxRest}
          {...hoverStyles}
          {...props}
        >
          <Box display="flex" alignItems="center">
            {truncateLabel ? (
              <TruncatedText text={option.label} openDelay={300} />
            ) : (
              option.label
            )}
            <Badge
              count={option.badgeCount}
              ml="4px"
              isEllipse
              max={99}
              isActive={props.isChecked || rest.value?.includes(option.value)}
            />
            {option.date && (
              <Box display="flex" alignItems="center" ml="4px">
                <InboxInIconFA color="gray.400" mr="4px" />
                <Text
                  maxW="90px"
                  color={getDocStatusColor(option.status || 'None')}
                  fontSize="12px"
                  fontWeight="600"
                  whiteSpace="nowrap"
                >
                  {`${dayjs(option.date).format('MM/DD')}${
                    daysDiff > 0 ? ` (${daysDiff} days)` : ''
                  }`}
                </Text>
              </Box>
            )}
          </Box>
          {withOnlyBtn && (
            <Button
              variant="ghost"
              position="absolute"
              right={0}
              top="50%"
              transform="translateY(-50%)"
              size="sm"
              textDecoration="underline"
              opacity={0}
              transitionProperty="common"
              transitionDuration="normal"
              onClick={() => {
                rest.onChange(
                  typeof option.value === 'string'
                    ? [option.value]
                    : option.value
                );
              }}
              className="checkbox-only"
            >
              Only
            </Button>
          )}
        </Checkbox>
      );
    },
    [
      checkboxRest,
      error,
      hoverStyles,
      isAllOptionsChecked,
      isIndeterminate,
      name,
      rest,
      withOnlyBtn,
      truncateLabel,
    ]
  );

  return (
    <FormControl isInvalid={Boolean(error)} id={name} {...formControlProps}>
      {label && <FormLabel>{label}</FormLabel>}
      <CheckboxGroup
        {...rest}
        onChange={(value: string[]) => {
          if (customOnChange) {
            customOnChange(value);
          }
          rest.onChange(value);
        }}
      >
        {options ? (
          <Stack spacing={withHoverEffect ? 0 : 2} direction={direction}>
            {options
              .slice(0, withSeeMore ? optionsToShow : undefined)
              .map((i) =>
                'options' in i ? (
                  <Box display="flex" flexDirection="column" key={i.label}>
                    {renderCheckbox(i)}
                    <Box pl="20px" display="flex" flexDirection="column">
                      {i.options
                        .slice(0, withSeeMore ? nestedOptionsToShow : undefined)
                        .map((option) => renderCheckbox(option))}
                      {i.options.length > nestedOptionsToShow &&
                        withSeeMore && (
                          <Button
                            onClick={() =>
                              setNestedOptionsToShow(
                                (prev) => prev + NESTED_OPTIONS_TO_SHOW
                              )
                            }
                            my="4px"
                            variant="ghost"
                          >
                            See More
                          </Button>
                        )}
                    </Box>
                  </Box>
                ) : (
                  renderCheckbox(i)
                )
              )}
            {(options.length > optionsToShow ||
              (onSeeMoreClick && options.length >= optionsToShow)) &&
              withSeeMore && ( // onSeeMoreClick && options.length >= optionsToShow this condition for a server fetching
                <Button
                  onClick={() => {
                    if (onSeeMoreClick) {
                      onSeeMoreClick(
                        (optionsToShow + OPTIONS_TO_SHOW) / OPTIONS_TO_SHOW - 1
                      ); // page calculation, starts from zero
                    }
                    setOptionsToShow((prev) => prev + OPTIONS_TO_SHOW);
                  }}
                  variant="ghost"
                >
                  See More
                </Button>
              )}
          </Stack>
        ) : (
          <CheckboxSkeleton count={skeletonCount} />
        )}
      </CheckboxGroup>
      {error && <FormErrorMessage>{error?.message}</FormErrorMessage>}
    </FormControl>
  );
}
