import { Box, HStack } from '@chakra-ui/react';
import { TooltipRenderProps } from 'react-joyride';
import Close from './components/Close';
import Arrow from './components/Arrow';

export default function Tooltip({
  continuous,
  index,
  step,
  backProps,
  closeProps,
  primaryProps,
  tooltipProps,
  size,
}: TooltipRenderProps) {
  return (
    <Box
      {...tooltipProps}
      maxW="600px"
      p="16px"
      background="white"
      display="flex"
      flexDirection="column"
      borderRadius="5px"
    >
      <Close onClick={closeProps.onClick} />
      {step.content}
      <Box display="flex" mt="12px" alignSelf="center">
        {continuous && (
          <>
            <Arrow {...backProps} isDisabled={index === 0} aria-label="prev" />
            <HStack spacing="6px" mx="12px">
              {Array(size)
                .fill(undefined)
                .map((_, slideIndex) => (
                  <Box
                    key={slideIndex}
                    width="30px"
                    height="3px"
                    background="blue.500"
                    opacity={index === slideIndex ? 1 : 0.5}
                    data-cy="tour-step"
                    className={`${
                      index === slideIndex ? 'active-tour-step' : 'tour-step'
                    }`}
                  />
                ))}
            </HStack>
            <Arrow
              {...primaryProps}
              isDisabled={index === size - 1}
              inverted
              aria-label="next"
            />
          </>
        )}
      </Box>
    </Box>
  );
}
