import { useCallback, useEffect, useState } from 'react';
import {
  ModalContent,
  ModalBody,
  ModalFooter,
  Button,
  Box,
  Text,
  Tag,
  Avatar,
  Skeleton,
  Stack,
  Collapse,
} from '@chakra-ui/react';
import {
  ClockSolidIconFA,
  ChevronDownIconFA,
  ChevronUpIconFA,
  TruncatedText,
} from 'suture-theme';
import dayjs from 'dayjs';
import { Virtuoso } from 'react-virtuoso';
import { useSelector } from '@redux/hooks';
import ModalTitle from '@components/ModalTitle';
import {
  useLazyGetDocumentHistoryQuery,
  TaskHistory,
} from '@containers/Inbox/apiReducer';
import DocumentType from '../../../DocumentType';

interface Props {
  onClose: () => void;
}

interface FooterProps {
  context?: {
    isLoading: boolean;
  };
}

const Footer = ({ context }: FooterProps) =>
  context?.isLoading ? (
    <Stack spacing="4px" mt="4px">
      {Array(6)
        .fill(null)
        .map((_, index) => (
          <Skeleton height="50px" key={index} />
        ))}
    </Stack>
  ) : (
    <></>
  );

export default function HistoryModal({ onClose }: Props) {
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument)!;
  const [page, setPage] = useState(0);
  const [accordionIds, setAccordionIds] = useState<number[]>([]);
  const [taskHistory, setTaskHistory] = useState<TaskHistory[]>([]);
  const [getDocumentHistory, { data: historyData, status }] =
    useLazyGetDocumentHistoryQuery();

  useEffect(() => {
    if (historyData?.taskHistory) {
      setTaskHistory((prev) => [
        ...new Map(
          [...prev, ...historyData.taskHistory].map((i) => [i.taskId, i])
        ).values(),
      ]);
    }
  }, [historyData]);

  useEffect(() => {
    getDocumentHistory({
      page,
      requestId: viewedDocument.sutureSignRequestId,
    });
  }, [page, viewedDocument]);

  const handleEndReached = () => {
    if (
      (historyData?.taskHistory && historyData.taskHistory.length) ||
      taskHistory.length === 50
    ) {
      setPage((prevPage) => prevPage + 1);
    }
  };

  const getAdditionalInfoTag = useCallback((actionId: number): string => {
    switch (actionId) {
      case 528:
        return 'Signature Submitted By';
      case 555:
      case 557:
        return 'Asisted By';
      case 1001:
        return 'Saved By';
      case 1002:
      case 1004:
        return 'Returned By';
      default:
        return '';
    }
  }, []);

  return (
    <ModalContent>
      <ModalTitle
        circleColor="cyan.500"
        title={
          <Box
            display="flex"
            justifyContent="space-between"
            width="100%"
            alignItems="center"
            as="span"
          >
            <Text as="span">Document History</Text>
            <Text fontSize="12px" color="blackAlpha.900" as="span">
              {`DocID: ${viewedDocument.sutureSignRequestId}`}
            </Text>
          </Box>
        }
        icon={<ClockSolidIconFA />}
      />
      <ModalBody>
        <Box display="flex" justifyContent="space-between">
          <DocumentType
            templateType={viewedDocument.template.templateType}
            color="gray.800"
            fontSize="18px"
            fontWeight={600}
          />
          {historyData?.requestStatus && (
            <Tag>
              {`${historyData.requestStatus} ${
                historyData.requestStatus === 'Pending' ? 'for' : 'in'
              } ${historyData.age} days`}
            </Tag>
          )}
        </Box>
        <Box position="relative" mt="24px">
          <Virtuoso
            context={{ isLoading: status === 'pending' }}
            style={{ height: 400 }}
            data={taskHistory}
            endReached={handleEndReached}
            components={{ Footer }}
            itemContent={(index, historyItem) => (
              <Box display="flex" mt={index !== 0 ? '4px' : 0}>
                <Box
                  color="blackAlpha.700"
                  fontSize="12px"
                  textAlign="right"
                  display="flex"
                  flexDirection="column"
                  justifyContent="center"
                  minW="68px"
                  whiteSpace="nowrap"
                >
                  <Text fontWeight={700}>
                    {dayjs(historyItem.createDate).format('MM/DD/YYYY')}
                  </Text>
                  <Text>{dayjs(historyItem.createDate).format('hh:mm A')}</Text>
                </Box>
                <Box
                  display="flex"
                  alignItems="center"
                  justifyContent="center"
                  mx="32px"
                  position="relative"
                >
                  <Box
                    width="12px"
                    height="12px"
                    borderRadius="full"
                    backgroundColor="teal.400"
                    zIndex={1}
                  />
                  <>
                    {index === 0 && (
                      <Box
                        position="absolute"
                        top={0}
                        borderWidth="0 3px 3px 0"
                        display="inline-block"
                        padding="3px"
                        borderColor="gray.400"
                        transform="rotate(-135deg)"
                        zIndex={1}
                      />
                    )}
                    <Box
                      width="2px"
                      position="absolute"
                      top="-2px"
                      bottom={0}
                      borderLeftWidth="2px"
                      borderLeftColor="gray.400"
                      borderLeftStyle="dashed"
                    />
                  </>
                </Box>
                <Box display="flex" flexDirection="column" width="100%">
                  <Box
                    border="unset"
                    onClick={() => {
                      if (historyItem.additionalInfoValue) {
                        setAccordionIds((prev) =>
                          prev.includes(historyItem.taskId)
                            ? prev.filter((i) => i !== historyItem.taskId)
                            : [...prev, historyItem.taskId]
                        );
                      }
                    }}
                    cursor={
                      historyItem.additionalInfoValue ? 'pointer' : 'default'
                    }
                    backgroundColor="gray.100"
                    borderRadius="5px"
                    _hover={{
                      ackgroundColor: 'gray.100',
                    }}
                    py="6px"
                    pl="6px"
                    pr="14px"
                    display="flex"
                  >
                    <Box
                      maxWidth="156px"
                      width="100%"
                      color="blackAlpha.800"
                      mr="70px"
                      textAlign="left"
                    >
                      <Text fontSize="16px" fontWeight={700}>
                        {historyItem.actionText}
                      </Text>
                      <Text fontSize="12px">
                        {historyItem.actionDetailDateTime
                          ? dayjs(historyItem.actionDetailDateTime).format(
                              '[Till] M/D h:m A'
                            )
                          : historyItem.actionDetail}
                      </Text>
                    </Box>
                    <Box display="flex" textAlign="left" alignItems="center">
                      <Avatar
                        backgroundColor="teal.700"
                        name={`${historyItem.submitterFirstName} ${historyItem.submitterLastName}`}
                        width="30px"
                        height="30px"
                        size="sm"
                        color="white"
                        fontWeight={500}
                        mr="4px"
                      />
                      <Box>
                        <TruncatedText
                          color="blackAlpha.800"
                          fontSize="14px"
                          fontWeight={700}
                          text={`${historyItem.submitterFirstName}
                                      ${historyItem.submitterLastName},
                                      ${historyItem.submitterSuffix}`}
                          maxWidth="160px"
                        />
                        <Text color="blackAlpha.700" fontSize="12px">
                          {historyItem.organizationName}
                        </Text>
                      </Box>
                    </Box>
                    {historyItem.additionalInfoValue && (
                      <Box ml="auto" color="gray.700">
                        {accordionIds.includes(historyItem.taskId) ? (
                          <ChevronUpIconFA />
                        ) : (
                          <ChevronDownIconFA />
                        )}
                      </Box>
                    )}
                  </Box>
                  <Collapse
                    in={accordionIds.includes(historyItem.taskId)}
                    animateOpacity
                  >
                    <Box
                      borderRadius="5px"
                      backgroundColor="gray.200"
                      boxShadow="inset 0px 2px 4px rgba(0, 0, 0, 0.06)"
                      mt="2px"
                      px="16px"
                      py="12px"
                      display="flex"
                    >
                      <Box width="156px" mr="92px">
                        <Text color="blackAlpha.800" fontSize="12px">
                          {getAdditionalInfoTag(historyItem.actionId)}
                        </Text>
                      </Box>
                      <Box display="flex">
                        <Avatar
                          backgroundColor="teal.700"
                          name={historyItem.additionalInfoValue}
                          size="2xs"
                          fontSize="6px !important"
                          color="white"
                          fontWeight={500}
                          mr="4px"
                        />
                        <Box>
                          <TruncatedText
                            color="blackAlpha.800"
                            fontSize="12px"
                            text={historyItem.additionalInfoValue}
                            maxWidth="160px"
                          />
                        </Box>
                      </Box>
                    </Box>
                  </Collapse>
                </Box>
              </Box>
            )}
          />
        </Box>
      </ModalBody>
      <ModalFooter>
        <Button onClick={onClose} size="sm">
          Cancel
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}
