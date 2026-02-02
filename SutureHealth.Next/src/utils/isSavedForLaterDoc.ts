import { DocumentSummaryItemType } from '@utils/zodModels';

export default function isSavedForLaterDoc(document: DocumentSummaryItemType) {
  if (document.memberBacklog === null) return false;

  const now = new Date().getTime();
  const expirationDate = new Date(
    document.memberBacklog.expiresAt as string
  ).getTime();

  if (expirationDate > now) {
    return true;
  }

  return false;
}
