import { useEffect, useState } from 'react';
import { CalendarIcon } from '@chakra-ui/icons';
import { Flex } from '@chakra-ui/react';
import { CheckboxIcon, SignIconFA, TextIcon } from 'suture-theme';
import { motion } from 'framer-motion';
import { AnnotationType } from '@utils/zodModels';
import round from '@utils/round';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useSelector } from '@redux/hooks';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { createAnnotation } from '@containers/Inbox/components/DocumentView/components/ImageViewer/utils';
import AnnotationMenuButton from './components/AnnotationMenuButton';

const variants = {
  open: { opacity: 1, transition: { duration: 0 } },
  closed: { opacity: 0, transition: { duration: 0 } },
};

const AnnotationMenu = () => {
  const [activeButton, setActiveButton] = useState<
    AnnotationType | undefined
  >();
  const { addAnnotationItem } = useInboxActions();
  const [timesClicked, setTimesClicked] = useState<number>(0);
  const pageData = useSelector((state) => state.inbox.currentPageData);
  const isOpen = useSelector((state) => state.inbox.isEditing);
  const stackAnimation = useSelector((state) => state.inbox.stackAnimation);
  const { syncAnnotations } = useUpdateAnnotations();

  useEffect(() => {
    if (!isOpen && !stackAnimation) {
      syncAnnotations();
    }
  }, [isOpen, stackAnimation]);

  useEffect(() => {
    if (isOpen && activeButton) {
      setActiveButton(undefined);
    }
  }, [isOpen]);

  useEffect(() => {
    if (timesClicked !== 0) {
      setTimesClicked(0);
    }
  }, [pageData.documentReqId, pageData.pageNumber]);

  const handleMouseDown = (type: AnnotationType) => {
    setActiveButton(type);
  };

  const handleMouseUp = (type: AnnotationType) => {
    const offset = timesClicked * 10;
    const middleX = round((pageData.dimensions!.width + offset) / 2, 0);
    const middleY = round((pageData.dimensions!.height + offset) / 2, 0);
    const currentAnnotation = createAnnotation({
      type,
      page: pageData.pageNumber,
      x: middleX,
      y: middleY,
    });

    addAnnotationItem(currentAnnotation);
    setTimesClicked((prev) => prev + 1);
  };

  return (
    <Flex
      as={motion.div}
      animate={isOpen ? 'open' : 'closed'}
      variants={variants}
      opacity={isOpen ? variants.open.opacity : variants.closed.opacity}
      w="55px"
      bg="gray.700"
      borderRadius="0px 0px 6px 6px"
      direction="column"
      justifyContent="center"
      position="absolute"
      top="43px"
      __css={{
        pointerEvents: isOpen ? 'all' : 'none !important',
      }}
    >
      <Flex
        direction="column"
        w="100%"
        h="100%"
        py="16px"
        justify="space-between"
        align="center"
      >
        <AnnotationMenuButton
          ariaLabel="signature"
          onMouseDown={() => handleMouseDown(AnnotationType.VisibleSignature)}
          onMouseUp={() => handleMouseUp(AnnotationType.VisibleSignature)}
          tooltip="Signature"
          icon={<SignIconFA color="gray.200" />}
          isActive={activeButton === AnnotationType.VisibleSignature}
        />
        <AnnotationMenuButton
          ariaLabel="calendar"
          onMouseDown={() => handleMouseDown(AnnotationType.DateSigned)}
          onMouseUp={() => handleMouseUp(AnnotationType.DateSigned)}
          tooltip="Date Signed"
          icon={<CalendarIcon color="gray.200" />}
          isActive={activeButton === AnnotationType.DateSigned}
        />
        <AnnotationMenuButton
          ariaLabel="textbox"
          onMouseDown={() => handleMouseDown(AnnotationType.TextArea)}
          onMouseUp={() => handleMouseUp(AnnotationType.TextArea)}
          tooltip="Text Box"
          icon={<TextIcon color="gray.200" />}
          isActive={activeButton === AnnotationType.TextArea}
        />
        <AnnotationMenuButton
          ariaLabel="checkbox"
          onMouseDown={() => handleMouseDown(AnnotationType.CheckBox)}
          onMouseUp={() => handleMouseUp(AnnotationType.CheckBox)}
          tooltip="Checkbox"
          icon={<CheckboxIcon color="gray.200" />}
          isActive={activeButton === AnnotationType.CheckBox}
        />
      </Flex>
    </Flex>
  );
};

export default AnnotationMenu;
