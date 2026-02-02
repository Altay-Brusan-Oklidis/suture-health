import { useEffect, useState } from 'react';
import { Divider } from '@chakra-ui/react';
import { Box, CloudArrowUpFA, CloudCheckFA, CloudSavingFA } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import useAnnotationValidation from '@hooks/useAnnotationValidation';
import SaveModal from '@containers/Inbox/components/SaveModal';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useUpdateFaceToFaceMetadataMutation } from '@containers/Inbox/apiReducer';

export const SaveIndicator = () => {
  const [modalOpen, setModalOpen] = useState<boolean>(false);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const isAnnotationsUpdating = useSelector(
    (state) => state.inbox.isAnnotationsUpdating
  );
  const { isAnnotationsChanged, syncAnnotations } = useUpdateAnnotations();
  const [, { isLoading }] = useUpdateFaceToFaceMetadataMutation({
    fixedCacheKey: 'face2FaceUpdating',
  });
  const { isMovedTo } = useAnnotationValidation();
  const [wasSaving, setWasSaving] = useState(false);
  const { setCurrentF2FPageStatus } = useInboxActions();
  const currentF2FPageStatus = useSelector(
    (state) => state.inbox.currentF2FPageStatus
  );
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);

  useEffect(() => {
    if (!isSaving && (isAnnotationsUpdating || isLoading)) {
      setIsSaving(true);
    } else if (isSaving && (!isAnnotationsUpdating || !isLoading)) {
      setIsSaving(false);
    }
  }, [isAnnotationsUpdating, isLoading]);

  useEffect(() => {
    if (viewedDocument?.sutureSignRequestId) {
      setWasSaving(false);
      setIsSaving(false);
    }
  }, [viewedDocument?.sutureSignRequestId]);

  useEffect(() => {
    if (isSaving && !wasSaving) {
      setWasSaving(true);
    } else if (!isSaving && wasSaving && !isAnnotationsChanged) {
      setTimeout(() => {
        setWasSaving(false);
      }, 4000);
    } else if (wasSaving && isAnnotationsChanged) {
      setWasSaving(false);
    }
  }, [
    isSaving,
    wasSaving,
    isAnnotationsChanged,
    currentF2FPageStatus.willBeSaved,
  ]);

  const handleClick = () => {
    if (isMovedTo !== null) {
      setModalOpen(true);
    } else {
      syncAnnotations(false);
    }

    if (currentF2FPageStatus.willBeSaved) {
      setCurrentF2FPageStatus({ doSave: true });
    }
  };

  if (
    !isAnnotationsChanged &&
    !isSaving &&
    !wasSaving &&
    !currentF2FPageStatus.willBeSaved
  ) {
    return null;
  }

  if (isSaving) {
    return (
      <Box
        display="flex"
        flexDir="column"
        justifyItems="center"
        alignItems="center"
        border="0.5px solid"
        borderColor="blackAlpha.200"
        color="gray.500"
        bg="gray.100"
        borderRadius="10px"
        fontSize="12px"
        padding="4px"
        w="48px"
        h="48px"
      >
        <CloudSavingFA width="24px" height="24px" />
        Saving
      </Box>
    );
  }
  if (wasSaving && !isAnnotationsChanged) {
    return (
      <Box
        display="flex"
        flexDir="column"
        justifyItems="center"
        alignItems="center"
        border="0.5px solid"
        borderColor="blackAlpha.200"
        color="gray.500"
        bg="gray.100"
        borderRadius="10px"
        fontSize="12px"
        padding="4px"
        w="48px"
        h="48px"
      >
        <CloudCheckFA width="24px" height="24px" />
        Saved
      </Box>
    );
  }

  return (
    <Box
      display="flex"
      flexDir="column"
      justifyItems="center"
      alignItems="center"
      border="0.5px solid"
      borderColor="blackAlpha.200"
      color="blue.500"
      bg="gray.100"
      borderRadius="10px"
      fontSize="12px"
      fontWeight="semibold"
      padding="4px"
      w="48px"
      h="84px"
      onClick={handleClick}
      cursor="pointer"
    >
      <SaveModal onClose={() => setModalOpen(false)} isOpen={modalOpen} />
      <Box fontSize="10px" color="gray.500" fontWeight="medium">
        Unsaved Changes
      </Box>
      <Divider my="2px" w="100%" />
      <CloudArrowUpFA width="24px" height="24px" />
      Save
    </Box>
  );
};
