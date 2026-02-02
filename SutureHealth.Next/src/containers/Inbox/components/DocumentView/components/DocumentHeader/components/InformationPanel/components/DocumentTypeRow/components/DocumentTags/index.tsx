import { useMemo } from 'react';
import { Tag, TagLabel, TagLeftIcon, HStack } from '@chakra-ui/react';
import {
  ExclamationTriangleIconFA,
  ApprovedIconWithTooltip,
  RecycleIconFA,
} from 'suture-theme';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';
import { RequestDocumentType, AnnotationType } from '@utils/zodModels';
import { useSelector } from '@redux/hooks';

interface Props {
  document: RequestDocumentType['document'];
}

export default function DocumentTags({
  document: { approvals, isResent },
}: Props) {
  const { data: member } = useGetMemberIdentityQuery();
  const documentAnnotations = useSelector(
    (state) => state.inbox.documentAnnotations
  );

  const requiredFields = useMemo(() => {
    const requiredTypes = [AnnotationType.TextArea];

    return documentAnnotations.filter(
      (i) => !i.value && requiredTypes.includes(i.annotationTypeId)
    ).length;
  }, [documentAnnotations]);

  return (
    <HStack ml={2} whiteSpace="nowrap">
      {false && ( // TODO: change condition
        <>
          <Tag colorScheme="red">
            <TagLeftIcon as={ExclamationTriangleIconFA} />
            <TagLabel fontWeight="bold" fontSize="12px">
              STAT
            </TagLabel>
          </Tag>
          <Tag colorScheme="orange">
            <TagLeftIcon as={ExclamationTriangleIconFA} />
            <TagLabel fontWeight="bold" fontSize="12px">
              URGENT
            </TagLabel>
          </Tag>
        </>
      )}
      {approvals.length && (
        <ApprovedIconWithTooltip
          currentUserId={member?.memberId}
          approvals={approvals}
          iconProps={{
            fontSize: '20px',
            color: 'teal.400',
            'data-cy': 'header-approved-icon',
          }}
        />
      )}
      {isResent && (
        <Tag bg="cyan.100" color="cyan.500" data-cy="resent-tag">
          <TagLeftIcon as={RecycleIconFA} />
          <TagLabel fontSize="12px">RESENT</TagLabel>
        </Tag>
      )}
      {requiredFields && (
        <Tag bg="red.100" color="red.500" data-cy="required-tag">
          <TagLabel fontSize="12px">{requiredFields} Required Fields</TagLabel>
        </Tag>
      )}
    </HStack>
  );
}
