import { useEffect } from 'react';
import { Box, HStack } from '@chakra-ui/react';
import { useForm, FormProvider } from 'react-hook-form';
import { smartlookEvent } from '@utils/analytics';
import DocumentListView from './components/DocumentListView';
import DocumentView from './components/DocumentView';
import DocumentActions from './components/DocumentActions';
import DocumentViewWrapper from './components/DocumentViewWrapper';
import RevenueCalculation from './components/RevenueCalculation';
import LottieAnimations from './components/LottieAnimations';
import FilterSidebar from './components/FilterSidebar';
import FilterMenu from './components/FilterMenu';
import FilterHandler from './components/FilterHandler';
import { StoreContextProvider } from './context';
import styles from './styles';
import { useGetMemberIdentityQuery } from './apiReducer';
import AnnotationsChangeListener from './components/AnnotationsChangeListener';

export type ApprovalStatus = 'all' | 'approved' | 'unapproved';

export type DocumentProcessStatus =
  | 'all'
  | 'needssignature'
  | 'needsfillingout'
  | 'savedforlater';

type SidebarFilters = {
  documentProcessStatus?: DocumentProcessStatus;
};

export type FilterMenuFilters = {
  dateReceivedStart?: Date;
  dateReceivedEnd?: Date;
  effectiveDateStart?: Date;
  effectiveDateEnd?: Date;
  isResent: string[];
  templateTypeIds: string[];
  signerIds: string[];
  collaboratorIds: string[];
  submitterIds: string[];
  patientIds: string[];
};

type InboxViewFilters = {
  documentIds: string[];
  documentType: 'personal' | 'team';
  approvalStatus: ApprovalStatus[];
  openSearchParam?: string;
};

export type FilterData = SidebarFilters & FilterMenuFilters & InboxViewFilters;

export default function Inbox() {
  const { data: member } = useGetMemberIdentityQuery();

  useEffect(() => {
    if (member) {
      smartlookEvent({
        name: 'identify',
        id: member.memberId.toString(),
        data: {
          userId: member.memberId.toString(),
          memberTypeId: member.memberTypeId.toString(),
          canSign: member.canSign,
          isPayingClient: member.isPayingClient,
          companyId: '',
          primaryOrgId: '',
        },
      });
    }
  }, [member]);
  const methods = useForm<FilterData>({
    defaultValues: {
      documentIds: [],
      isResent: [],
      templateTypeIds: [],
      approvalStatus: [],
      signerIds: [],
      collaboratorIds: [],
      submitterIds: [],
      patientIds: [],
    },
  });

  return (
    <StoreContextProvider>
      {styles}
      <AnnotationsChangeListener />
      <RevenueCalculation />
      <Box
        width={{ sm: '100vw', xl: '100%' }}
        height={{ sm: 'var(--doc-height)', xl: '100%' }}
        display="flex"
        flexDirection="column"
      >
        <HStack alignItems="normal" spacing={0}>
          <FormProvider {...methods}>
            <FilterHandler />
            <form>
              <Box display="flex">
                <FilterSidebar />
                <FilterMenu />
                <DocumentListView />
              </Box>
            </form>
            <DocumentViewWrapper>
              <DocumentView />
              <Box mt={{ sm: 'auto', xl: 0 }}>
                <DocumentActions />
              </Box>
            </DocumentViewWrapper>
          </FormProvider>
        </HStack>
        <LottieAnimations />
      </Box>
    </StoreContextProvider>
  );
}
