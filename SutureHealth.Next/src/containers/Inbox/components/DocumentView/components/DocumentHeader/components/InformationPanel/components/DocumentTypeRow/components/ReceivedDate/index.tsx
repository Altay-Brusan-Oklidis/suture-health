import { Box, Text, Tooltip } from '@chakra-ui/react';
import { getDocStatusColor } from 'suture-theme';
import dayjs from 'dayjs';
import { RequestDocumentType } from '@utils/zodModels';

interface Props {
  submittedAt: RequestDocumentType['document']['submittedAt'];
  statusDisplay: RequestDocumentType['document']['statusDisplay'];
  sutureSignRequestId: RequestDocumentType['document']['sutureSignRequestId'];
}

export default function ReceivedDate({
  submittedAt,
  statusDisplay,
  sutureSignRequestId,
}: Props) {
  const daysDiff = dayjs().diff(dayjs(submittedAt), 'days');

  return (
    <Box ml="auto" display="flex" alignItems="center" whiteSpace="nowrap">
      <Tooltip label="Received On">
        <Text
          color={getDocStatusColor(statusDisplay || 'None')}
          fontSize="10px"
          fontWeight="semibold"
          ml={2}
        >
          {`${dayjs(submittedAt).format('MM/DD')} ${
            daysDiff > 0 ? `(${daysDiff} days)` : ''
          }`}
        </Text>
      </Tooltip>
      <Text fontSize="8px" color="blackAlpha.900" ml={1} data-cy="doc-id">
        DocID: {sutureSignRequestId}
      </Text>
    </Box>
  );
}
