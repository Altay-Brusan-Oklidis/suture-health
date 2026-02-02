import { useEffect } from 'react';
import { useInboxActions } from '@containers/Inbox/localReducer';
import useLoadDocuments from '@hooks/useLoadDocuments';
import { useSelector } from '@redux/hooks';

export default function RevenueCalculation() {
  const { calculateRVU } = useInboxActions();
  const documents = useSelector((state) => state.inbox.documents);

  useLoadDocuments();

  useEffect(() => {
    if (documents) {
      const rvuUnit: number = documents.reduce(
        (acc, doc) => acc + doc.relativeValueUnit,
        0
      );
      const rvuValue: number = documents.reduce(
        (acc, doc) => acc + doc.revenueValue,
        0
      );

      calculateRVU({ value: rvuValue, unit: rvuUnit });
    }
  }, [calculateRVU, documents]);

  return null;
}
