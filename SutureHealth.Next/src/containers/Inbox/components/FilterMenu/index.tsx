import { useMemo, useState } from 'react';
import {
  Accordion,
  Box,
  Button,
  Divider,
  IconButton,
  Text,
} from '@chakra-ui/react';
import {
  CloseIconFA,
  InboxInIconFA,
  CalendarPlusIconFA,
  escapeString,
} from 'suture-theme';
import { useFormContext, useWatch } from 'react-hook-form';
import type { FilterData } from '@containers/Inbox';
import { useInboxActions } from '@containers/Inbox/localReducer';
import type { TransformedFilterOptions } from '@containers/Inbox/apiReducer';
import { useSelector } from '@redux/hooks';
import debounce from '@utils/debounce';
import usePatientOptions from '@hooks/usePatientOptions';
import SearchInput from './components/SearchInput';
import FilterAccordion from './components/FilterAccordion';

export default function FilterMenu() {
  const datePickerIndexes = useMemo(() => [0, 1], []);
  const [accordionIndexes, setAccordionIndexes] =
    useState<number[]>(datePickerIndexes);
  const {
    toggleIsFilterMenuOpen,
    increasePatientOptionsPage,
    setFilterMenuSearchValue,
    resetPatientOptionsPage,
  } = useInboxActions();
  const isFilterMenuOpen = useSelector((state) => state.inbox.isFilterMenuOpen);
  const filterOptions = useSelector((state) => state.inbox.filterOptions);
  const patientOptions = useSelector((state) => state.inbox.patientOptions);
  const allPatientOptions = useSelector(
    (state) => state.inbox.allPatientOptions
  );
  const filterMenuSearchValue = useSelector(
    (state) => state.inbox.filterMenuSearchValue
  );
  const noMorePatientOptions = useSelector(
    (state) => state.inbox.noMorePatientOptions
  );
  const { reset, control } = useFormContext<FilterData>();
  const documentType = useWatch({ control, name: 'documentType' });
  const { isFetching: isPatientsFetching } = usePatientOptions();

  const options = useMemo(() => {
    if (filterOptions) {
      return Object.entries(filterOptions).reduce((acc, [key, value]) => {
        const searchVal = escapeString(filterMenuSearchValue.toLowerCase());
        const typedKey = key as keyof TransformedFilterOptions;

        if (filterMenuSearchValue && value[0] && 'options' in value[0]) {
          return {
            ...acc,
            [typedKey]: value
              .flatMap((i) => ('options' in i ? i.options : i))
              .filter(
                (option) => option.label.toLowerCase().search(searchVal) !== -1
              )
              .sort((a, b) => (b?.badgeCount || 0) - (a?.badgeCount || 0))
              .slice(0, 10),
          };
        }

        return {
          ...acc,
          [typedKey]: value
            .map((i) => {
              const isOptionMatch =
                i.label.toLowerCase().search(searchVal) !== -1;

              return isOptionMatch ? i : undefined;
            })
            .filter((i) => i),
        };
      }, {} as TransformedFilterOptions);
    }

    return undefined;
  }, [filterOptions, filterMenuSearchValue]);
  const patients = useMemo(
    () =>
      patientOptions?.filter(
        (i) =>
          i.label
            .toLowerCase()
            .search(escapeString(filterMenuSearchValue.toLowerCase())) !== -1
      ),
    [patientOptions, filterMenuSearchValue]
  );

  const debouncedSetSearchValue = useMemo(
    () =>
      debounce((value: string) => {
        if (value && accordionIndexes.length !== 100) {
          setAccordionIndexes([...Array(100).keys()]); // open all accordions
        } else if (!value) {
          setAccordionIndexes(datePickerIndexes);
        }

        setFilterMenuSearchValue(value);
        resetPatientOptionsPage();
      }, 200),
    [
      datePickerIndexes,
      accordionIndexes,
      setFilterMenuSearchValue,
      resetPatientOptionsPage,
    ]
  );

  return (
    <Box
      w={isFilterMenuOpen ? 'var(--filter-menu-width)' : '0px'}
      pointerEvents={isFilterMenuOpen ? 'all' : 'none'}
      pt="24px"
      display="flex"
      flexDirection="column"
      borderRightWidth={isFilterMenuOpen ? '0.5px' : 0}
      borderColor="gray.200"
      transition="width .3s, padding .3s"
      overflow="hidden"
      zIndex={1} // should overlap FilterSidebar
      height="calc(var(--doc-height) - var(--navbar-height))"
      sx={
        !isFilterMenuOpen
          ? {
              '> *': {
                pointerEvents: 'none',
              },
            }
          : undefined
      }
    >
      <Box
        w="var(--filter-menu-width)"
        display="flex"
        flexDirection="column"
        overflow="auto"
      >
        <Box px="16px">
          <Box display="flex" justifyContent="space-between">
            <Button
              size="sm"
              variant="outline"
              onClick={() =>
                reset({
                  documentProcessStatus: 'all',
                  dateReceivedStart: undefined,
                  dateReceivedEnd: undefined,
                  effectiveDateStart: undefined,
                  effectiveDateEnd: undefined,
                  isResent: [],
                  templateTypeIds: [],
                  approvalStatus: [],
                  signerIds: [],
                  collaboratorIds: [],
                  submitterIds: [],
                  patientIds: [],
                  documentIds: [],
                  documentType,
                  openSearchParam: undefined,
                })
              }
            >
              Reset All Filters
            </Button>
            <IconButton
              aria-label="close menu"
              icon={<CloseIconFA />}
              onClick={() => toggleIsFilterMenuOpen()}
              size="sm"
              colorScheme="gray"
              bg="transparent"
            />
          </Box>
          <SearchInput onChange={debouncedSetSearchValue} />
          <Divider mt="18px" />
        </Box>
        {Object.values({ ...options, patients }).flat().length === 0 &&
          !isPatientsFetching && (
            <Text color="gray.500" mt="18px" px="16px" fontSize="14px">
              We didn’t find filter options that match your search
            </Text>
          )}
        <Accordion
          allowMultiple
          onChange={(indexes) => setAccordionIndexes(indexes as number[])}
          index={accordionIndexes}
          overflow="auto"
          mt="18px"
          px="16px"
          sx={{
            '.chakra-divider:first-of-type': {
              display: 'none',
            },
            '.chakra-accordion__item:first-of-type': {
              marginTop: 0,
            },
          }}
        >
          {!filterMenuSearchValue && (
            <>
              <FilterAccordion
                label="Date Received"
                startDateName="dateReceivedStart"
                endDateName="dateReceivedEnd"
                icon={<InboxInIconFA />}
              />
              <FilterAccordion
                label="Effective Date"
                startDateName="effectiveDateStart"
                endDateName="effectiveDateEnd"
                icon={<CalendarPlusIconFA />}
              />
            </>
          )}
          <FilterAccordion
            label="Priority"
            name="isResent"
            options={options?.isResent}
            allOptions={filterOptions?.isResent}
            withSeeMore
          />
          <FilterAccordion
            label="Document Type"
            name="templateTypeIds"
            options={options?.templateTypes}
            allOptions={filterOptions?.templateTypes}
            withSeeMore
          />
          <FilterAccordion
            label="Approval"
            name="approvalStatus"
            options={options?.approval}
            allOptions={filterOptions?.approval}
            withSeeMore
          />
          <FilterAccordion
            label="Signers"
            name="signerIds"
            options={options?.signers}
            allOptions={filterOptions?.signers}
            withSeeMore
          />
          <FilterAccordion
            label="Collaborators"
            name="collaboratorIds"
            options={options?.collaborators}
            allOptions={filterOptions?.collaborators}
            withSeeMore
          />
          <FilterAccordion
            label="Patients"
            name="patientIds"
            options={patients}
            allOptions={allPatientOptions}
            onSeeMoreClick={() => increasePatientOptionsPage()}
            withSeeMore={!noMorePatientOptions}
          />
          <FilterAccordion
            label="Senders"
            name="submitterIds"
            options={options?.senderOrganizations}
            allOptions={filterOptions?.senderOrganizations}
            withSeeMore
          />
        </Accordion>
      </Box>
    </Box>
  );
}
