import { ReactElement } from 'react';
import { Box, Text } from '@chakra-ui/react';

interface Props {
  title: string;
  description: string;
  icon?: ReactElement;
  extraContent?: ReactElement;
}

export default function TourContent({
  title,
  description,
  icon,
  extraContent,
}: Props) {
  return (
    <Box display="flex" flexDirection="column">
      <Box display="flex">
        {icon && <Box mr="24px">{icon}</Box>}
        <Box display="flex" flexDirection="column">
          <Text fontWeight={700} fontSize="16px" color="black">
            {title}
          </Text>
          <Text fontSize="16px" color="black" mt="8px">
            {description}
          </Text>
        </Box>
      </Box>
      {extraContent && <Box mt="12px">{extraContent}</Box>}
    </Box>
  );
}
