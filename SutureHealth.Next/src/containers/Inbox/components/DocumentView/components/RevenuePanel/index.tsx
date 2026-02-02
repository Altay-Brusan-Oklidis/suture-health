import { Flex, Text } from '@chakra-ui/react';
import { useSelector } from '@redux/hooks';
import styles from './styles';

export default function RevenuePanel() {
  const rvu = useSelector((state) => state.inbox.rvuAmount);
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);

  return (
    <Flex
      direction="row"
      alignItems="center"
      bg="gray.50"
      px={2}
      minHeight="var(--document-revenue-header-height)"
      color="gray.600"
      display={stackAnimation ? 'none' : 'flex'}
    >
      {styles}
      <Text pr={1} fontSize="sm">
        Potential Signature Revenue
      </Text>
      <Text fontWeight="bold">{` $${rvu.value} (${rvu.unit} RVU)`}</Text>
    </Flex>
  );
}
