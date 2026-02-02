import { Flex, SkeletonText } from '@chakra-ui/react';

export default function Skeleton() {
  return (
    <Flex direction="column" w="100%" py={1} pl={4} pr={1}>
      <Flex direction="row" mb={1.5} alignItems="center">
        <SkeletonText noOfLines={1} width="80px" />
        <SkeletonText noOfLines={1} ml={1} width="70px" />
        <SkeletonText noOfLines={1} ml="auto" width="120px" />
      </Flex>
      <Flex mb={1}>
        <SkeletonText noOfLines={1} width="130px" />
        <SkeletonText noOfLines={1} ml="auto" width="90px" />
      </Flex>
      <Flex mb={1}>
        <SkeletonText noOfLines={1} width="50px" />
      </Flex>
    </Flex>
  );
}
