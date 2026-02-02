import { useEffect, useMemo } from 'react';
import { ModalBody, ModalFooter, useToast, VStack } from '@chakra-ui/react';
import { Button, ForwardIconFA, Toast } from 'suture-theme';
import { useForm, SubmitHandler, useWatch } from 'react-hook-form';
import { skipToken } from '@reduxjs/toolkit/dist/query';
import { useSelector } from '@redux/hooks';
import ModalTitle from '@components/ModalTitle';
import Select from '@formAdapters/Select';
import toOptionFormat, { Option } from '@utils/toOptionFormat';
import SelectSkeleton from '@components/SelectSkeleton';
import { useInboxActions } from '@containers/Inbox/localReducer';
import {
  useReassignDocumentMutation,
  useGetAssociatesQuery,
  useChangeDocumentStatusMutation,
} from '@containers/Inbox/apiReducer';
import useErrorToast from '@hooks/useErrorToast';
import useRefetchViewedDoc from '@hooks/useRefetchViewedDoc';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import fullNameWithSuffix from '@utils/fullNameWithSuffix';

interface Props {
  onClose: () => void;
  withApprove?: boolean;
}

interface FormValues {
  signerMemberId: string;
  organizationId: string;
  collaboratorMemberId?: string;
  assistantMemberId?: string;
}

interface SelectOptions {
  [key: string]: {
    officeOptions: Option[];
    collaboratorOptions: Option[];
    assistantOptions: Option[];
  };
}

export default function ReassignModal({ onClose, withApprove }: Props) {
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { data, isLoading } = useGetAssociatesQuery(
    viewedDocument?.sutureSignRequestId || skipToken
  );
  const [reassignDocument] = useReassignDocumentMutation();
  const {
    handleSubmit,
    control,
    formState: { isSubmitting, isDirty },
    setValue,
    reset,
  } = useForm<FormValues>();
  const signerMemberId = useWatch({ control, name: 'signerMemberId' });
  const organizationId = useWatch({ control, name: 'organizationId' });
  const toast = useToast({ render: Toast });
  const showErrorToast = useErrorToast();
  const refetchViewedDoc = useRefetchViewedDoc();
  const { setViewedDocument } = useInboxActions();
  const [changeDocumentStatus] = useChangeDocumentStatusMutation();
  const setNextDoc = useSetNextDoc();
  const { syncAnnotations } = useUpdateAnnotations();

  const signerOptions = useMemo(() => {
    if (!isLoading && data) {
      return toOptionFormat(
        data,
        (i) => i.signer.memberId,
        (i) => fullNameWithSuffix(i.signer)
      );
    }

    return [];
  }, [data, isLoading]);
  const options: SelectOptions | undefined = useMemo(() => {
    if (!isLoading && data) {
      const organizationFilter = (orgId?: number | string) =>
        orgId?.toString() === organizationId;

      return data.reduce(
        (acc, cur) => ({
          ...acc,
          [cur.signer.memberId]: {
            officeOptions: toOptionFormat(
              cur.organizations,
              'organizationId',
              'name'
            ),
            collaboratorOptions: toOptionFormat(
              cur.collaborators.filter((i) =>
                organizationFilter(i?.organizationId)
              ),
              'memberId',
              (i) => fullNameWithSuffix(i)
            ),
            assistantOptions: toOptionFormat(
              cur.assistants.filter((i) =>
                organizationFilter(i?.organizationId)
              ),
              'memberId',
              (i) => fullNameWithSuffix(i)
            ),
          },
        }),
        {}
      );
    }

    return undefined;
  }, [data, organizationId, isLoading]);

  useEffect(() => {
    if (viewedDocument) {
      reset({
        signerMemberId: viewedDocument.signer.memberId.toString(),
        organizationId:
          viewedDocument.signerOrganization.organizationId.toString(),
        collaboratorMemberId: viewedDocument?.collaborator
          ? viewedDocument.collaborator.memberId.toString()
          : '',
        assistantMemberId: viewedDocument?.assistant
          ? viewedDocument.assistant.memberId.toString()
          : '',
      });
    }
  }, [viewedDocument, reset]);

  const onSubmit: SubmitHandler<FormValues> = async (values) => {
    if (
      viewedDocument &&
      values.signerMemberId !== viewedDocument.signer.memberId.toString() &&
      !viewedDocument.template.templateType.signerChangeAllowed
    ) {
      toast({
        title: 'You are unable to reassign this document type.',
        description:
          'If this document was sent to you in error, reject the document.',
        status: 'error',
        duration: 8000,
      });
    } else {
      try {
        const newSigner = data!.find(
          (i) => i.signer.memberId.toString() === values.signerMemberId
        )?.signer;

        syncAnnotations(true);
        if (withApprove) {
          await changeDocumentStatus({
            status: 'approve',
            requestIds: [viewedDocument!.sutureSignRequestId],
          }).unwrap();
        }

        await reassignDocument({
          id: viewedDocument!.sutureSignRequestId,
          ...values,
          assistantMemberId: values.assistantMemberId || 0,
          collaboratorMemberId: values.collaboratorMemberId || 0,
        }).unwrap();

        if (newSigner) {
          setViewedDocument({
            ...viewedDocument!,
            signer: newSigner,
          });
        }

        refetchViewedDoc();

        if (withApprove) {
          setNextDoc({
            animation: {
              type: 'success',
              documentId: viewedDocument!.sutureSignRequestId,
            },
          });

          toast({
            title: 'Document Approved and Reassigned Successfully',
            status: 'success',
            duration: 2000,
          });
        } else {
          toast({
            title: 'Document Assigned Successfully',
            status: 'success',
            duration: 2000,
          });
        }
        onClose();
      } catch (error) {
        showErrorToast(error);
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} data-cy="forward-modal">
      <ModalTitle
        circleColor="cyan.500"
        title="Document Assignment"
        icon={<ForwardIconFA fontSize="24px" />}
      />
      <ModalBody ml="56px">
        <VStack spacing={2}>
          {options ? (
            <>
              <Select
                name="signerMemberId"
                control={control}
                label="Signer"
                options={signerOptions}
                rules={{ required: { value: true, message: 'Required' } }}
                onCustomOnChange={(value) => {
                  setValue(
                    'organizationId',
                    options[value]?.officeOptions.find(
                      (i) =>
                        i.value ===
                        viewedDocument?.signerOrganization.organizationId.toString()
                    )?.value ||
                      options[value]?.officeOptions[0]?.value ||
                      ''
                  );
                  setValue('collaboratorMemberId', '');
                  setValue('assistantMemberId', '');
                }}
              />
              <Select
                name="organizationId"
                control={control}
                label="Organization"
                options={options[signerMemberId]?.officeOptions || []}
                rules={{ required: { value: true, message: 'Required' } }}
                onCustomOnChange={() => {
                  setValue('collaboratorMemberId', '');
                  setValue('assistantMemberId', '');
                }}
              />
              <Select
                name="collaboratorMemberId"
                control={control}
                label="Collaborator"
                options={options[signerMemberId]?.collaboratorOptions || []}
                placeholder="None"
              />
              <Select
                name="assistantMemberId"
                control={control}
                label="Assistant (i.e. nurse)"
                options={options[signerMemberId]?.assistantOptions || []}
                placeholder="None"
              />
            </>
          ) : (
            <SelectSkeleton count={4} />
          )}
        </VStack>
      </ModalBody>
      <ModalFooter>
        <Button
          variant="ghost"
          mr={3}
          onClick={onClose}
          colorScheme="gray"
          data-cy="forward-cancel"
        >
          Cancel
        </Button>
        <Button
          type="submit"
          isDisabled={isSubmitting || !isDirty}
          isLoading={isSubmitting}
          data-cy="forward-submit"
        >
          {`OK${withApprove ? ' & Approve' : ''}`}
        </Button>
      </ModalFooter>
    </form>
  );
}
