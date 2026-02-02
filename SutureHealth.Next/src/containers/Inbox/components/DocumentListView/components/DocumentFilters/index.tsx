import { useEffect, useMemo, useState } from 'react';
import { Box, Flex, useConst, Stack, IconButton } from '@chakra-ui/react';
import { SortButton, ChevronLeftIconFA } from 'suture-theme';
import { useFormContext, useWatch } from 'react-hook-form';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useSelector } from '@redux/hooks';
import SwitchButton from '@formAdapters/SwitchButton';
import sortByUtil from '@utils/sortBy';
import { useUpdateInboxPreferencesMutation } from '@containers/Inbox/apiReducer';
import { SortVariant } from '@containers/Inbox/utils';
import type { FilterData } from '@containers/Inbox';
import SearchBar from './components/SearchBar';
import ApprovalBtn from './components/ApprovalBtn';
import FilterTags from './components/FilterTags';
import BulkActionBar from './components/BulkActionBar';
import { calcFilterCount } from './utils';

export default function DocumentFilters() {
  const [filteredDocsIds, setFilteredDocsIds] = useState<string[]>([]);
  const { control } = useFormContext<FilterData>();
  const allFilterValues = useWatch({ control });
  const documentProcessStatus = useWatch({
    control,
    name: 'documentProcessStatus',
  });
  const { toggleOrder, setSortBy, setIsTabletDrawerOpen } = useInboxActions();
  const documents = useSelector((state) => state.inbox.documents);

  const sortOptions = useConst([
    { value: 'date', label: 'Date Received' },
    { value: 'patient', label: 'Patient' },
    { value: 'sender', label: 'Sender' },
    { value: 'signer', label: 'Signer' },
  ]);
  const sortOrder = useSelector((state) => state.inbox.sortOrder);
  const sortBy = useSelector((state) => state.inbox.sortBy);
  const globalDocuments = useSelector((state) => state.inbox.globalDocuments);
  const [updateInboxPreferences] = useUpdateInboxPreferencesMutation();
  const filterCount = useMemo(() => {
    const stats = calcFilterCount(globalDocuments);

    return stats;
  }, [globalDocuments]);

  useEffect(() => {
    if (documents) {
      setFilteredDocsIds(
        sortByUtil(documents, ['sutureSignRequestId'], [1]).map((i) =>
          i.sutureSignRequestId.toString()
        )
      );
    }
  }, [documents]);

  return (
    <Flex
      bg="white"
      w="100%"
      py={2}
      px="16px"
      pt="24px"
      pb="8px"
      direction="column"
      className="documents-filters"
      data-cy="documents-filters"
    >
      <IconButton
        aria-label="close list"
        icon={<ChevronLeftIconFA />}
        onClick={() => setIsTabletDrawerOpen(false)}
        colorScheme="gray"
        size="xs"
        mb="16px"
        display={{ sm: 'none', md: 'block', xl: 'none' }}
        marginLeft="auto"
      />
      <SearchBar />
      <FilterTags />
      <SwitchButton
        name="documentType"
        control={control}
        options={[
          {
            value: 'personal',
            label: 'Personal',
            count: filterCount.personal.count,
            tooltipProps: {
              label:
                'Personal view displays only documents that are relevant to you and enables workflow',
              textAlign: 'center',
              openDelay: 1000,
            },
          },
          {
            value: 'team',
            label: 'Team',
            count: filterCount.team.count,
            tooltipProps: {
              label:
                'Team view displays a shared inbox, showing all documents regardless of status',
              textAlign: 'center',
              openDelay: 1000,
            },
          },
        ]}
        mb={6}
        badgeProps={{
          max: 999,
          showZero: true,
          withTolltip: true,
          pointerEvents: 'none',
        }}
        onCustomOnChange={(value) => {
          updateInboxPreferences({
            view: value as FilterData['documentType'],
            approval:
              allFilterValues.approvalStatus?.length === 1
                ? allFilterValues.approvalStatus[0]!
                : 'all',
            documentProcess: documentProcessStatus!,
          });
        }}
      />
      <Stack spacing="16px" direction="row" mb="16px">
        <ApprovalBtn value="all" label="All" />
        <ApprovalBtn value="approved" label="Approved" />
        <ApprovalBtn value="unapproved" label="Unapproved" />
      </Stack>
      <Flex direction="row" alignItems="center">
        <BulkActionBar filteredDocsIds={filteredDocsIds} />
        <Box ml="auto" data-cy="sort-btn">
          <SortButton
            selected={sortBy}
            options={sortOptions}
            onSortChange={(value) => setSortBy(value as SortVariant)}
            order={sortOrder === 1 ? 'asc' : 'desc'}
            toggleOrder={toggleOrder}
            menuListProps={{ zIndex: 100, 'data-cy': 'sort-dropdown' }}
          />
        </Box>
      </Flex>
    </Flex>
  );
}
