import { useEffect, useMemo } from 'react';
import {
  Menu,
  MenuButton,
  MenuOptionGroup,
  MenuItemOption,
  MenuList,
} from '@chakra-ui/react';
import { ChevronDownIcon } from '@chakra-ui/icons';
import { Button } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import { useMainLayoutActions } from '@layouts/MainLayout/reducer';
import { useGetOrganizationsQuery } from '@containers/Inbox/apiReducer';
import { smartlookEvent } from '@utils/analytics';

export default function OrgDropdown() {
  const { data } = useGetOrganizationsQuery();
  const selectedOrganizationId = useSelector(
    (state) => state.mainLayout.selectedOrganizationId
  );
  const { setSelectedOrganizationId } = useMainLayoutActions();
  const isOneOrganization = data?.organizations.length === 1;
  const label = useMemo(
    () =>
      data?.organizationOptions.find((i) => i.value === selectedOrganizationId)
        ?.label,
    [data, selectedOrganizationId]
  );

  useEffect(() => {
    const organization = sessionStorage.getItem('selectedOrganizationId');

    if (organization) {
      setSelectedOrganizationId(organization);
    }
  }, [setSelectedOrganizationId]);

  useEffect(() => {
    // TODO: set default instead of string 'many'
    smartlookEvent({
      name: 'properties',
      data: {
        organizationId: selectedOrganizationId || 'many',
      },
    });
  }, [selectedOrganizationId]);

  useEffect(() => {
    if (isOneOrganization && !selectedOrganizationId && data) {
      setSelectedOrganizationId(data.organizationOptions[0]?.value as string);
    }
  }, [
    isOneOrganization,
    selectedOrganizationId,
    data,
    setSelectedOrganizationId,
  ]);

  return (
    <Menu isLazy>
      <MenuButton
        as={Button}
        rightIcon={<ChevronDownIcon />}
        size="xs"
        mr={4}
        data-cy="organization-btn"
      >
        {label || 'Organizations'}
      </MenuButton>
      <MenuList zIndex={2} data-cy="org-menu">
        <MenuOptionGroup
          defaultValue={isOneOrganization ? selectedOrganizationId : 'All'}
          value={selectedOrganizationId}
        >
          {!isOneOrganization && (
            <MenuItemOption
              value="All"
              onClick={() => setSelectedOrganizationId(undefined)}
            >
              All
            </MenuItemOption>
          )}
          {data?.organizationOptions.map((i) => (
            <MenuItemOption
              value={i.value.toString()}
              key={i.value as string}
              onClick={() => {
                if (i.value !== selectedOrganizationId) {
                  setSelectedOrganizationId(i.value as string);
                }
              }}
            >
              {i.label}
            </MenuItemOption>
          ))}
        </MenuOptionGroup>
      </MenuList>
    </Menu>
  );
}
