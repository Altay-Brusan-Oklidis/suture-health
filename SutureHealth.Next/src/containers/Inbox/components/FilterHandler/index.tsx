import { useEffect, useMemo, useState, useRef } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import queryString from 'query-string';
import { useRouter } from 'next/router';
import dayjs from 'dayjs';
import minMax from 'dayjs/plugin/minMax';
import { skipToken } from '@reduxjs/toolkit/dist/query';
import compareSimpleArr from '@utils/compareSimpleArr';
import type { FilterData } from '@containers/Inbox';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useDocumentsRef, useDocViewRef } from '@containers/Inbox/context';
import {
  useLazyGetDocumentsQuery,
  useGetOrganizationsQuery,
  useGetInboxPreferencesQuery,
  useGetFilterOptionsQuery,
  useLazyGetAllDocumentsQuery,
} from '@containers/Inbox/apiReducer';
import type { TransformedFilterOptions } from '@containers/Inbox/apiReducer';
import { useSelector } from '@redux/hooks';
import { useMainLayoutActions } from '@layouts/MainLayout/reducer';
import debounce from '@utils/debounce';
import uniqBy from '@utils/uniqBy';
import getQueryParams from '@utils/getQueryParams';
import optionsCalc, { FilterCountObj } from '@utils/optionsCalc';
import usePatientOptions from '@hooks/usePatientOptions';
import useOrgIds from '@hooks/useOrgIds';
import { Option } from '@formAdapters/Checkbox';
import {
  getFiltersCount,
  combineFilterOptionsWithCount,
} from '@containers/Inbox/utils';

dayjs.extend(minMax);

interface ExtendedFilterData extends FilterData {
  signerOrganizationIds: string[];
}

export interface FilterQuery
  extends Omit<
    ExtendedFilterData,
    'approvalStatus' | 'isResent' | 'documentProcessStatus'
  > {
  isResent?: string;
  approvalStatus?: string;
  documentProcessStatus: 'All' | 'NeedsSign' | 'Fillout';
}

type FiltersCount = Record<
  'patients' | keyof Omit<TransformedFilterOptions, 'documentIds'>,
  FilterCountObj
>;

export default function FilterHandler() {
  const [canUpdateQueryParams, setCanUpdateQueryParams] =
    useState<boolean>(false);
  const {
    setDocuments,
    setSelectedDocuments,
    setViewedDocument,
    setFilterOptions,
    setGlobalDocuments,
    setIsDocumentsLoading,
    setPatientOptions,
    setNoMorePatientOptions,
    resetPatientOptionsPage,
    setAllPatientOptions,
  } = useInboxActions();
  const [getDocuments, { data: fetchedDocuments, isLoading }] =
    useLazyGetDocumentsQuery({
      pollingInterval: 10000,
    });
  const [
    getAllDocuments,
    { data: allDocuments, isLoading: isAllDocumentsLoading },
  ] = useLazyGetAllDocumentsQuery();
  const selectedOrganizationId = useSelector(
    (state) => state.mainLayout.selectedOrganizationId
  );
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const patientOptions = useSelector((state) => state.inbox.patientOptions);
  const allPatientOptions = useSelector(
    (state) => state.inbox.allPatientOptions
  );
  const patientOptionsPage = useSelector(
    (state) => state.inbox.patientOptionsPage
  );
  const { setSelectedOrganizationId } = useMainLayoutActions();
  const { control, setValue, getValues } = useFormContext<FilterData>();
  const documentType = useWatch({ control, name: 'documentType' });
  const documentIds = useWatch({ control, name: 'documentIds' });
  const filterValues = useWatch({
    control,
    name: [
      'documentProcessStatus',
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
      'approvalStatus',
      'openSearchParam',
    ],
  });
  const { data: organizations } = useGetOrganizationsQuery();
  const { data: inboxPreferences } = useGetInboxPreferencesQuery();
  const documentsRef = useDocumentsRef();
  const docViewRef = useDocViewRef();
  const router = useRouter();
  const orgIds = useOrgIds();
  const { data: filterOptions } = useGetFilterOptionsQuery(orgIds || skipToken);
  const { data: patients } = usePatientOptions();
  const documentsHashRef = useRef<string[] | null>(null);
  const allDocumentsHashRef = useRef<string[] | null>(null);

  const getGeneralFilterParams = () => {
    const query = queryString.parse(window.location.search, {
      arrayFormat: 'index',
    }) as Partial<FilterQuery>;

    return queryString.stringify(
      {
        approvalStatus: query.approvalStatus,
        documentProcessStatus: query.documentProcessStatus,
        signerOrganizationIds: query.signerOrganizationIds,
      },
      { arrayFormat: 'index' }
    );
  };

  // compare filtered documents and all documents if they are different, refetch all documents to recalculate filter count
  useEffect(() => {
    if (fetchedDocuments && allDocuments) {
      if (
        documentsHashRef.current &&
        documentsHashRef.current.length !== fetchedDocuments.length
      ) {
        getAllDocuments(getGeneralFilterParams());
      }

      documentsHashRef.current = fetchedDocuments.map((i) => JSON.stringify(i));
      allDocumentsHashRef.current = allDocuments.map((i) => JSON.stringify(i));

      if (
        documentsHashRef.current.some(
          (i) => !allDocumentsHashRef.current?.includes(i)
        )
      ) {
        getAllDocuments(getGeneralFilterParams());
      }
    }
  }, [fetchedDocuments, allDocuments]);

  const resetScroll = () => {
    documentsRef?.scrollToIndex(0);
    docViewRef?.scrollTo({ top: 0 });
    setViewedDocument(undefined);
  };

  useEffect(() => {
    if (localStorage.getItem('allPatientOptions')) {
      // TODO: maybe we should remove it
      setAllPatientOptions(
        JSON.parse(localStorage.getItem('allPatientOptions')!) as Option[]
      );
    }
  }, []);

  useEffect(() => {
    localStorage.setItem(
      'allPatientOptions',
      JSON.stringify(allPatientOptions)
    );
  }, [allPatientOptions]);

  useEffect(() => {
    if (allDocuments && patients) {
      const patientsCount = {
        patients: {},
      } as FiltersCount;

      allDocuments.forEach((i) => {
        optionsCalc(patientsCount.patients, i.patient.patientId, i);
      });

      setPatientOptions(
        uniqBy(
          [...(patientOptions || []), ...patients] // for patient pagination, combination of prevPatients + currentPatients
            .map((i) => ({
              ...i,
              ...patientsCount.patients[i.value as string],
            }))
            .sort((a, b) => (b?.badgeCount || 0) - (a?.badgeCount || 0)),
          'value'
        )
      );

      if (patients.length === 0) {
        setNoMorePatientOptions(true);
      }
    }
  }, [allDocuments, patients, setNoMorePatientOptions, setPatientOptions]);

  useEffect(() => {
    if (orgIds && patientOptionsPage !== 0) {
      resetPatientOptionsPage();
      setNoMorePatientOptions(false);
    }
  }, [orgIds, resetPatientOptionsPage, setNoMorePatientOptions]);

  useEffect(() => {
    if (allDocuments && filterOptions) {
      const filtersCount = getFiltersCount(allDocuments);
      const options = combineFilterOptionsWithCount(
        filterOptions,
        filtersCount
      );

      setFilterOptions(options);
    }
  }, [allDocuments, setFilterOptions, filterOptions]);

  useEffect(() => {
    if (fetchedDocuments) {
      const globalDocuments = fetchedDocuments.filter((i) =>
        documentIds.length
          ? documentIds.includes(i.sutureSignRequestId.toString())
          : true
      );

      setDocuments(
        globalDocuments.filter(
          (i) =>
            (documentType === 'personal' && i.isPersonalDocument) ||
            documentType === 'team'
        )
      );
      setGlobalDocuments(globalDocuments);

      if (router.query.documentType !== documentType) {
        const query = queryString.parse(window.location.search, {
          arrayFormat: 'index',
        });
        const stringifyQuery = queryString.stringify(
          {
            ...query,
            documentType,
          },
          { arrayFormat: 'index' }
        );

        router.push({ query: stringifyQuery }, undefined, { shallow: true });
        resetScroll();
      }
    }
  }, [
    fetchedDocuments,
    setDocuments,
    documentType,
    documentIds,
    setGlobalDocuments,
  ]);

  useEffect(() => {
    if (inboxPreferences) {
      setValue('documentType', inboxPreferences.view);
      setValue(
        'approvalStatus',
        inboxPreferences.approval === 'all' ? [] : [inboxPreferences.approval]
      );
      setValue(
        'documentProcessStatus',
        inboxPreferences.documentProcess || 'all'
      );
    }
  }, [inboxPreferences, setValue]);

  const debouncedSet = useMemo(
    () =>
      debounce(
        async ({ signerOrganizationIds }: Partial<ExtendedFilterData>) => {
          const {
            approvalStatus,
            isResent,
            documentProcessStatus,
            documentIds: documentIdsTemp,
            ...rest
          } = getValues();
          const approvalStatusValue =
            approvalStatus.length === 1 ? approvalStatus[0]! : 'all';
          const stringifyQuery = queryString.stringify(
            {
              ...rest,
              signerOrganizationIds,
              isResent: isResent.length ? true : undefined,
              approvalStatus:
                approvalStatusValue.charAt(0).toUpperCase() +
                approvalStatusValue.slice(1),
              documentProcessStatus: documentProcessStatus
                ? {
                    all: 'All',
                    needsfillingout: 'Fillout',
                    needssignature: 'NeedsSign',
                    savedforlater: 'SavedForLater',
                  }[documentProcessStatus]
                : 'All',
            } as FilterQuery,
            {
              arrayFormat: 'index',
            }
          );

          if (stringifyQuery !== getQueryParams()) {
            setIsDocumentsLoading(true);
            resetScroll();

            if (selectedDocuments.length !== 0) {
              setSelectedDocuments([]);
            }

            router.push(
              {
                query: stringifyQuery,
              },
              undefined,
              { shallow: true }
            );
            await getDocuments(stringifyQuery);
            setIsDocumentsLoading(false);
          }
        },
        300
      ),
    [getDocuments, router, setIsDocumentsLoading, selectedDocuments.length]
  );

  useEffect(() => {
    // fetch documents on the first render with query params
    if (
      !fetchedDocuments &&
      Object.keys(router.query).length > 0 &&
      !isLoading
    ) {
      getDocuments(getQueryParams());
    }
  }, [fetchedDocuments, router.query, isLoading, getDocuments]);

  useEffect(() => {
    // fetch documents on the first render with query params
    if (
      !allDocuments &&
      Object.keys(router.query).length > 0 &&
      !isAllDocumentsLoading
    ) {
      getAllDocuments(getGeneralFilterParams());
    }
  }, [allDocuments, router.query, isAllDocumentsLoading, getAllDocuments]);

  useEffect(() => {
    setTimeout(() => {
      const filterVal = getValues();
      const query = queryString.parse(window.location.search, {
        arrayFormat: 'index',
      }) as Partial<FilterQuery>;

      Object.entries(query).forEach(([key, value]) => {
        const typedKey = key as keyof FilterQuery | keyof FilterData;

        switch (typedKey) {
          case 'signerOrganizationIds': {
            if (
              Array.isArray(value) &&
              value?.length === 1 &&
              selectedOrganizationId !== value[0]
            ) {
              setSelectedOrganizationId(value[0]!);
            }
            break;
          }

          case 'isResent': {
            setValue(typedKey, [value.toString()]);
            break;
          }

          case 'approvalStatus': {
            const approvalVal = filterVal.approvalStatus;

            setValue(
              typedKey,
              (value as string) === 'All'
                ? approvalVal.length
                  ? approvalVal
                  : []
                : ([
                    (value as string).toLowerCase(),
                  ] as FilterData['approvalStatus'])
            );
            break;
          }

          case 'documentProcessStatus': {
            setValue(
              typedKey,
              {
                All: 'all',
                Fillout: 'needsfillingout',
                NeedsSign: 'needssignature',
                SavedForLater: 'savedforlater',
              }[value as string] as FilterData['documentProcessStatus']
            );
            break;
          }

          default: {
            if (
              (Array.isArray(value) &&
                !compareSimpleArr(
                  (filterVal[typedKey] || []) as string[],
                  value as string[]
                )) ||
              (!Array.isArray(value) && filterVal[typedKey] !== value)
            ) {
              setValue(
                typedKey,
                value as FilterData[keyof Omit<
                  FilterData,
                  'approvalStatus' | 'documentProcessStatus' | 'documentType'
                >]
              );
            }
            break;
          }
        }
      });

      setCanUpdateQueryParams(true);
    }, 0);
  }, [router.query]);

  useEffect(() => {
    if (organizations && canUpdateQueryParams) {
      debouncedSet({
        signerOrganizationIds: selectedOrganizationId
          ? [selectedOrganizationId]
          : organizations.organizationOptions.map((i) => i.value),
      });
    }
  }, [
    filterValues,
    selectedOrganizationId,
    organizations,
    canUpdateQueryParams,
    debouncedSet,
  ]);

  return null;
}
