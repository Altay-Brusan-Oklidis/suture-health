import { useMemo, useEffect } from 'react';
import {
  ModalFooter,
  ModalBody,
  Stack,
  Box,
  Flex,
  useToast,
  useConst,
  Modal,
  ModalOverlay,
  ModalContent,
} from '@chakra-ui/react';
import {
  Button,
  PlusIcon,
  OptionsOrGroups,
  GroupBase,
  Toast,
} from 'suture-theme';
import { useForm, SubmitHandler } from 'react-hook-form';
import { z } from 'zod';
import toOptionFormat, { Option } from '@utils/toOptionFormat';
import DatePicker from '@formAdapters/DatePicker';
import Select from '@formAdapters/Select';
import Radio from '@formAdapters/Radio';
import Checkbox from '@formAdapters/Checkbox';
import Textarea from '@formAdapters/Textarea';
import NumberInput from '@formAdapters/NumberInput';
import AsyncAutocomplete from '@formAdapters/AsyncAutocomplete';
import ModalTitle from '@components/ModalTitle';
import debounce from '@utils/debounce';
import {
  useGetCPOTypesQuery,
  useGetOrganizationsQuery,
  useRegisterCpoDataMutation,
} from '@containers/Inbox/apiReducer';
import post from '@utils/post';
import useErrorToast from '@hooks/useErrorToast';
import { useSelector } from '@redux/hooks';

interface Props {
  onClose: () => void;
  isOpen: boolean;
}

interface FormValues {
  date: Date;
  patient: Option;
  organizationId: string;
  entryTypeId: string;
  cpoTypeIds: string[];
  comments: string;
  minutes: number;
}

const Users = z.array(
  z.object({
    patientId: z.number(),
    firstName: z.string(),
    middleName: z.string(),
    lastName: z.string(),
    suffix: z.string().nullable(),
    birthdate: z.string(),
  })
);

type UsersType = z.infer<typeof Users>;

export default function AddCPOModal({ onClose, isOpen }: Props) {
  const {
    handleSubmit,
    control,
    setValue,
    formState: { isSubmitting },
  } = useForm<FormValues>({
    defaultValues: { date: new Date() },
  });
  const orgs = useGetOrganizationsQuery();
  const { data, isLoading } = useGetCPOTypesQuery();
  const [registerCPOTime] = useRegisterCpoDataMutation();
  const toast = useToast({ render: Toast });
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
  const selectedOrganizationId = useSelector(
    (state) => state.mainLayout.selectedOrganizationId
  );

  useEffect(() => {
    if (selectedOrganizationId) {
      setValue('organizationId', selectedOrganizationId);
    }
  }, [selectedOrganizationId, setValue]);

  const onSubmit: SubmitHandler<FormValues> = async ({ patient, ...rest }) => {
    try {
      await registerCPOTime({
        url: '/inbox/cpo',
        method: 'POST',
        body: {
          patientId: patient.value,
          ...rest,
        },
      });

      toast({
        title: 'CPO Time Added Successfully',
        status: 'success',
        duration: 2000,
      });
      onClose();
    } catch (error) {
      showErrorToast(error);
    }
  };

  const loadOptions = debounce(
    async (
      inputValue: string,
      callback: (options: OptionsOrGroups<unknown, GroupBase<unknown>>) => void
    ) => {
      const res: UsersType = await post('/patients/search', {
        search: inputValue,
      });

      Users.parse(res);

      callback(
        toOptionFormat(res, 'patientId', (i) => `${i.firstName} ${i.lastName}`)
      );
    },
    500
  );

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="2xl">
      <ModalOverlay />
      <ModalContent>
        <form onSubmit={handleSubmit(onSubmit)} data-cy="add-cpo-modal">
          <ModalTitle
            circleColor="cyan.500"
            title="Record CPO Time"
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
              <AsyncAutocomplete
                name="patient"
                placeholder="Patient Name, or MRN"
                control={control}
                formControlProps={{ width: 'xs' }}
                label="Patient"
                loadOptions={loadOptions}
                rules={{
                  required: {
                    value: true,
                    message: 'Required',
                  },
                }}
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
                  options={(orgs.data?.organizationOptions as Option[]) || []}
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
              data-cy="cancel-add-cpo"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              isDisabled={isSubmitting}
              isLoading={isSubmitting}
              data-cy="submit-add-cpo"
            >
              Record CPO time
            </Button>
          </ModalFooter>
        </form>
      </ModalContent>
    </Modal>
  );
}
