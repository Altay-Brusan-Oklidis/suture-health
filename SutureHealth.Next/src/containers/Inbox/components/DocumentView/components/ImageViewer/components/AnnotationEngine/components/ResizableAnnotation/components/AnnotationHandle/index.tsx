import { DeleteIcon } from '@chakra-ui/icons';
import { Flex } from '@chakra-ui/react';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useSelector } from '@redux/hooks';
import { AnnotationItem, AnnotationType } from '@utils/zodModels';
import { Box } from 'suture-theme';

type HandleProps = {
  children: JSX.Element;
  annotation: AnnotationItem;
  isResizing: boolean;
};

const AnnotationHandle = ({
  children,
  annotation,
  isResizing,
}: HandleProps) => {
  const { deleteAnnotationItem } = useInboxActions();
  const isOpen = useSelector((state) => state.inbox.isEditing);
  const hasValue = typeof annotation.value === 'string';
  const isTextBox = annotation.annotationTypeId === AnnotationType.TextArea;
  const borderColor = (ann: AnnotationItem) => {
    switch (ann.annotationTypeId) {
      case AnnotationType.VisibleSignature:
        return '#2AADFA';
      case AnnotationType.DateSigned:
        return '#2AADFA';
      case AnnotationType.CheckBox:
        return '#A0AEC0';
      case AnnotationType.TextArea:
        return hasValue ? '#2AADFA' : '#C22C27';
      default:
        return '#2AADFA';
    }
  };

  const handleClick = () => {
    if (isOpen) {
      deleteAnnotationItem(annotation.id!);
    }
  };

  return (
    <Box w="100%" h="100%" border={`1px solid ${borderColor(annotation)}`}>
      {isResizing ? (
        <></>
      ) : (
        <Flex
          alignContent="center"
          justifyContent="center"
          p="4px"
          w="16px"
          h="16px"
          position="absolute"
          top="-16px"
          right={0}
          cursor={isOpen ? 'pointer' : 'not-allowed'}
          backgroundColor={isTextBox && !hasValue ? '#C22C27' : '#2AADFA'}
          onClick={() => handleClick()}
          zIndex={1}
        >
          <DeleteIcon w="8px" h="10px" color="#FFFFFF" />
        </Flex>
      )}

      {children}
    </Box>
  );
};

export default AnnotationHandle;
