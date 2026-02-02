import { useMemo, useEffect, useState, useCallback } from 'react';
import {
  SignIconFA,
  CheckCircleIcon,
  InfoIcon,
  groupBy,
  DynamicSelectProps,
  LockIconFA,
  Badge,
} from 'suture-theme';
import {
  Button,
  ModalContent,
  ModalBody,
  ModalFooter,
  Text,
  VStack,
  useConst,
  FormLabelProps,
  Box,
  UnorderedList,
  ListItem,
  Popover,
  PopoverTrigger,
  PopoverContent,
  PopoverBody,
  useStyleConfig,
} from '@chakra-ui/react';
import { useForm, SubmitHandler, useWatch } from 'react-hook-form';
import {
  useSignDocumentsMutation,
  useRemoveDocsMutation,
  useChangeDocumentStatusMutation,
  useChangeDocumentStatusCacheMutation,
  useGetMiscSettingsQuery,
  useGetMemberIdentityQuery,
  useAssociateMeMutation,
} from '@containers/Inbox/apiReducer';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useSelector } from '@redux/hooks';
import DynamicSelect from '@formAdapters/DynamicSelect';
import ModalTitle from '@components/ModalTitle';
import Switch from '@formAdapters/Switch';
import Input from '@formAdapters/Input';
import useErrorToast from '@hooks/useErrorToast';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useMemberRole from '@hooks/useMemberRole';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { DocumentSummaryItemType } from '@utils/zodModels';
import isErrorWithMessage from '@utils/isErrorWithMessage';
import fullNameWithSuffix from '@utils/fullNameWithSuffix';
import isProblemsWithPassword from '@utils/isProblemsWithPassword';
import PasswordErrorModal from '@components/PasswordErrorModal';

interface Props {
  title: string;
  onModalClose: () => void;
  approvableDocs: DocumentSummaryItemType[];
  signableDocs: DocumentSummaryItemType[];
  reassignableDocs: DocumentSummaryItemType[];
  otherSignableDocs: DocumentSummaryItemType[];
}

interface FormValues {
  approve: boolean;
  sign: boolean;
  reassign: boolean;
  otherSign: boolean;
  reassignFrom: string[];
  signFrom?: string;
  password?: string;
}

export default function BulkModal({
  title,
  onModalClose,
  approvableDocs,
  signableDocs,
  reassignableDocs,
  otherSignableDocs,
}: Props) {
  const [errorModalVisible, setErrorModalVisible] = useState<boolean>(false);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const showErrorToast = useErrorToast();
  const [signDocuments] = useSignDocumentsMutation();
  const [removeDocs] = useRemoveDocsMutation();
  const setNextDoc = useSetNextDoc();
  const { isSigner, isCollaboratorSigner, isCollaborator, isAssistant } =
    useMemberRole();
  const {
    handleSubmit,
    control,
    formState: { isSubmitting, isValid },
    setValue,
    setError,
    reset,
    getValues,
  } = useForm<FormValues>({
    defaultValues: {
      approve: false,
      sign: false,
      reassign: false,
      otherSign: false,
      reassignFrom: [],
    },
  });
  const approve = useWatch({ control, name: 'approve' });
  const sign = useWatch({ control, name: 'sign' });
  const reassignFrom = useWatch({ control, name: 'reassignFrom' });
  const reassign = useWatch({ control, name: 'reassign' });
  const signFrom = useWatch({ control, name: 'signFrom' });
  const otherSign = useWatch({ control, name: 'otherSign' });
  const { data: settings } = useGetMiscSettingsQuery();
  const labelSwitchStyle = useConst<FormLabelProps>({
    color: 'blackAlpha.800',
    fontSize: '12px',
    fontWeight: 400,
    _groupChecked: { fontWeight: 600 },
  });
  const { data: member } = useGetMemberIdentityQuery();
  const [changeDocumentStatus] = useChangeDocumentStatusMutation();
  const [changeDocumentStatusCache] = useChangeDocumentStatusCacheMutation();
  const [associateMe] = useAssociateMeMutation();
  const tooltipStyles = useStyleConfig('Tooltip');
  const { syncAnnotations } = useUpdateAnnotations();
  const { setSelectedDocuments } = useInboxActions();

  const btnText = useMemo(() => {
    let text = '';

    if (approve) {
      text += 'Approve';
    }
    if (sign || reassign || otherSign) {
      text += approve ? ' & Sign' : 'Sign';
    }

    if (!approve && !sign && !reassign && !otherSign) {
      if (isSigner) {
        text = 'Sign';
      }
      if (isCollaboratorSigner) {
        text = 'Approve & Sign';
      }
      if (isCollaborator || isAssistant) {
        text = 'Approve';
      }
    }

    return text;
  }, [
    approve,
    sign,
    reassign,
    otherSign,
    isSigner,
    isCollaboratorSigner,
    isCollaborator,
    isAssistant,
  ]);

  const signOptions = useMemo(
    () =>
      Object.values(
        groupBy(reassignableDocs, (i) => i.signer.memberId.toString())
      ).reduce((acc: DynamicSelectProps['options'], docs) => {
        const doc = docs[0]!;

        return [
          ...acc,
          {
            avatar: `${doc.signer.firstName} ${doc.signer.lastName}`,
            value: doc.signer.memberId.toString(),
            label: fullNameWithSuffix(doc.signer),
            count: docs.length,
          },
        ];
      }, []),
    [reassignableDocs]
  );

  const otherSignOptions = useMemo(
    () =>
      Object.values(
        groupBy(otherSignableDocs, (i) => i.signer.memberId.toString())
      ).reduce((acc: DynamicSelectProps['options'], docs) => {
        const doc = docs[0]!;

        return [
          ...acc,
          {
            avatar: `${doc.signer.firstName} ${doc.signer.lastName}`,
            value: doc.signer.memberId.toString(),
            label: fullNameWithSuffix(doc.signer),
            count: docs.length,
          },
        ];
      }, []),
    [otherSignableDocs]
  );

  const resetSignFrom = useCallback(() => {
    setValue('otherSign', false);

    if (otherSignOptions.length > 1) {
      setValue('signFrom', '');
    }
  }, [otherSignOptions.length, setValue]);

  const resetReassignFrom = useCallback(() => {
    setValue('reassign', false);

    if (signOptions.length > 1) {
      setValue('reassignFrom', []);
    }
  }, [signOptions.length, setValue]);

  useEffect(() => {
    if (!reassign && reassignFrom.length && signOptions.length > 1) {
      setValue('reassign', true);
      resetSignFrom();
    }
  }, [reassign, reassignFrom, resetSignFrom, signOptions.length, setValue]);

  useEffect(() => {
    if (!otherSign && signFrom && otherSignOptions.length > 1) {
      setValue('approve', false);
      setValue('sign', false);
      resetReassignFrom();
      setValue('otherSign', true);
    }
  }, [
    otherSign,
    signFrom,
    resetReassignFrom,
    otherSignOptions.length,
    setValue,
  ]);

  const selectedReassignDocs = useMemo(
    () =>
      reassignableDocs.filter((i) =>
        reassignFrom.includes(i.signer.memberId.toString())
      ),
    [reassignableDocs, reassignFrom]
  );

  const selectedOtherSignDocs = useMemo(
    () =>
      otherSignableDocs.filter(
        (i) => signFrom === i.signer.memberId.toString()
      ),
    [otherSignableDocs, signFrom]
  );
  const selectedSigner = useMemo(
    () =>
      otherSignableDocs.find((i) => i.signer.memberId.toString() === signFrom)
        ?.signer,
    [otherSignableDocs, signFrom]
  );

  const approvedDocs = useMemo(() => {
    const approvedDocsTemp = approvableDocs.filter(
      (i) =>
        !selectedReassignDocs.find(
          (doc) => doc.sutureSignRequestId === i.sutureSignRequestId
        )
    );

    if (approvedDocsTemp.length === 0) {
      setValue('approve', false);
    }

    return approvedDocsTemp;
  }, [approvableDocs, selectedReassignDocs, setValue]);

  useEffect(() => {
    reset({
      ...getValues(),
      approve: Boolean(approvableDocs.length),
      sign: Boolean(signableDocs.length),
    });
  }, [approvableDocs, signableDocs, reassignableDocs, reset, getValues]);

  const switchLabel = (label: string, count: number) => (
    <Box display="flex" alignItems="center">
      {label}
      {count > 0 && (
        <Text
          ml="8px"
          color="gray.500"
          fontWeight={700}
          _groupChecked={{
            color: 'blackAlpha.800',
          }}
        >
          {count}
        </Text>
      )}
    </Box>
  );

  const canApprove = useMemo(
    () =>
      Boolean(
        approvableDocs.length > 0 &&
          (signableDocs.length ||
            reassignableDocs.length ||
            otherSignableDocs.length)
      ),
    [approvableDocs, signableDocs, reassignableDocs, otherSignableDocs]
  );

  const canSign = useMemo(
    () =>
      Boolean(
        signableDocs.length > 0 &&
          (approvableDocs.length ||
            reassignableDocs.length ||
            otherSignableDocs.length)
      ),
    [approvableDocs, signableDocs, otherSignableDocs]
  );

  const canReassign = useMemo(
    () => (isSigner || isCollaboratorSigner) && reassignableDocs.length > 0,
    [reassignableDocs]
  );

  const canOtherSign = otherSignableDocs.length > 0;

  useEffect(() => {
    const availableActions = [
      { name: 'approve', value: canApprove },
      { name: 'sign', value: canSign },
      { name: 'reassign', value: canReassign },
      { name: 'otherSign', value: canOtherSign },
    ].filter(({ value }) => value);

    if (availableActions.length === 1 && availableActions[0]) {
      reset({
        ...getValues(),
        [availableActions[0].name]: true,
      });
    }
  }, [
    approvableDocs,
    signableDocs,
    reassignableDocs,
    otherSignableDocs,
    reset,
    getValues,
  ]);

  useEffect(() => {
    if (signOptions?.length === 1 && signOptions[0]?.value)
      setValue('reassignFrom', [signOptions[0].value]);
  }, [signOptions, setValue]);

  useEffect(() => {
    if (otherSignOptions?.length === 1 && otherSignOptions[0]?.value)
      setValue('signFrom', otherSignOptions[0].value);
  }, [otherSignOptions, setValue]);

  const onSubmit: SubmitHandler<FormValues> = async (values) => {
    try {
      const removableDocs: DocumentSummaryItemType[] = [];

      await syncAnnotations(false);

      if (values.approve) {
        await changeDocumentStatus({
          status: 'approve',
          requestIds: approvedDocs.map((i) => i.sutureSignRequestId),
          approver: member,
        }).unwrap();
        setSelectedDocuments(
          selectedDocuments.filter(
            (i) =>
              !approvedDocs.find(
                (doc) => doc.sutureSignRequestId.toString() === i
              )
          )
        );
      }
      if (values.sign) {
        removableDocs.push(...signableDocs);
        await signDocuments({
          ids: signableDocs.map((i) => i.sutureSignRequestId),
          signerPassword: '',
        }).unwrap();
        setSelectedDocuments(
          selectedDocuments.filter(
            (i) =>
              !signableDocs.find(
                (doc) => doc.sutureSignRequestId.toString() === i
              )
          )
        );
      }
      if (values.reassign) {
        const ids = selectedReassignDocs.map((i) => i.sutureSignRequestId);

        removableDocs.push(...selectedReassignDocs);
        await associateMe(ids).unwrap();
        await signDocuments({
          ids,
          signerPassword: '',
        }).unwrap();
        setSelectedDocuments(
          selectedDocuments.filter(
            (i) => !ids.find((docId) => docId.toString() === i)
          )
        );
      }
      if (values.otherSign) {
        removableDocs.push(...selectedOtherSignDocs);
        await signDocuments({
          ids: selectedOtherSignDocs.map((i) => i.sutureSignRequestId),
          signerPassword: values.password!,
        }).unwrap();
        setSelectedDocuments(
          selectedDocuments.filter(
            (i) =>
              !selectedOtherSignDocs.find(
                (doc) => doc.sutureSignRequestId.toString() === i
              )
          )
        );
      }

      onModalClose();
      setNextDoc({
        selectedDocuments: removableDocs.map((i) => i.toString()),
        animation: {
          type: 'success',
          documentId: viewedDocument!.sutureSignRequestId,
        },
        callback: () => {
          removeDocs({
            documentIds: removableDocs.map((i) => i.sutureSignRequestId),
          });
          changeDocumentStatusCache({
            status: 'approve',
            requestIds: approvedDocs.map((i) => i.sutureSignRequestId),
            approver: member,
          });
        },
      });
    } catch (error) {
      if (isErrorWithMessage(error)) {
        if (isProblemsWithPassword(error)) {
          setErrorModalVisible(true);
        } else {
          setError('password', {
            type: 'server',
            message: 'Incorrect Password.',
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
      <ModalContent data-cy="sign-all-modal">
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalTitle
            circleColor="cyan.500"
            title={title}
            icon={
              isAssistant || isCollaborator ? (
                <CheckCircleIcon checkColor="cyan.500" fontSize="24px" />
              ) : (
                <SignIconFA fontSize="24px" />
              )
            }
          />
          <ModalBody pb={0}>
            <Box display="flex" alignItems="center">
              <Text color="gray.500" fontSize="14px ">
                What makes documents eligible?
              </Text>
              <Popover>
                <PopoverTrigger>
                  <Box ml="8px" cursor="pointer" role="button" tabIndex={0}>
                    <InfoIcon />
                  </Box>
                </PopoverTrigger>
                <PopoverContent border="none">
                  <PopoverBody sx={tooltipStyles}>
                    <Box>
                      {(isAssistant ||
                        isCollaborator ||
                        isCollaboratorSigner) && (
                        <>
                          <Text fontWeight="bold">Approve Eligible</Text>
                          <UnorderedList
                            pl="8px"
                            mb={
                              settings?.allowOtherToSignFromScreen ? '24px' : 0
                            }
                          >
                            <ListItem>{`Viewed for ${
                              (settings?.documentViewDuration || 2000) / 1000
                            } seconds`}</ListItem>
                          </UnorderedList>
                        </>
                      )}
                      {(isSigner ||
                        isCollaboratorSigner ||
                        settings?.allowOtherToSignFromScreen) && (
                        <>
                          <Text fontWeight="bold">Sign Eligible</Text>
                          <UnorderedList pl="8px">
                            <ListItem>{`Viewed for ${
                              (settings?.documentViewDuration || 2000) / 1000
                            } seconds`}</ListItem>
                            <ListItem>Required fields completed</ListItem>
                          </UnorderedList>
                        </>
                      )}
                    </Box>
                  </PopoverBody>
                </PopoverContent>
              </Popover>
            </Box>
            <VStack spacing="14px" mt="16px">
              {canApprove && (
                <Switch
                  name="approve"
                  control={control}
                  label={switchLabel('Approve', approvedDocs.length)}
                  formLabelProps={labelSwitchStyle}
                  onCustomOnChange={(e) => {
                    if (e.target.checked) {
                      resetSignFrom();
                    }
                  }}
                  isDisabled={approvedDocs.length === 0}
                />
              )}
              {canSign && (
                <Switch
                  name="sign"
                  control={control}
                  label={switchLabel('Sign', signableDocs.length)}
                  formLabelProps={labelSwitchStyle}
                  onCustomOnChange={(e) => {
                    if (e.target.checked) {
                      resetSignFrom();
                    }
                  }}
                />
              )}
              {canReassign && (
                <Box
                  display="inline-flex"
                  alignItems="center"
                  alignSelf="baseline"
                  whiteSpace="nowrap"
                >
                  <Switch
                    name="reassign"
                    control={control}
                    label="Reassign from"
                    formLabelProps={labelSwitchStyle}
                    onCustomOnChange={(e) => {
                      if (e.target.checked) {
                        resetSignFrom();
                      } else {
                        resetReassignFrom();
                      }
                    }}
                  />
                  <DynamicSelect
                    control={control}
                    name="reassignFrom"
                    options={signOptions}
                    placeholder="Select Signer(s)"
                    isMulti
                    size="xs"
                  />
                  <Text
                    ml="8px"
                    color="blackAlpha.800"
                    fontSize="12px"
                    fontWeight={reassign ? 600 : 400}
                  >
                    and sign
                  </Text>
                  <Text
                    ml="8px"
                    color={reassign ? 'blackAlpha.800' : 'gray.400'}
                    fontWeight={700}
                    fontSize="12px"
                  >
                    {selectedReassignDocs.length ||
                      signOptions.reduce(
                        (acc, cur) => acc + (cur.count || 0),
                        0
                      )}
                  </Text>
                </Box>
              )}
            </VStack>
            {canOtherSign && (
              <>
                {(approvableDocs.length > 0 ||
                  signableDocs.length > 0 ||
                  reassignableDocs.length > 0) && (
                  <Box
                    opacity={0.2}
                    display="flex"
                    my="24px"
                    alignItems="center"
                  >
                    <Box width="100%" height="1px" bg="black" />
                    <Text
                      mx="8px"
                      fontSize="14px"
                      fontWeight={500}
                      lineHeight="14px"
                    >
                      or
                    </Text>
                    <Box width="100%" height="1px" bg="black" />
                  </Box>
                )}
                <Box
                  display="inline-flex"
                  alignItems="center"
                  whiteSpace="nowrap"
                >
                  <Switch
                    name="otherSign"
                    control={control}
                    label="Have"
                    formLabelProps={labelSwitchStyle}
                    onCustomOnChange={(e) => {
                      if (e.target.checked) {
                        setValue('approve', false);
                        setValue('sign', false);
                        resetReassignFrom();
                        document.getElementsByName('signFrom')[0]?.click();
                      } else {
                        resetSignFrom();
                      }
                    }}
                  />
                  <DynamicSelect
                    control={control}
                    name="signFrom"
                    options={otherSignOptions}
                    placeholder="Select a Signer"
                    isMulti={false}
                    size="xs"
                    maxLabelWidth="200px"
                  />
                  <Text
                    ml="8px"
                    color="gray.800"
                    fontSize="12px"
                    fontWeight={otherSign ? 600 : 400}
                  >
                    Sign From Here
                  </Text>
                  {selectedOtherSignDocs.length > 0 && (
                    <Text
                      ml="8px"
                      color="blackAlpha.800"
                      fontWeight={700}
                      fontSize="12px"
                    >
                      {selectedOtherSignDocs.length}
                    </Text>
                  )}
                  <LockIconFA ml="8px" color="gray.400" />
                </Box>
                {otherSign && selectedSigner && (
                  <>
                    <Text mt="12px" color="gray.400" fontWeight={500}>
                      {`I, ${fullNameWithSuffix(
                        selectedSigner
                      )}, certify that I have reviewed and
                        understood the document(s) being signed`}
                    </Text>
                    <Input
                      name="password"
                      control={control}
                      type="password"
                      formControlProps={{ mt: '8px' }}
                      placeholder="Enter Password"
                      rules={{
                        required: {
                          value: true,
                          message: 'Required',
                        },
                      }}
                    />
                  </>
                )}
              </>
            )}
          </ModalBody>
          <ModalFooter>
            <Button
              variant="ghost"
              mr={3}
              colorScheme="gray"
              color="gray.900"
              onClick={onModalClose}
              data-cy="close-sign-all-modal"
            >
              Cancel
            </Button>
            <Button
              isLoading={isSubmitting}
              isDisabled={
                isSubmitting ||
                !isValid ||
                (!approve && !sign && !reassign && !otherSign) ||
                (reassign && reassignFrom.length === 0) ||
                (otherSign && !signFrom)
              }
              data-cy="submit-sign-all"
              type="submit"
              leftIcon={
                sign || reassign || otherSign ? (
                  <SignIconFA />
                ) : (
                  <CheckCircleIcon checkColor="blue.500" />
                )
              }
            >
              {btnText}
              <Badge
                count={
                  ((approve && approvedDocs.length) || 0) +
                  ((sign && signableDocs.length) || 0) +
                  ((reassign && selectedReassignDocs.length) || 0) +
                  ((otherSign && selectedOtherSignDocs.length) || 0)
                }
                showZero
                isEllipse
                ml="8px"
                isActive
                activeColors={{ bg: 'white', color: 'blue.500' }}
              />
            </Button>
          </ModalFooter>
        </form>
      </ModalContent>
    </>
  );
}
