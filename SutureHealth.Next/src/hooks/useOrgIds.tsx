import { useMemo } from 'react';
import { useGetOrganizationsQuery } from '@containers/Inbox/apiReducer';
import { useSelector } from '@redux/hooks';

export default function useOrgIds() {
  const { data: organizations } = useGetOrganizationsQuery();
  const selectedOrganizationId = useSelector(
    (state) => state.mainLayout.selectedOrganizationId
  );
  const orgIds = useMemo(() => {
    if (organizations) {
      return selectedOrganizationId
        ? [selectedOrganizationId]
        : organizations.organizationOptions.map((i) => i.value as string);
    }

    return undefined;
  }, [selectedOrganizationId, organizations]);

  return orgIds;
}
