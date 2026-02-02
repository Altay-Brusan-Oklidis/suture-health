import { Skeleton, Flex } from '@chakra-ui/react';

interface Props {
  count: number;
}

export default function CheckboxSkeleton({ count }: Props) {
  return (
    <>
      {Array(count)
        .fill(null)
        .map((_, index) => (
          <Flex direction="column" align="normal" key={index} width="100%">
            <Skeleton width="80px" height="24px" noOfLines={1} mb={2} />
            <Skeleton width="100%" height="40px" noOfLines={1} />
          </Flex>
        ))}
    </>
  );
}
