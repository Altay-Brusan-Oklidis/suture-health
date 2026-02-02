import { useEffect, useMemo } from 'react';
import {
  ModalFooter,
  ModalBody,
  Text,
  Stack,
  Box,
  Flex,
  useToast,
  useConst,
} from '@chakra-ui/react';
import { Button, PlusIcon, Toast } from 'suture-theme';
import { useForm, SubmitHandler, useWatch } from 'react-hook-form';
import { useSelector } from '@redux/hooks';
import toOptionFormat, { Option } from '@utils/toOptionFormat';
import DatePicker from '@formAdapters/DatePicker';
import Input from '@formAdapters/Input';
import Select from '@formAdapters/Select';
import Radio from '@formAdapters/Radio';
import Switch from '@formAdapters/Switch';
import Checkbox from '@formAdapters/Checkbox';
import Textarea from '@formAdapters/Textarea';
import NumberInput from '@formAdapters/NumberInput';
import ModalTitle from '@components/ModalTitle';
import useDocumentPermissions from '@hooks/useDocumentPermissions';
import {
  useGetCPOTypesQuery,
  useGetOrganizationsQuery,
  useSubmitCpoDataMutation,
  useGetMemberIdentityQuery,
  useSignDocumentMutation,
  useRemoveDocsMutation,
  useGetMiscSettingsQuery,
} from '@containers/Inbox/apiReducer';
import isErrorWithMessage from '@utils/isErrorWithMessage';
import useErrorToast from '@hooks/useErrorToast';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useMemberRole from '@hooks/useMemberRole';
import fullNameWithSuffix from '@utils/fullNameWithSuffix';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import useApproveAction from '@hooks/useApproveAction';

interface Props {
  signMultipleDocuments?: boolean;
  onClose: () => void;
}

interface FormValues {
  date: Date;
  patient: string;
  organizationId: string;
  entryTypeId: string;
  cpoTypeIds: string[];
  comments: string;
  minutes: number;
  signatureEnabled: boolean;
  password: string;
}

export default function CPOModal({ onClose }: Props) {
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const {
    setError,
    handleSubmit,
    control,
    setValue,
    formState: { isSubmitting },
    trigger,
    getValues,
  } = useForm<FormValues>({
    mode: 'onChange',
    defaultValues: {
      date: new Date(),
      patient: `${viewedDocument?.patient.firstName} ${viewedDocument?.patient.lastName}`,
      password: '',
    },
  });
  const signatureEnabledValue = useWatch({ control, name: 'signatureEnabled' });
  const { data: orgs } = useGetOrganizationsQuery();
  const { data, isLoading } = useGetCPOTypesQuery();
  const [cpoSubmit] = useSubmitCpoDataMutation();
  const [signDocument] = useSignDocumentMutation();
  const [removeDocs] = useRemoveDocsMutation();
  const { syncAnnotations } = useUpdateAnnotations();
  const approve = useApproveAction();
  const selectedOrganizationId = useSelector(
    (state) => state.mainLayout.selectedOrganizationId
  );
  const signer = fullNameWithSuffix(viewedDocument!.signer);
  const toast = useToast({ render: Toast });
  const { canSign } = useDocumentPermissions();
  const entryOptions = useConst([
    { value: '1', label: 'Home Health' },
    { value: '2', label: 'Hospice' },
  ]);
  const cpoTypeOptions = useMemo(() => {
    if (!isLoading && data) {
      return toOptionFormat(
        data,
        'cpoTypeId',
        (i) => `${i.description} (${i.minutes * data.length} min)`
      );
    }

    return [];
  }, [data, isLoading]);
  const showErrorToast = useErrorToast();
  const setNextDoc = useSetNextDoc();
  const { data: member } = useGetMemberIdentityQuery();
  const { data: settings } = useGetMiscSettingsQuery();
  const { isSigner } = useMemberRole();

  useEffect(() => {
    if (viewedDocument) {
      setValue(
        'entryTypeId',
        entryOptions.find(
          (i) => i.label === viewedDocument.template.templateType.category
        )?.value || ''
      );
    }
  }, [viewedDocument, entryOptions, setValue]);

  useEffect(() => {
    if (selectedOrganizationId) {
      setValue('organizationId', selectedOrganizationId);
    }
  }, [selectedOrganizationId, setValue]);

  const onSubmit: SubmitHandler<FormValues> = async ({
    password,
    date,
    organizationId,
    entryTypeId,
    cpoTypeIds,
    comments,
    signatureEnabled,
  }) => {
    try {
      syncAnnotations(true);

      await cpoSubmit({
        documentId: viewedDocument!.sutureSignRequestId,
        data: {
          date,
          organizationId,
          patientId: viewedDocument!.patient.patientId,
          entryTypeId,
          cpoTypeIds,
          comments,
        },
      }).unwrap();

      if (canSign || (!canSign && signatureEnabled)) {
        await signDocument({
          id: viewedDocument!.sutureSignRequestId,
          signerPassword: password || '',
        }).unwrap();

        toast({
          title: 'Document Signed Successfully',
          status: 'success',
          duration: 2000,
        });

        setNextDoc({
          animation: {
            type: 'success',
            documentId: viewedDocument!.sutureSignRequestId,
          },
          callback: () =>
            removeDocs({
              documentIds: [viewedDocument!.sutureSignRequestId],
            }),
        });
      } else if (!canSign && !signatureEnabled && member) {
        await approve();
      }
      onClose();
    } catch (error) {
      if (isErrorWithMessage(error)) {
        setError('password', {
          type: 'server',
          message: error.data.signerPassword?.join(''),
        });
      } else {
        showErrorToast(error);
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} data-cy="cpo-modal">
      <ModalTitle
        circleColor="cyan.500"
        title="Record Your CPO Time"
        icon={<PlusIcon color="white" fontSize="24px" />}
      />
      <ModalBody ml="56px">
        <Stack spacing={6}>
          <DatePicker
            name="date"
            control={control}
            formControlProps={{ width: 'xs' }}
            label="Date"
          />
          <Input
            name="patient"
            control={control}
            rules={{
              required: {
                value: true,
                message: 'Required',
              },
            }}
            formControlProps={{
              width: 'xs',
              isReadOnly: true,
            }}
            label="Patient"
            placeholder="Patient Name, or MRN"
          />
          <Box width="xs">
            <Select
              name="organizationId"
              control={control}
              rules={{
                required: {
                  value: true,
                  message: 'Required',
                },
              }}
              label="Location"
              placeholder="Select Location"
              options={orgs?.organizationOptions as Option[]}
              formControlProps={{ mb: 2 }}
            />
            <Radio
              name="entryTypeId"
              control={control}
              rules={{ required: { value: true, message: 'Required' } }}
              options={entryOptions}
            />
          </Box>
          <Checkbox
            name="cpoTypeIds"
            control={control}
            customOnChange={(value) => {
              if (!isLoading && data) {
                setValue(
                  'minutes',
                  data.reduce<number>(
                    (acc, cur) =>
                      value.includes(cur.cpoTypeId.toString())
                        ? acc + cur.minutes * data.length
                        : acc,
                    0
                  ),
                  { shouldValidate: true }
                );
              }
            }}
            label="Task Performed"
            rules={{
              required: {
                value: true,
                message: 'Required',
              },
            }}
            formControlProps={{ mb: 4 }}
            options={cpoTypeOptions}
          />
          <Textarea
            name="comments"
            control={control}
            placeholder="Add supporting information here..."
            label="Comments"
            formControlProps={{ width: 'xs' }}
          />
          <Flex direction="column">
            <NumberInput
              name="minutes"
              control={control}
              placeholder="Minutes"
              numberInputProps={{ min: 0 }}
              rules={{
                required: {
                  value: true,
                  message: 'Required',
                },
              }}
              formControlProps={{ mb: 4, width: 'xs' }}
              label="Time"
            />
            {!canSign &&
              settings?.allowOtherToSignFromScreen &&
              !viewedDocument?.isIncomplete && (
                <>
                  <Switch
                    name="signatureEnabled"
                    control={control}
                    formControlProps={{ width: 'xs' }}
                    label={`${signer} Password`}
                    formLabelProps={{
                      color: signatureEnabledValue ? 'gray.700' : 'gray.400',
                    }}
                    onCustomOnChange={() => trigger('password')}
                    rules={{
                      validate: (value) => {
                        if (isSigner && !value) {
                          return 'Required';
                        }

                        return true;
                      },
                    }}
                  />
                  <Flex direction="column" ml={4} width="sm">
                    <Text
                      color={signatureEnabledValue ? 'gray.400' : 'gray.300'}
                      fontSize="sm"
                      fontWeight="medium"
                      mb={2}
                      mt={2}
                    >
                      {`I, ${signer}, certify that I have reviewed and understood the document being signed.`}
                    </Text>
                    <Input
                      name="password"
                      control={control}
                      placeholder="Enter Password"
                      type="password"
                      formControlProps={{ isDisabled: !signatureEnabledValue }}
                      rules={{
                        validate: (value) => {
                          if (getValues('signatureEnabled') && !value)
                            return 'Required';

                          return true;
                        },
                      }}
                      shouldUnregister={signatureEnabledValue}
                    />
                  </Flex>
                </>
              )}
          </Flex>
        </Stack>
      </ModalBody>
      <ModalFooter>
        <Button
          variant="outline"
          mr={3}
          onClick={onClose}
          colorScheme="gray"
          color="gray.900"
          data-cy="cancel-cpo"
        >
          Cancel
        </Button>
        <Button
          type="submit"
          isDisabled={isSubmitting || viewedDocument === undefined}
          isLoading={isSubmitting}
          data-cy="submit-cpo"
        >
          {!viewedDocument?.isIncomplete &&
          (canSign || isSigner || signatureEnabledValue)
            ? 'Sign w/ CPO'
            : '+ Approve with CPO'}
        </Button>
      </ModalFooter>
    </form>
  );
}
