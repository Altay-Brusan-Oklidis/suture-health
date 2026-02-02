import { type ApprovalType, DocumentSummaryItemType } from '@utils/zodModels';
import sortBy, { SortOrder } from '@utils/sortBy';
import optionsCalc, { FilterCountObj } from '@utils/optionsCalc';
import { DocStatus } from 'suture-theme';
import dayjs from 'dayjs';
import minMax from 'dayjs/plugin/minMax';
import type { TransformedFilterOptions } from './apiReducer';

dayjs.extend(minMax);

export type SortVariant = 'date' | 'patient' | 'sender' | 'signer';

type FiltersCount = Record<
  'patients' | keyof Omit<TransformedFilterOptions, 'documentIds'>,
  FilterCountObj
>;

export function docsSortHandler(
  docs: DocumentSummaryItemType[],
  sortVariant: SortVariant,
  sortOrder: SortOrder
) {
  switch (sortVariant) {
    case 'date': {
      return sortBy(docs, ['submittedAt'], [sortOrder]);
    }

    case 'patient': {
      return sortBy(
        docs,
        [(i) => `${i.patient.lastName} ${i.patient.firstName}`, 'submittedAt'],
        [sortOrder]
      );
    }

    case 'sender': {
      return sortBy(
        docs,
        [(i) => i.submitterOrganization.name, 'submittedAt'],
        [sortOrder]
      );
    }

    case 'signer': {
      return sortBy(
        docs,
        [(i) => `${i.signer.lastName} ${i.signer.firstName}`, 'submittedAt'],
        [sortOrder]
      );
    }

    /* istanbul ignore next */
    default:
      return [];
  }
}

export function sortByApprovedAt(a: ApprovalType, b: ApprovalType) {
  const dateA = new Date(a.approvedAt);
  const dateB = new Date(b.approvedAt);

  if (dateA < dateB) {
    return -1;
  }
  if (dateA > dateB) {
    return 1;
  }

  return 0;
}

export function isAutomaticallyMovedDoc(doc: DocumentSummaryItemType) {
  return Boolean(
    doc.memberBacklog?.expiresAt &&
      !doc.memberBacklog?.closedAt &&
      new Date(doc.memberBacklog.expiresAt).getTime() < new Date().getTime()
  );
}

export function getFiltersCount(documents: DocumentSummaryItemType[]) {
  const filtersCount = {
    templateTypes: {},
    signers: {},
    collaborators: {},
    senderOrganizations: {},
    isResent: {},
    approval: {},
  } as FiltersCount;

  documents.forEach((i) => {
    optionsCalc(
      filtersCount.templateTypes,
      i.template.templateType.templateTypeId,
      i
    );
    optionsCalc(filtersCount.signers, i.signer.memberId, i);
    optionsCalc(filtersCount.collaborators, i.collaborator?.memberId || '0', i);
    optionsCalc(
      filtersCount.senderOrganizations,
      i.submitterOrganization.organizationId,
      i
    );
    optionsCalc(
      filtersCount.approval,
      i.approvals.length > 0 ? 'approved' : 'unapproved',
      i
    );

    if (i.isResent) {
      optionsCalc(filtersCount.isResent, 'resent', i);
    }
  });

  return filtersCount;
}

export function combineFilterOptionsWithCount(
  filterOptions: TransformedFilterOptions,
  filtersCount: FiltersCount
) {
  return Object.entries(filterOptions).reduce((acc, [key, value]) => {
    const typedKey = key as keyof TransformedFilterOptions;

    return {
      ...acc,
      [typedKey]: value
        .map((i) => {
          let status: DocStatus | undefined;

          if ('options' in i) {
            const filteredOptions = i.options
              .map((option) => ({
                ...option,
                ...(typedKey === 'documentIds'
                  ? undefined
                  : filtersCount[typedKey][option.value]),
              }))
              .sort((a, b) => (b?.badgeCount || 0) - (a?.badgeCount || 0));

            return {
              ...i,
              options: filteredOptions,
              badgeCount: filteredOptions.reduce(
                (accCount, cur) => accCount + (cur.badgeCount || 0),
                0
              ),
              date: filteredOptions.reduce(
                (minDate: string | undefined, cur) => {
                  status =
                    cur.status === 'Warning' || cur.status === 'Critical'
                      ? cur.status
                      : status;

                  if (minDate && cur.date) {
                    return dayjs
                      .min([dayjs(minDate), dayjs(cur.date)])
                      .format();
                  }

                  return cur.date || minDate;
                },
                undefined
              ),
              status,
            };
          }

          return {
            ...i,
            ...(typedKey === 'documentIds'
              ? undefined
              : filtersCount[typedKey][
                  typedKey === 'isResent' ? 'resent' : i.value
                ]),
          };
        })
        .sort((a, b) => (b?.badgeCount || 0) - (a?.badgeCount || 0)),
    };
  }, {} as TransformedFilterOptions);
}
