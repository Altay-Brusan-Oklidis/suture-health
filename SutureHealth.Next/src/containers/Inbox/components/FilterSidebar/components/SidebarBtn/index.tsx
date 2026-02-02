import { ReactNode } from 'react';
import { Box, StyleProps, BoxProps, Text, Tooltip } from '@chakra-ui/react';
import { Badge } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import { useFormContext, useController, useWatch } from 'react-hook-form';
import type { FilterData } from '@containers/Inbox';
import { useUpdateInboxPreferencesMutation } from '@containers/Inbox/apiReducer';

interface Props extends BoxProps {
  label: string;
  icon: ReactNode;
  tooltipLabel?: string;
  badgeCount?: number;
  color?: StyleProps['color'];
  value?: FilterData['documentProcessStatus'];
}

export default function SidebarBtn({
  label,
  icon,
  tooltipLabel,
  badgeCount,
  color = 'blue.500',
  value,
  ...rest
}: Props) {
  const { control } = useFormContext<FilterData>();
  const {
    field: { value: currentValue, onChange },
  } = useController({ control, name: 'documentProcessStatus' });
  const [updateInboxPreferences] = useUpdateInboxPreferencesMutation();
  const [approvalStatus, documentType] = useWatch({
    control,
    name: ['approvalStatus', 'documentType'],
  });
  const isActive = currentValue === value;
  const isFilterSidebarOpen = useSelector(
    (state) => state.inbox.isFilterSidebarOpen
  );
  const currentColor = isActive ? color : 'gray.500';

  return (
    <Tooltip label={tooltipLabel} placement="right">
      <Box
        color={currentColor}
        borderLeftWidth="4px"
        borderColor={isActive ? currentColor : 'transparent'}
        display="flex"
        alignItems="center"
        height={isFilterSidebarOpen ? '48px' : '60px'}
        width="100%"
        cursor="pointer"
        bg={isActive ? 'gray.100' : 'unset'}
        _hover={{
          bg: 'gray.200',
        }}
        transition="background .3s, height .3s"
        onClick={() => {
          if (value) {
            onChange(value);
            updateInboxPreferences({
              approval:
                approvalStatus.length === 1 ? approvalStatus[0]! : 'all',
              documentProcess: value,
              view: documentType,
            });
          }
        }}
        {...rest}
      >
        <Box
          position="relative"
          display="flex"
          transform={`translate(${isFilterSidebarOpen ? '8px' : '16px'}, ${
            badgeCount && !isFilterSidebarOpen ? '50%' : 0
          })`}
          transition="transform .3s"
        >
          <Box display="flex" alignItems="center">
            <Box fontSize="20px" display="flex">
              {icon}
            </Box>
            <Text
              opacity={isFilterSidebarOpen ? 1 : 0}
              whiteSpace="nowrap"
              mx="16px"
              transition="opacity .3s"
              fontSize="16px"
            >
              {label}
            </Text>
            <Badge
              ml={0}
              max={99}
              isEllipse
              count={badgeCount}
              fontWeight="semibold"
              position={isFilterSidebarOpen ? 'static' : 'absolute'}
              bottom="100%"
              left={badgeCount && badgeCount > 99 ? '-1px' : '2px'}
              isActive={isActive}
              activeColors={{ color: 'white', bg: currentColor }}
              pointerEvents="none"
            />
          </Box>
        </Box>
      </Box>
    </Tooltip>
  );
}
