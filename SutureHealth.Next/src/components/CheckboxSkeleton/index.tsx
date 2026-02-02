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
          <Flex key={index}>
            <Skeleton width={4} height={4} mr={2} />
            <Skeleton width="100%" height={4} />
          </Flex>
        ))}
    </>
  );
}
