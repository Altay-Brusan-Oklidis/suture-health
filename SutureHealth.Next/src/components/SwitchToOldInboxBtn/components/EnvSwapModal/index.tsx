import {
  Modal,
  ModalOverlay,
  ModalFooter,
  ModalContent,
  ModalBody,
  Button,
  useToast,
  FormLabel,
} from '@chakra-ui/react';
import ModalTitle from '@components/ModalTitle';
import Textarea from '@formAdapters/Textarea';
import {
  useGetMemberIdentityQuery,
  useUpdateMiscUserSettingsMutation,
  useGetMiscSettingsQuery,
  useGetPrimaryOrgDetailsQuery,
} from '@containers/Inbox/apiReducer';
import { useRouter } from 'next/router';
import { SubmitHandler, useForm } from 'react-hook-form';
import { QuestionMarkIcon, Toast } from 'suture-theme';
import { mixpanelEvent } from '@utils/analytics';
import Reason from './components/Reason';

interface FormValues {
  reason: string;
}

type Props = {
  isOpen: boolean;
  onOpen: () => void;
  onClose: () => void;
};

const EnvSwapModal = ({ isOpen, onClose }: Props) => {
  const {
    handleSubmit,
    control,
    formState: { isSubmitting, isValid, isDirty },
  } = useForm<FormValues>({
    mode: 'onChange',
    defaultValues: { reason: '' },
  });
  const { data: member } = useGetMemberIdentityQuery();
  const { data: orgDetails } = useGetPrimaryOrgDetailsQuery();
  const router = useRouter();
  const { data } = useGetMiscSettingsQuery();
  const toast = useToast({ render: Toast });
  const [updateSetting] = useUpdateMiscUserSettingsMutation();

  const submit: SubmitHandler<FormValues> = async ({ reason }) => {
    /* istanbul ignore next */
    if (data) {
      await updateSetting({
        inboxPreference: 0,
      });
      mixpanelEvent('inbox-rollback', {
        userId: member?.memberId,
        role: member?.memberTypeId,
        org: orgDetails?.organizationId,
        reason,
      });

      router.push(
        `https://web.${window.location.host}/UserArea/ModifyRequest.aspx`
      );
      onClose();
    } else {
      toast({
        title: 'An error occurred',
        description: 'Please try again.',
        status: 'error',
        duration: 6000,
        isClosable: true,
      });
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose}>
      <ModalOverlay />
      <ModalContent data-cy="env-swap-modal">
        <form onSubmit={handleSubmit(submit)} data-cy="reject-modal">
          <ModalTitle
            circleColor="red.100"
            title="Specify Reason(s) for Switching to Old Inbox"
            icon={<QuestionMarkIcon />}
          />
          <ModalBody>
            <ModalBody>
              <FormLabel>Select all that apply</FormLabel>
              <Reason
                name="reason"
                value="Refresh my memory of the old inbox for comparison"
                control={control}
              />
              <Reason
                name="reason"
                value="Don't want to learn something new"
                control={control}
              />
              <Reason
                name="reason"
                value="New inbox is too busy"
                control={control}
              />
              <Reason
                name="reason"
                value="The following is not working right:"
                control={control}
              />
              <Reason
                name="reason"
                value="Can't find a document"
                control={control}
              />
              <Textarea
                name="reason"
                control={control}
                placeholder="Add comments here..."
                rules={{
                  required: {
                    value: true,
                    message:
                      'You must provide at least one reason in order to return to the old inbox experience',
                  },
                  maxLength: { value: 600, message: 'Max length exceeded' },
                }}
                maxLength={600}
                rows={4}
              />
            </ModalBody>
          </ModalBody>
          <ModalFooter>
            <Button
              variant="outline"
              mr={3}
              onClick={onClose}
              colorScheme="gray"
              color="gray.900"
              data-cy="close-env-swap-modal"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              isDisabled={isSubmitting || !isValid || !isDirty}
              isLoading={isSubmitting}
              data-cy="submit-env-swap-modal"
            >
              Confirm
            </Button>
          </ModalFooter>
        </form>
      </ModalContent>
    </Modal>
  );
};

export default EnvSwapModal;
