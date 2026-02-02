import { useState } from 'react';
import { ModalFooter, ModalBody, Text, useToast } from '@chakra-ui/react';
import { Button, LockBoldIcon, Toast } from 'suture-theme';
import { useForm, SubmitHandler } from 'react-hook-form';
import { useSelector } from '@redux/hooks';
import Input from '@formAdapters/Input';
import ModalTitle from '@components/ModalTitle';
import isErrorWithMessage from '@utils/isErrorWithMessage';
import isProblemsWithPassword from '@utils/isProblemsWithPassword';
import {
  useSignDocumentMutation,
  useSignDocumentsMutation,
  useRemoveDocsMutation,
} from '@containers/Inbox/apiReducer';
import { useInboxActions } from '@containers/Inbox/localReducer';
import useErrorToast from '@hooks/useErrorToast';
import useSetNextDoc from '@hooks/useSetNextDoc';
import fullNameWithSuffix from '@utils/fullNameWithSuffix';
import PasswordErrorModal from '@components/PasswordErrorModal';

interface Props {
  signMultipleDocuments?: boolean;
  onClose: () => void;
}

interface FormValues {
  password: string;
}

export default function SignModal({ signMultipleDocuments, onClose }: Props) {
  const {
    setError,
    handleSubmit,
    control,
    formState: { isSubmitting, isValid },
  } = useForm<FormValues>({ defaultValues: { password: '' } });
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const signer = fullNameWithSuffix(viewedDocument!.signer);
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const toast = useToast({ render: Toast });
  const { setSelectedDocuments } = useInboxActions();
  const [signDocument] = useSignDocumentMutation();
  const [signDocuments] = useSignDocumentsMutation();
  const showErrorToast = useErrorToast();
  const [errorModalVisible, setErrorModalVisible] = useState<boolean>(false);
  const setNextDoc = useSetNextDoc();
  const [removeDocs] = useRemoveDocsMutation();

  const onSubmit: SubmitHandler<FormValues> = async (values) => {
    try {
      if (signMultipleDocuments) {
        await signDocuments({
          ids: selectedDocuments,
          signerPassword: values.password,
        }).unwrap();
      } else {
        await signDocument({
          id: viewedDocument!.sutureSignRequestId,
          signerPassword: values.password,
        }).unwrap();
      }

      toast({
        title: 'Document Signed Successfully',
        status: 'success',
        duration: 2000,
      });

      setNextDoc({
        selectedDocuments,
        animation: {
          type: 'success',
          documentId: viewedDocument!.sutureSignRequestId,
        },
        callback: () =>
          removeDocs({
            documentIds: signMultipleDocuments
              ? selectedDocuments.map((i) => parseInt(i, 10))
              : [viewedDocument!.sutureSignRequestId],
          }),
      });
      if (signMultipleDocuments) setSelectedDocuments([]);
      onClose();
    } catch (error) {
      if (isErrorWithMessage(error)) {
        if (isProblemsWithPassword(error)) {
          setErrorModalVisible(true);
        } else {
          setError('password', {
            type: 'server',
            message: 'Password is incorrect',
          });
        }
      } else {
        showErrorToast(error);
      }
    }
  };

  const closeErrorModal = () => setErrorModalVisible(false);

  return (
    <>
      <PasswordErrorModal
        isOpen={errorModalVisible}
        onClose={closeErrorModal}
      />
      <form onSubmit={handleSubmit(onSubmit)} data-cy="sign-modal">
        <ModalTitle
          circleColor="cyan.500"
          title="Signer Password Required"
          icon={<LockBoldIcon color="white" />}
        />
        <ModalBody>
          <Text color="gray.700" fontSize="sm" mb={3}>
            {`Please have ${signer}, enter his/her password below`}
          </Text>
          <Text color="gray.400" fontSize="sm" fontWeight="medium" mb={2}>
            {`I, ${signer}, certify that I have reviewed and understood the document being signed.`}
          </Text>
          <Input
            name="password"
            control={control}
            placeholder="Enter Password"
            type="password"
          />
        </ModalBody>
        <ModalFooter>
          <Button
            variant="outline"
            mr={3}
            onClick={onClose}
            colorScheme="gray"
            color="gray.900"
          >
            Cancel
          </Button>
          <Button
            type="submit"
            w="101px"
            isDisabled={isSubmitting || !isValid}
            isLoading={isSubmitting}
            data-cy="sign-submit"
          >
            Sign
          </Button>
        </ModalFooter>
      </form>
    </>
  );
}
