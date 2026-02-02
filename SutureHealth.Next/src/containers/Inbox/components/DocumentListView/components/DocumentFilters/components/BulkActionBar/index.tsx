import { Checkbox, Flex, Tooltip, Text, useConst } from '@chakra-ui/react';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useSelector } from '@redux/hooks';
import compareSimpleArr from '@utils/compareSimpleArr';
import SignIconButton from './components/SignIconButton';
import RejectIconButton from './components/RejectIconButton';
import ReassignIconButton from './components/ReassignIconButton';
import ApproveIconButton from './components/ApproveIconButton';

type BulkActionBarProps = {
  filteredDocsIds: string[];
};

export interface BulkButtonProps {
  tooltipDelay: number;
}

const BulkActionBar = ({ filteredDocsIds }: BulkActionBarProps) => {
  const { setSelectedDocuments } = useInboxActions();
  const documents = useSelector((state) => state.inbox.documents);
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const tooltipDelay = useConst(3000);

  return (
    <Flex dir="row" alignItems="center">
      <Tooltip label="Select All" placement="top" openDelay={tooltipDelay}>
        <Checkbox
          isDisabled={!documents || documents.length === 0}
          mr="auto"
          isChecked={
            Boolean(filteredDocsIds.length) &&
            compareSimpleArr(filteredDocsIds, selectedDocuments)
          }
          isIndeterminate={
            Boolean(selectedDocuments.length) &&
            !compareSimpleArr(filteredDocsIds, selectedDocuments)
          }
          onChange={(e) =>
            setSelectedDocuments(e.target.checked ? filteredDocsIds : [])
          }
          color="gray.500"
        />
      </Tooltip>
      <Text color="gray.500" ml="8px" mr="4px">
        |
      </Text>
      <SignIconButton tooltipDelay={tooltipDelay} />
      <ApproveIconButton tooltipDelay={tooltipDelay} />
      <RejectIconButton tooltipDelay={tooltipDelay} />
      <ReassignIconButton tooltipDelay={tooltipDelay} />
    </Flex>
  );
};

export default BulkActionBar;
