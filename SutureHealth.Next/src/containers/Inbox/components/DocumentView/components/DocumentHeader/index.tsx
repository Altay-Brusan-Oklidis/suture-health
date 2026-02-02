import { useMemo } from 'react';
import { Flex, SkeletonCircle, SkeletonText } from '@chakra-ui/react';
import isSavedForLaterDoc from '@utils/isSavedForLaterDoc';
import { useSelector } from '@redux/hooks';
import InformationPanel from './components/InformationPanel';
import SaveForLaterHeader from './components/SaveForLaterHeader';

type HeaderProps = {
  documentId: number;
};

const DocumentViewSkeleton = () => (
  <Flex
    direction="row"
    h="var(--document-information-header-height)"
    alignItems="center"
    justifyContent="center"
    w="100%"
    px="12px"
    pb={2}
  >
    <Flex direction="row" mr="auto" w="50%">
      <SkeletonText noOfLines={3} mr="auto" width="100%" />
      <SkeletonText noOfLines={3} pl={8} ml="auto" width="100%" />
    </Flex>
    <Flex direction="column" ml="auto" mr="auto" w="45%">
      <SkeletonText noOfLines={1} w="25%" ml="auto" pb={2} />
      <SkeletonCircle size="10" ml="auto" />
    </Flex>
  </Flex>
);

export default function DocumentHeader({ documentId }: HeaderProps) {
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const document = useMemo(
    () => documentsObj[documentId],
    [documentsObj, documentId]
  );

  if (!document || document === 'loading') return <DocumentViewSkeleton />;

  return (
    <Flex
      boxShadow="inset 0px -1px 0px #DEE2E6"
      w="full"
      direction="column"
      top={0}
      position="sticky"
      minHeight="60px"
      zIndex={2} // shouldn't be less than 2, the datepicker icon has zIndex = 1
      display={stackAnimation ? 'none' : 'flex'}
    >
      {document && <InformationPanel document={document} />}
      {document.document.memberBacklog && (
        <SaveForLaterHeader
          date={
            document.document.memberBacklog.closedAt
              ? null
              : document.document.memberBacklog.expiresAt
          }
          isSaved={isSavedForLaterDoc(document.document)}
          documentId={document.document.sutureSignRequestId as number}
        />
      )}
    </Flex>
  );
}
