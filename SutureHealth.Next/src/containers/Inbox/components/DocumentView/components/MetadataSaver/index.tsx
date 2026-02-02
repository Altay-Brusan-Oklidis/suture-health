import { useEffect, useMemo } from 'react';
import { useSelector } from '@redux/hooks';
import {
  F2FPageStatusFormValues,
  useInboxActions,
} from '@containers/Inbox/localReducer';
import { RequestDocumentType } from '@utils/zodModels';
import { useUpdateFaceToFaceMetadataMutation } from '@containers/Inbox/apiReducer';
import { SaveIconFA } from 'suture-theme';
import { useToast } from '@chakra-ui/react';
import useErrorToast from '@hooks/useErrorToast';

const MetadataSaver = () => {
  const [updateFaceToFaceMetadata] = useUpdateFaceToFaceMetadataMutation({
    fixedCacheKey: 'face2FaceUpdating',
  });
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { setDocumentAt, setCurrentF2FPageStatus } = useInboxActions();
  const [, { isLoading }] = useUpdateFaceToFaceMetadataMutation({
    fixedCacheKey: 'face2FaceUpdating',
  });
  const currentF2FPageStatus = useSelector(
    (state) => state.inbox.currentF2FPageStatus
  );
  const toast = useToast();
  const showErrorToast = useErrorToast();

  const doc = useMemo(
    () => documentsObj[currentF2FPageStatus.id] as RequestDocumentType,
    [documentsObj, currentF2FPageStatus.id]
  );

  const isSaving = useMemo(() => isLoading, [isLoading]);

  const setCurrentDocument = (
    document: RequestDocumentType,
    values: F2FPageStatusFormValues
  ) => {
    setDocumentAt({
      id: currentF2FPageStatus.id,
      document: {
        ...document,
        metadata: {
          ...values,
          medicalCondition: values.medicalCondition
            .map((i) => i.value)
            .join('. '),
          treatmentPlan: values.treatmentPlan || null,
        },
      },
    });
  };

  const saveCurrentDocument = async (values: F2FPageStatusFormValues) => {
    setCurrentF2FPageStatus({
      willBeSaved: false,
      doSave: false,
    });
    await updateFaceToFaceMetadata({
      id: currentF2FPageStatus.id,
      type: currentF2FPageStatus.templateTypeId,
      metadata: {
        ...values,
        encounterDate: new Date(values.encounterDate),
        medicalCondition: values.medicalCondition
          ? values.medicalCondition.map((i) => i.value).join('. ')
          : '',
      },
    }).unwrap();

    setCurrentF2FPageStatus({
      willBeSaved: false,
      doSave: false,
      formValues: values,
    });
  };

  useEffect(() => {
    if (currentF2FPageStatus.doSave && !isSaving) {
      saveCurrentDocument(currentF2FPageStatus.formValues);
      setCurrentDocument(doc, currentF2FPageStatus.formValues);
    }
  }, [currentF2FPageStatus.doSave]);

  useEffect(() => {
    if (
      viewedDocument?.sutureSignRequestId !== currentF2FPageStatus.id &&
      currentF2FPageStatus.willBeSaved
    ) {
      if (doc !== null) {
        try {
          saveCurrentDocument(currentF2FPageStatus.formValues);
          toast({
            icon: SaveIconFA({ w: '24px', h: '24px' }),
            title: 'Previous Document Saved',
            position: 'bottom-left',
            duration: 1000,
          });
          setCurrentDocument(doc, currentF2FPageStatus.formValues);
        } catch (error) {
          showErrorToast(error);
        }
      }
    }
  }, [viewedDocument?.sutureSignRequestId]);

  return null;
};

export default MetadataSaver;
