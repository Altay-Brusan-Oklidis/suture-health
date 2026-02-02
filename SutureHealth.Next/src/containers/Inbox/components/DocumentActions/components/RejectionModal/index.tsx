import {
  ModalFooter,
  ModalBody,
  Stack,
  Box,
  FormLabel,
} from '@chakra-ui/react';
import {
  Button,
  QuestionMarkIcon,
  PlusIcon,
  BanIconFA,
  Badge,
} from 'suture-theme';
import { useForm, SubmitHandler } from 'react-hook-form';
import { skipToken } from '@reduxjs/toolkit/query/react';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';
import Textarea from '@formAdapters/Textarea';
import ModalTitle from '@components/ModalTitle';
import CheckboxSkeleton from '@components/CheckboxSkeleton';
import {
  useRejectDocumentMutation,
  useRejectDocumentsMutation,
  useGetRejectReasonsQuery,
  useRemoveDocsMutation,
} from '@containers/Inbox/apiReducer';
import useErrorToast from '@hooks/useErrorToast';
import useSetNextDoc from '@hooks/useSetNextDoc';
import sortBy from '@utils/sortBy';
import { DocumentSummaryItemType } from '@utils/zodModels';

interface Props {
  rejectableDocs?: DocumentSummaryItemType[];
  onClose: () => void;
}

interface FormValues {
  reason: string;
}

export default function RejectionModal({ rejectableDocs, onClose }: Props) {
  const {
    handleSubmit,
    control,
    formState: { isSubmitting },
    setValue,
    getValues,
  } = useForm<FormValues>({});
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { data: reasonOptions, isLoading } = useGetRejectReasonsQuery(
    (viewedDocument?.sutureSignRequestId as number) ?? skipToken
  );
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const { setSelectedDocuments } = useInboxActions();
  const [rejectDocument] = useRejectDocumentMutation();
  const [rejectDocuments] = useRejectDocumentsMutation();
  const showErrorToast = useErrorToast();
  const setNextDoc = useSetNextDoc();
  const [removeDocs] = useRemoveDocsMutation();

  const onSubmit: SubmitHandler<FormValues> = async ({ reason }) => {
    try {
      if (rejectableDocs?.length) {
        await rejectDocuments({
          requestIds: rejectableDocs.map((i) => i.sutureSignRequestId),
          reason,
          rejectionReasonIds: reasonOptions
            ?.filter((i) => reason.match(i.description))
            .map((i) => i.rejectionReasonId),
        }).unwrap();
      } else {
        await rejectDocument({
          id: viewedDocument!.sutureSignRequestId,
          reason,
          rejectionReasonIds: reasonOptions
            ?.filter((i) => reason.match(i.description))
            .map((i) => i.rejectionReasonId),
        }).unwrap();
      }

      setNextDoc({
        selectedDocuments,
        animation: {
          type: 'rejected',
          documentId: viewedDocument!.sutureSignRequestId,
        },
        callback: () =>
          removeDocs({
            documentIds: rejectableDocs?.length
              ? rejectableDocs.map((i) => i.sutureSignRequestId)
              : [viewedDocument!.sutureSignRequestId],
          }),
      });
      if (rejectableDocs?.length) {
        setSelectedDocuments(
          selectedDocuments.filter(
            (i) =>
              !rejectableDocs.find(
                (doc) => doc.sutureSignRequestId.toString() === i
              )
          )
        );
      }
      onClose();
    } catch (error) {
      showErrorToast(error);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} data-cy="reject-modal">
      <ModalTitle
        circleColor="red.100"
        title={
          rejectableDocs?.length
            ? 'Specify Reason(s) for Bulk Reject'
            : 'Specify Reason(s) for Rejection'
        }
        icon={<QuestionMarkIcon />}
      />
      <ModalBody ml="56px">
        {!isLoading ? (
          <>
            <FormLabel>Select all that apply</FormLabel>
            <Stack spacing={2} mb={4} data-cy="reject-reasons">
              {sortBy(reasonOptions!, ['description'], [1]).map((i) => (
                <Box
                  key={i.description}
                  alignItems="center"
                  alignSelf="self-start"
                  display="flex"
                  onClick={() => {
                    const currentReason = getValues('reason');

                    setValue(
                      'reason',
                      currentReason
                        ? `${currentReason} ${i.description}`
                        : i.description,
                      { shouldValidate: true }
                    );
                  }}
                  cursor="pointer"
                  userSelect="none"
                >
                  <PlusIcon mr={2} fontSize="20px" color="blue.500" />
                  {i.description}
                </Box>
              ))}
            </Stack>
          </>
        ) : (
          <Stack spacing={2} mb={4}>
            <CheckboxSkeleton count={5} />
          </Stack>
        )}
        <Textarea
          name="reason"
          control={control}
          placeholder="Add comments here..."
          rules={{
            required: {
              value: true,
              message:
                'You must provide at least one rejection reason in order to reject a document',
            },
            maxLength: { value: 600, message: 'Max length exceeded' },
          }}
          maxLength={600}
          rows={4}
        />
      </ModalBody>
      <ModalFooter>
        <Button
          variant="outline"
          mr={3}
          onClick={onClose}
          colorScheme="gray"
          color="gray.900"
          data-cy="rejection-modal-close"
        >
          Cancel
        </Button>
        <Button
          type="submit"
          isDisabled={isSubmitting || isLoading}
          isLoading={isSubmitting}
          data-cy="submit-rejection"
          leftIcon={<BanIconFA />}
        >
          Reject
          {!rejectableDocs?.length ? null : (
            <Badge
              count={rejectableDocs.length}
              isEllipse
              ml="8px"
              isActive
              activeColors={{ bg: 'white', color: 'blue.500' }}
            />
          )}
        </Button>
      </ModalFooter>
    </form>
  );
}
