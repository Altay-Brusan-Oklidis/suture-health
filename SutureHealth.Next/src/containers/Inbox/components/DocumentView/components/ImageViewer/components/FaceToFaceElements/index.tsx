import { useEffect, useMemo, useState } from 'react';
import {
  Stack,
  Box,
  useToast,
  Modal,
  ModalOverlay,
  ModalContent,
} from '@chakra-ui/react';
import { useForm, SubmitHandler, useWatch } from 'react-hook-form';
import { orderSort, Toast } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import DatePicker from '@formAdapters/DatePicker';
import TagSelect from '@formAdapters/TagSelect';
import CheckBox from '@formAdapters/Checkbox';
import TextareaSelect from '@formAdapters/TextareaSelect';
import Textarea from '@formAdapters/Textarea';
import dayjs from 'dayjs';
import { RequestDocumentType } from '@utils/zodModels';
import { FormValues, useInboxActions } from '@containers/Inbox/localReducer';
import {
  useUpdateFaceToFaceMetadataMutation,
  useGetFaceToFaceMetadataQuery,
  useSignDocumentMutation,
  useRemoveDocsMutation,
} from '@containers/Inbox/apiReducer';
import useErrorToast from '@hooks/useErrorToast';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useApproveAction from '@hooks/useApproveAction';
import useDocumentPermissions from '@hooks/useDocumentPermissions';
import SignModal from '@components/SignModal';

interface Props {
  doc: RequestDocumentType;
}

export const FACE_FORM_ID = 'face-to-face-form';

export default function FaceToFaceElements({ doc }: Props) {
  const [modalVisible, setModalVisible] = useState<boolean>(false);
  const {
    handleSubmit,
    control,
    formState: { isValid, isDirty },
  } = useForm<FormValues>({
    mode: 'onChange',
    defaultValues:
      doc?.metadata && typeof doc.metadata !== 'string'
        ? {
            ...doc.metadata,
            skilledServices: doc.metadata.skilledServices.map((i) =>
              i.toLocaleLowerCase()
            ), // the server transmits keys in upper case
            encounterDate: dayjs(doc.metadata.encounterDate).toDate(),
            medicalCondition: doc.metadata.medicalCondition
              .split('. ')
              .map((i) => ({ label: i, value: i })),
          }
        : { encounterDate: new Date() },
  });
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const { data, isLoading } = useGetFaceToFaceMetadataQuery();
  const [updateFaceToFaceMetadata] = useUpdateFaceToFaceMetadataMutation();
  const [signDocument] = useSignDocumentMutation();
  const [removeDocs] = useRemoveDocsMutation();
  const { setCurrentAction, setDocumentAt, setCurrentF2FPageStatus } =
    useInboxActions();
  const currentF2FPageStatus = useSelector(
    (state) => state.inbox.currentF2FPageStatus
  );
  const showErrorToast = useErrorToast();
  const setNextDoc = useSetNextDoc();
  const { canSign } = useDocumentPermissions();
  const allFields = useWatch({ control }) as FormValues;
  const toast = useToast({ render: Toast });
  const approve = useApproveAction();
  const clonedDoc = useMemo(() => structuredClone(doc), [doc]);

  const setCurrentDocument = (
    document: RequestDocumentType,
    values: FormValues
  ) => {
    setDocumentAt({
      id: document?.document.sutureSignRequestId,
      document: {
        ...document,
        metadata: {
          ...values,
          encounterDate: values.encounterDate.toDateString(),
          medicalCondition: values.medicalCondition
            .map((i) => i.value)
            .join('. '),
          treatmentPlan: values.treatmentPlan || null,
        },
      },
    });
  };

  const resetCurrentDocument = (clonedDocument: RequestDocumentType) => {
    setDocumentAt({
      id: clonedDocument.document.sutureSignRequestId,
      document: clonedDocument,
    });
  };

  const saveCurrentDocument = async (
    document: RequestDocumentType,
    values: FormValues
  ) => {
    setCurrentF2FPageStatus({
      willBeSaved: false,
      doSave: false,
    });

    await updateFaceToFaceMetadata({
      id: document!.document.sutureSignRequestId,
      type: document!.template.templateType.templateTypeId,
      metadata: {
        ...values,
        medicalCondition: values.medicalCondition
          .map((i) => i.value)
          .join('. '),
      },
    }).unwrap();

    setCurrentF2FPageStatus({
      formValues: {
        ...values,
        encounterDate: values.encounterDate.toString(),
      },
    });
  };

  const options = useMemo(() => {
    if (!isLoading && data) {
      return {
        medicalCondition: orderSort(
          data.medicalConditions.slice(),
          'sequenceNumber'
        ).reduce<
          { label: string; options: { label: string; value: string }[] }[]
        >((acc, cur) => {
          if (cur.isPlaceholder) {
            return [...acc, { label: cur.description, options: [] }];
          }
          const currentGroup = acc[acc.length - 1];

          currentGroup?.options.push({
            label: cur.description,
            value: cur.description,
          });

          return acc;
        }, []),
        skilledService: Object.entries(data.skilledServices).map(
          ([key, value]) => ({
            label: value,
            value: key.toLocaleLowerCase(),
          })
        ),
        clinicalReason: [...data.clinicalReasons],
        homeboundReason: [...data.homeboundReasons],
      };
    }

    return undefined;
  }, [data]);

  useEffect(() => {
    if (isValid && allFields && isDirty) {
      if (!currentF2FPageStatus.willBeSaved) {
        setCurrentF2FPageStatus({
          willBeSaved: true,
          id: doc.document.sutureSignRequestId,
          templateTypeId: doc.document.template.templateType.templateTypeId,
          formValues: {
            ...allFields,
            encounterDate: allFields.encounterDate.toString(),
          },
        });
      } else {
        setCurrentF2FPageStatus({
          id: doc.document.sutureSignRequestId,
          templateTypeId: doc.document.template.templateType.templateTypeId,
          formValues: {
            ...allFields,
            encounterDate: allFields.encounterDate.toString(),
          },
        });
      }
    }
  }, [allFields, isValid, isDirty]);

  const onSubmit: SubmitHandler<FormValues> = async (values) => {
    if (currentAction === 'approve') {
      try {
        await saveCurrentDocument(doc, values);
        setCurrentDocument(doc, values);

        await approve();
      } catch (error) {
        resetCurrentDocument(clonedDoc);
        showErrorToast(error);
      } finally {
        setCurrentAction(null);
      }
    } else {
      try {
        setCurrentAction('submitFaceToFace');
        await saveCurrentDocument(doc, values);
        setCurrentDocument(doc, values);

        if (canSign) {
          setCurrentAction('sign');

          await signDocument({
            id: viewedDocument!.sutureSignRequestId,
            signerPassword: '',
          }).unwrap();

          setNextDoc({
            animation: {
              type: 'success',
              documentId: viewedDocument!.sutureSignRequestId,
            },
            callback: () => {
              removeDocs({
                documentIds: [viewedDocument!.sutureSignRequestId],
              });
            },
          });
          toast({
            title: 'Document Signed Successfully',
            status: 'success',
            duration: 2000,
          });
        } else {
          setModalVisible(true);
        }
      } catch (error) {
        resetCurrentDocument(clonedDoc);

        showErrorToast(error);
      } finally {
        setCurrentAction(null);
      }
    }
  };

  const onModalClose = () => {
    setModalVisible(false);
  };

  const onError = () => setCurrentAction(null);

  return (
    <>
      <Modal isOpen={modalVisible} onClose={onModalClose} size="lg">
        <ModalOverlay />
        <ModalContent>
          <SignModal onClose={onModalClose} />
        </ModalContent>
      </Modal>
      <Box
        width={826}
        maxH={1169}
        my={4}
        ml="auto"
        mr="auto"
        bg="white"
        p={6}
        className="panzoom-exclude"
      >
        <form
          onSubmit={handleSubmit(onSubmit, onError)}
          id={FACE_FORM_ID}
          data-cy="face-to-face-form"
        >
          <Stack spacing={4}>
            <DatePicker
              name="encounterDate"
              control={control}
              label="Date of Face-to-Face Encounter"
              rules={{ required: { value: true, message: 'Required' } }}
              placeholder="MM/DD/YYYY"
              formControlProps={{ w: '360px', zIndex: 1 }}
            />
            {options && (
              <>
                <TagSelect
                  name="medicalCondition"
                  control={control}
                  label="Medical Condition"
                  options={options.medicalCondition}
                  joinSymbolLength={1}
                  rules={{
                    validate: (value) => {
                      const typedValue =
                        value as FormValues['medicalCondition'];

                      if (
                        !typedValue ||
                        (typedValue && typedValue.length === 0)
                      ) {
                        return 'Required';
                      }

                      const currentLength = typedValue.reduce(
                        (acc: number, cur) => acc + cur.value.length + 1, // 1 is the length of dot symbol
                        0
                      );

                      if (currentLength > 150) {
                        return 'Max length exceeded';
                      }

                      return true;
                    },
                  }}
                  maxLength={150}
                />
                <CheckBox
                  name="skilledServices"
                  control={control}
                  label="Skilled Services Required (Select all that apply)"
                  options={options.skilledService}
                  rules={{ required: { value: true, message: 'Required' } }}
                  direction="row"
                />
                <TextareaSelect
                  name="clinicalReason"
                  control={control}
                  label="Clinical Findings & Reasons For Homecare"
                  options={options.clinicalReason}
                  rules={{
                    required: { value: true, message: 'Required' },
                    maxLength: { value: 450, message: 'Max length exceeded' },
                  }}
                  maxLength={450}
                />
                <TextareaSelect
                  name="homeboundReason"
                  control={control}
                  label="Reasons For Being Homebound"
                  options={options.homeboundReason}
                  rules={{
                    required: { value: true, message: 'Required' },
                    maxLength: { value: 250, message: 'Max length exceeded' },
                  }}
                  maxLength={250}
                />
              </>
            )}
            {viewedDocument?.template.templateType.templateTypeId === 2 && ( // Home Health Face-to-Face (non-primary)
              <Textarea
                name="treatmentPlan"
                control={control}
                label="Treatment Plan"
                rules={{
                  required: {
                    value: true,
                    message: 'A treatment plan is required',
                  },
                  maxLength: { value: 400, message: 'Max length exceeded' },
                }}
                maxLength={400}
              />
            )}
          </Stack>
        </form>
      </Box>
    </>
  );
}
