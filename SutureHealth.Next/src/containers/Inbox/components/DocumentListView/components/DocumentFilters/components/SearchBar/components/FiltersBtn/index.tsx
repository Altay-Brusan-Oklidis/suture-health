import { FilterIcon, Badge } from 'suture-theme';
import { Box, Tooltip, ListItem, UnorderedList, Text } from '@chakra-ui/react';
import { useFormContext, useWatch } from 'react-hook-form';
import { useInboxActions } from '@containers/Inbox/localReducer';
import type { FilterData } from '@containers/Inbox';
import { useMemo } from 'react';

export default function FiltersBtn() {
  const { toggleIsFilterMenuOpen } = useInboxActions();
  const { control } = useFormContext<FilterData>();
  const [
    dateReceivedStart,
    dateReceivedEnd,
    effectiveDateStart,
    effectiveDateEnd,
    ...filterValues
  ] = useWatch({
    control,
    name: [
      'dateReceivedStart',
      'dateReceivedEnd',
      'effectiveDateStart',
      'effectiveDateEnd',
      'isResent',
      'templateTypeIds',
      'signerIds',
      'collaboratorIds',
      'submitterIds',
      'patientIds',
    ],
  });
  const filtersCount = useMemo(() => {
    let count = 0;

    if (dateReceivedStart || dateReceivedEnd) {
      count += 1;
    }
    if (effectiveDateStart || effectiveDateEnd) {
      count += 1;
    }

    filterValues.forEach((i) => {
      count += i.length;
    });

    return count;
  }, [
    dateReceivedStart,
    dateReceivedEnd,
    effectiveDateStart,
    effectiveDateEnd,
    filterValues,
  ]);

  return (
    <Tooltip
      label={
        <Box fontWeight="bold">
          <Text>Filter By</Text>
          <UnorderedList pl="8px">
            <ListItem>Signer</ListItem>
            <ListItem>Sender</ListItem>
            <ListItem>Document Type</ListItem>
            <ListItem>Date Range</ListItem>
            <ListItem>and More</ListItem>
          </UnorderedList>
        </Box>
      }
    >
      <Box
        color="gray.500"
        bg="blackAlpha.100"
        borderRadius="8px"
        display="flex"
        flexDirection="column"
        alignItems="center"
        justifyContent="center"
        ml="8px"
        px="8px"
        height="40px"
        fontSize="12px"
        cursor="pointer"
        borderWidth="1px"
        borderColor="blackAlpha.100"
        onClick={() => toggleIsFilterMenuOpen()}
        _hover={{ bg: 'gray.200' }}
        position="relative"
      >
        <FilterIcon fill="gray.500" />
        <Text fontWeight="bold" lineHeight="12px">
          Filters
        </Text>
        <Badge
          count={filtersCount}
          isEllipse
          position="absolute"
          top={0}
          right={0}
          transform="translate(50%, -50%)"
        />
      </Box>
    </Tooltip>
  );
}
