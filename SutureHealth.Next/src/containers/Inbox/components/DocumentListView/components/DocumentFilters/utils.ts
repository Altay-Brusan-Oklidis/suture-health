import { DocumentSummaryItemType } from '@utils/zodModels';

type FilterCountItem = {
  count: number;
};

type FilterType = 'personal' | 'team';

type FilterCount = Record<FilterType, FilterCountItem>;

export const calcFilterCount = (
  docs: DocumentSummaryItemType[] | undefined
) => {
  const stats: FilterCount = {
    personal: {
      count: 0,
    },
    team: {
      count: 0,
    },
  };

  docs?.forEach((i) => {
    stats.team.count += 1;
    if (i.isPersonalDocument) {
      stats.personal.count += 1;
    }
  });

  return stats;
};
