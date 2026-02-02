import { IconButton, Tooltip } from '@chakra-ui/react';

type ButtonProps = {
  ariaLabel: string;
  onMouseUp: () => void;
  onMouseDown: () => void;
  tooltip?: string;
  icon: React.ReactElement;
  isActive?: boolean;
};

const AnnotationMenuButton = ({
  ariaLabel,
  onMouseUp,
  tooltip,
  icon,
  isActive = false,
  onMouseDown,
}: ButtonProps) => (
  <Tooltip
    hasArrow
    label={tooltip}
    bg="black"
    color="whiteAlpha.900"
    placement="right-end"
  >
    <IconButton
      variant="ghost"
      w="42px"
      h="48px"
      mx="6.5px"
      _hover={{ bg: 'blue.500' }}
      _active={{ bg: 'blue.500' }}
      aria-label={ariaLabel}
      icon={icon}
      isActive={isActive}
      onMouseDown={() => onMouseDown()}
      onMouseUp={() => onMouseUp()}
      // onDragStart={() => handleDragStart()}
      // onClick={() => onMouseUp()}
    />
  </Tooltip>
);

export default AnnotationMenuButton;
