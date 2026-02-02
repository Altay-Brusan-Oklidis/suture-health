import { useEffect } from 'react';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';

export default function SessionHandler() {
  const { data, isLoading } = useGetMemberIdentityQuery();

  // fix for bug #3901
  useEffect(() => {
    if (!isLoading && data) {
      /* istanbul ignore else */
      if (!sessionStorage.getItem('user')) {
        sessionStorage.setItem('user', data.memberId.toString());
      } else {
        const user = sessionStorage.getItem('user');

        if (user !== data.memberId.toString()) {
          sessionStorage.removeItem('selectedOrganizationId');
        }
      }
    }
  }, [data]);

  return null;
}
