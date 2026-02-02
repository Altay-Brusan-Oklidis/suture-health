/* eslint-disable prefer-regex-literals */
/* eslint-disable no-useless-escape */
import { useEffect, useMemo, useState } from 'react';
import { SearchBar, escapeString } from 'suture-theme';
import { Box } from '@chakra-ui/react';
import { useFormContext, useWatch } from 'react-hook-form';
import type { FilterData } from '@containers/Inbox';
import { useSelector } from '@redux/hooks';
import { skipToken } from '@reduxjs/toolkit/dist/query';
import type { TransformedFilterOptions } from '@containers/Inbox/apiReducer';
import { useGetFilterOptionsQuery } from '@containers/Inbox/apiReducer';
import { useInboxActions } from '@containers/Inbox/localReducer';
import {
  getFiltersCount,
  combineFilterOptionsWithCount,
} from '@containers/Inbox/utils';
import type { SimpleOtion } from '@formAdapters/Checkbox';
import { useMainLayoutActions } from '@layouts/MainLayout/reducer';
import usePatientOptions from '@hooks/usePatientOptions';
import useOrgIds from '@hooks/useOrgIds';
import optionsCalc, { FilterCountObj } from '@utils/optionsCalc';
import debounce from '@utils/debounce';
import FiltersBtn from './components/FiltersBtn';

type SearchOptions = Record<
  keyof Omit<TransformedFilterOptions, 'approval' | 'isResent'> | 'patients',
  SimpleOtion[]
>;

export default function DocsSearchBar() {
  const [searchValue, setSearchValue] = useState<string>('');
  const [debouncedSearchValue, setDebouncedSearchValue] = useState<string>('');
  const [tempFilterValues, setTempFilterValues] = useState<
    FilterData | undefined
  >();
  const { setValue, getValues, reset, control } = useFormContext<FilterData>();
  const documents = useSelector((state) => state.inbox.documents);
  const { setSelectedOrganizationId } = useMainLayoutActions();
  const { data: searchPatients, isFetching } =
    usePatientOptions(debouncedSearchValue);
  const { setAllPatientOptions } = useInboxActions();
  const orgIds = useOrgIds();
  const { data: filterOptions } = useGetFilterOptionsQuery(orgIds || skipToken);
  const openSearchParam = useWatch({ control, name: 'openSearchParam' });

  useEffect(() => {
    if (openSearchParam && !searchValue) {
      setSearchValue(openSearchParam);
    }
  }, [openSearchParam]);

  const patientOptions = useMemo(() => {
    if (documents && searchPatients) {
      const patientsCount: { patients: FilterCountObj } = {
        patients: {},
      };

      documents.forEach((i) => {
        optionsCalc(patientsCount.patients, i.patient.patientId, i);
      });

      return searchPatients
        .map((i) => ({
          ...i,
          ...patientsCount.patients[i.value as string],
        }))
        .sort((a, b) => (b?.badgeCount || 0) - (a?.badgeCount || 0));
    }

    return [];
  }, [documents, searchPatients]);
  const searchFilterOptions = useMemo(() => {
    if (filterOptions && documents) {
      const filtersCount = getFiltersCount(documents);
      const options = combineFilterOptionsWithCount(
        filterOptions,
        filtersCount
      );

      return options;
    }

    return undefined;
  }, [filterOptions, documents]);

  useEffect(() => {
    if (patientOptions) {
      setAllPatientOptions(patientOptions);
    }
  }, [patientOptions, setAllPatientOptions]);

  const options = useMemo(() => {
    if (searchFilterOptions && patientOptions) {
      const {
        collaborators,
        documentIds,
        patients,
        senderOrganizations,
        signers,
        templateTypes,
      } = Object.entries({
        ...searchFilterOptions,
        patients: patientOptions,
      }).reduce((acc, [key, value]) => {
        const typedKey = key as keyof TransformedFilterOptions;

        return {
          ...acc,
          [typedKey]: value
            .flatMap((i) => {
              const searchVal = escapeString(
                debouncedSearchValue.toLowerCase()
              );
              const isOptionMatch =
                i.label.toLowerCase().search(searchVal) !== -1;

              if ('options' in i) {
                const filteredOptions = i.options
                  ?.filter(
                    (option) =>
                      option.label.toLowerCase().search(searchVal) !== -1
                  )
                  .slice(0, 10);

                return filteredOptions?.length === 0 && !isOptionMatch
                  ? undefined
                  : filteredOptions;
              }

              return isOptionMatch ? i : undefined;
            })
            .filter((i) => i)
            .sort((a, b) => (b?.badgeCount || 0) - (a?.badgeCount || 0))
            .slice(0, 10),
        };
      }, {} as SearchOptions);

      return {
        collaborators,
        documentIds,
        patients,
        senderOrganizations,
        signers,
        templateTypes,
      };
    }

    return {} as SearchOptions;
  }, [searchFilterOptions, debouncedSearchValue, patientOptions]);

  const debouncedSetSearchValue = useMemo(
    () =>
      debounce((value: string) => {
        setDebouncedSearchValue(value);
      }, 200),
    []
  );

  useEffect(() => {
    debouncedSetSearchValue(searchValue);
  }, [searchValue, debouncedSetSearchValue]);

  return (
    <Box display="flex" mb="16px">
      <SearchBar
        searchInput={{
          onChange: (e) => setSearchValue(e.target.value),
          value: searchValue,
        }}
        isLoading={isFetching}
        placeholder="Search (ex. patient, sender, etc)"
        options={options}
        onMenuItemSelect={(name, value, type) => {
          const formName = {
            collaborators: 'collaboratorIds',
            documentIds: 'documentIds',
            patients: 'patientIds',
            senderOrganizations: 'submitterIds',
            signers: 'signerIds',
            templateTypes: 'templateTypeIds',
            search: 'openSearchParam',
          }[name] as Partial<keyof FilterData>;
          const values = getValues(formName);

          if (type === 'reset') {
            setValue(
              formName,
              Array.isArray(values)
                ? (values as string[]).filter((i) => i !== value)
                : undefined
            );

            if (tempFilterValues) {
              reset(tempFilterValues);
              setTempFilterValues(undefined);
            }
          } else if (type === 'showAll') {
            if (!tempFilterValues) {
              setTempFilterValues(getValues());
            }

            setSelectedOrganizationId(undefined);
            reset({
              documentProcessStatus: 'all',
              dateReceivedStart: undefined,
              dateReceivedEnd: undefined,
              effectiveDateStart: undefined,
              effectiveDateEnd: undefined,
              isResent: [],
              templateTypeIds: [],
              signerIds: [],
              collaboratorIds: [],
              submitterIds: [],
              patientIds: [],
              documentIds: [],
              documentType: 'personal',
              approvalStatus: [],
              openSearchParam: undefined,
              [formName]: [value],
            });
          } else if (formName === 'documentIds') {
            setValue(formName, [value]);
          } else if (formName === 'openSearchParam') {
            setValue(formName, value);
          } else {
            setValue(
              formName,
              (values as string[]).find((i) => i === value)
                ? (values as string[])
                : [...(values as string[]), value]
            );
          }
        }}
        labels={{
          collaborators: 'Collaborator',
          documentIds: 'Document ID',
          patients: 'Patient',
          senderOrganizations: 'Sender',
          signers: 'Signer',
          templateTypes: 'Document Type',
        }}
        menuDropdownProps={{ zIndex: 2 }}
        data-cy="search-bar"
        w="100%"
      />
      <FiltersBtn />
    </Box>
  );
}
