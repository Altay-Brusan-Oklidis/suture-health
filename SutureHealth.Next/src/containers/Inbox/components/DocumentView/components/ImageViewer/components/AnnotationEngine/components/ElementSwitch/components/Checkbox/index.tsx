import { Checkbox } from 'suture-theme';
import type { AnnotationElementProps } from '../../index';

const checkedColor = 'rgba(250, 240, 137, .3)';

const CheckboxAnnotation = ({ value, onChange }: AnnotationElementProps) => (
  <Checkbox
    style={{ textShadow: 'none !important' }}
    backgroundColor={checkedColor}
    __css={{
      '.chakra-checkbox__control': {
        border: 'none',
        backgroundColor: checkedColor,
        color: 'black',
      },
    }}
    _checked={{
      _hover: {
        '.chakra-checkbox__control': {
          backgroundColor: checkedColor,
          color: 'black',
        },
      },
    }}
    w="16px"
    h="16px"
    isChecked={value === 'true' || value === true}
    onChange={(e) => onChange(e.target.checked)}
    display="block"
  />
);
export default CheckboxAnnotation;
