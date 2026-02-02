import { AnnotationItem, AnnotationType } from '@utils/zodModels';
import { useInboxActions } from '@containers/Inbox/localReducer';
import CheckboxAnnotation from './components/Checkbox';
import VisibleSignatureAnnotation from './components/VisibleSignature';
import DateSignedAnnotation from './components/DateSigned';
import TextAreaAnnotation from './components/TextArea';

type ElementSwitchProps = {
  annotation: AnnotationItem;
  isDragging?: boolean;
};

export interface AnnotationElementProps {
  value?: string | boolean | null;
  onChange: (value: string | boolean) => void;
}

const ElementSwitch = ({ annotation }: ElementSwitchProps) => {
  const { updateAnnotationItem } = useInboxActions();

  const onChange = (value: string | boolean) => {
    const updatedAnnotation = {
      ...annotation,
      value,
    };

    updateAnnotationItem(updatedAnnotation);
  };

  return {
    [AnnotationType.VisibleSignature]: <VisibleSignatureAnnotation />,
    [AnnotationType.CheckBox]: (
      <CheckboxAnnotation onChange={onChange} value={annotation.value} />
    ),
    [AnnotationType.DateSigned]: (
      <DateSignedAnnotation onChange={onChange} value={annotation.value} />
    ),
    [AnnotationType.TextArea]: (
      <TextAreaAnnotation onChange={onChange} value={annotation.value} />
    ),
  }[annotation.annotationTypeId];
};

export default ElementSwitch;
