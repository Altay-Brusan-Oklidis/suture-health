import { useEffect } from 'react';
import { useSelector } from '@redux/hooks';
import {
  useGetPatientOptionsQuery,
  useLazyGetPatientOptionsQuery,
} from '@containers/Inbox/apiReducer';
import useOrgIds from './useOrgIds';

export default function usePatientOptions(searchParam?: string) {
  const patientOptionsPage = useSelector(
    (state) => state.inbox.patientOptionsPage
  );
  const filterMenuSearchValue = useSelector(
    (state) => state.inbox.filterMenuSearchValue
  );
  const orgIds = useOrgIds();
  const [getPatientOptions, lazyData] = useLazyGetPatientOptionsQuery();
  const data = useGetPatientOptionsQuery(
    {
      orgIds: orgIds as string[],
      pageNumber: patientOptionsPage,
      searchParam: filterMenuSearchValue,
    },
    { skip: !orgIds || Boolean(searchParam) }
  );

  useEffect(() => {
    if (searchParam && orgIds) {
      getPatientOptions({
        orgIds: orgIds as string[],
        pageNumber: 0,
        searchParam,
      });
    }
  }, [getPatientOptions, orgIds, searchParam]);

  return searchParam ? lazyData : data;
}
