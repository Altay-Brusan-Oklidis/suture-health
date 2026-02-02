import { Box } from '@chakra-ui/react';
import {
  InboxIconFA,
  SignIconFA,
  PenFieldIconFA,
  HistoryIconFA,
  ChevronLeftIconFA,
  ChevronRightIconFA,
} from 'suture-theme';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useGetDocumentsCountQuery } from '@containers/Inbox/apiReducer';
import { useSelector } from '@redux/hooks';
import { skipToken } from '@reduxjs/toolkit/dist/query';
import useOrgIds from '@hooks/useOrgIds';
import SidebarBtn from './components/SidebarBtn';

export default function FilterSidebar() {
  const { toggleIsFilterSidebarOpen } = useInboxActions();
  const isFilterSidebarOpen = useSelector(
    (state) => state.inbox.isFilterSidebarOpen
  );
  const orgIds = useOrgIds();
  const { data: documentsCount } = useGetDocumentsCountQuery(
    orgIds || skipToken,
    { pollingInterval: 10000 }
  );

  return (
    <Box
      w={
        isFilterSidebarOpen
          ? 'var(--filter-sidebar-open-width)'
          : 'var(--filter-sidebar-close-width)'
      }
      py="20px"
      display="flex"
      flexDirection="column"
      alignItems="flex-start"
      borderRightWidth="0.5px"
      borderColor="gray.200"
      transition="width .3s, padding .3s"
      className={`filter-sidebar ${isFilterSidebarOpen ? 'opened' : ''}`}
      data-cy="filter-sidebar"
    >
      <SidebarBtn
        label="All"
        icon={<InboxIconFA />}
        tooltipLabel="All"
        badgeCount={documentsCount?.all}
        value="all"
        data-cy="all"
      />
      <SidebarBtn
        label="Sign"
        icon={<SignIconFA />}
        tooltipLabel="Needs Signature"
        badgeCount={documentsCount?.needsSignature}
        value="needssignature"
        data-cy="needs-signature"
      />
      <SidebarBtn
        label="Fill out"
        icon={<PenFieldIconFA />}
        tooltipLabel="Needs Filling Out"
        badgeCount={documentsCount?.needsFillingOut}
        value="needsfillingout"
        data-cy="needs-filling-out"
      />
      <Box
        backgroundColor="gray.300"
        height="1px"
        width={isFilterSidebarOpen ? '100%' : 'calc(100% - 32px)'}
        px={isFilterSidebarOpen ? 0 : '16px'}
        my="12px"
        alignSelf="center"
      />
      <SidebarBtn
        label="Saved for Later"
        icon={<HistoryIconFA />}
        tooltipLabel="Saved for Later"
        badgeCount={documentsCount?.savedForLater}
        color="cyan.400"
        value="savedforlater"
      />
      <SidebarBtn
        label="Close"
        icon={
          isFilterSidebarOpen ? <ChevronLeftIconFA /> : <ChevronRightIconFA />
        }
        mt="auto"
        onClick={() => toggleIsFilterSidebarOpen()}
      />
    </Box>
  );
}
