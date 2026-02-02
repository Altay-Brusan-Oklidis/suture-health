import { AnnotationItem, AnnotationType } from '@utils/zodModels';

interface CreateAnnotation {
  type: AnnotationType;
  page: number;
  x: number;
  y: number;
}

export const createAnnotation = ({
  type,
  page,
  x,
  y,
}: CreateAnnotation): AnnotationItem => {
  const id = crypto.randomUUID();

  switch (type) {
    case AnnotationType.CheckBox:
      return {
        pageNumber: page,
        annotationTypeId: type,
        left: x - 8,
        top: y - 8,
        right: x + 8,
        bottom: y + 8,
        value: false,
        id,
      };

    case AnnotationType.DateSigned:
      return {
        pageNumber: page,
        annotationTypeId: type,
        left: x - 50,
        top: y - 22,
        right: x + 50,
        bottom: y + 22,
        value: null,
        id,
      };

    case AnnotationType.VisibleSignature:
      return {
        pageNumber: page,
        annotationTypeId: type,
        left: x - 136,
        top: y - 21,
        right: x + 136,
        bottom: y + 21,
        value: null,
        id,
      };

    case AnnotationType.TextArea:
      return {
        pageNumber: page,
        annotationTypeId: type,
        left: x - 82,
        top: y - 37,
        right: x + 82,
        bottom: y + 37,
        value: null,
        id,
      };

    default:
      return {
        pageNumber: page,
        annotationTypeId: type,
        left: x,
        top: y,
        right: x,
        bottom: y,
        value: null,
        id,
      };
  }
};
