import type { DocumentSummaryItemType } from '@utils/zodModels';
import { DocStatus } from 'suture-theme';
import dayjs from 'dayjs';
import minMax from 'dayjs/plugin/minMax';

dayjs.extend(minMax);

export type FilterCountObj = {
  [key: string]: {
    badgeCount: number;
    date: string;
    status: DocStatus;
  };
};

export default function optionsCalc(
  obj: FilterCountObj,
  id: number | '0' | 'resent' | 'approved' | 'unapproved',
  doc: DocumentSummaryItemType
) {
  obj[id] = {
    badgeCount: (obj[id]?.badgeCount || 0) + 1,
    date: dayjs.min([dayjs(obj[id]?.date), dayjs(doc.submittedAt)]).format(),
    status: obj[id]?.status
      ? obj[id]?.status === 'Critical' || obj[id]?.status === 'Warning'
        ? obj[id]?.status!
        : doc.statusDisplay
      : doc.statusDisplay,
  };
}
