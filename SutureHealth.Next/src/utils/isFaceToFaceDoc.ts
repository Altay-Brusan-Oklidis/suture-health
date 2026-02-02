import { DocumentSummaryItemType } from '@utils/zodModels';

export default function isFaceToFaceDoc(document?: DocumentSummaryItemType) {
  return (
    document?.template.templateType.templateTypeId === 1 ||
    document?.template.templateType.templateTypeId === 2
  );
}
