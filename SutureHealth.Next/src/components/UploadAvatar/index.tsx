import { useState } from 'react';
import {
  Avatar,
  Flex,
  Box,
  AvatarProps,
  StyleProps,
  Tooltip,
  TooltipProps,
} from '@chakra-ui/react';
import { CameraIcon, PenIconFA } from 'suture-theme';

interface Props extends AvatarProps {
  badgeSize: StyleProps['width'];
  badgePosition?: {
    bottom?: StyleProps['bottom'];
    right?: StyleProps['right'];
  };
  topSegmentStyle?: StyleProps;
  defaulTooltipLabel?: TooltipProps['label'];
  addTooltipLabel?: TooltipProps['label'];
  editTooltipLabel?: TooltipProps['label'];
}

export default function UploadAvatar({
  badgeSize,
  badgePosition,
  topSegmentStyle,
  defaulTooltipLabel,
  addTooltipLabel,
  editTooltipLabel,
  ...rest
}: Props) {
  const [tooltipVisible, setTooltipVisible] = useState<boolean>(false);

  return (
    <Tooltip
      label={
        rest.src ? editTooltipLabel : defaulTooltipLabel || addTooltipLabel
      }
      openDelay={defaulTooltipLabel ? 0 : 3000}
      isOpen={defaulTooltipLabel ? tooltipVisible : undefined}
      closeOnScroll
      closeOnPointerDown
      onClose={() => setTooltipVisible(false)}
    >
      <Avatar
        color="white"
        backgroundColor="gray.500"
        size="xl"
        cursor="pointer"
        onClick={() => {
          if (defaulTooltipLabel) {
            setTooltipVisible((prev) => !prev);
          }
        }}
        {...rest}
      >
        <Box
          position="absolute"
          overflow="hidden"
          borderRadius={rest.borderRadius || 'full'}
          inset={0}
        >
          <Flex
            color="white"
            fontSize="12px"
            h="22px"
            bg="blue.500"
            position="absolute"
            top={0}
            left={0}
            right={0}
            alignItems="center"
            justifyContent="center"
            textTransform="none"
            fontWeight={400}
            {...topSegmentStyle}
          >
            Add
          </Flex>
        </Box>
        <Flex
          w={badgeSize}
          h={badgeSize}
          position="absolute"
          bg="white"
          alignItems="center"
          justifyContent="center"
          filter="drop-shadow(0px 4px 8px rgba(23, 25, 35, 0.12))"
          bottom={0}
          right={0}
          borderRadius="full"
          {...badgePosition}
        >
          {rest.src ? (
            <PenIconFA color="gray.500" fontSize={`calc(${badgeSize} / 2)`} />
          ) : (
            <CameraIcon color="blue.500" fontSize={`calc(${badgeSize} / 2)`} />
          )}
        </Flex>
      </Avatar>
    </Tooltip>
  );
}
