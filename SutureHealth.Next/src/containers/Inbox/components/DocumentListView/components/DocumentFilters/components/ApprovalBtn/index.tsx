import { Button } from '@chakra-ui/react';
import { useFormContext, useController, useWatch } from 'react-hook-form';
import type { FilterData, ApprovalStatus } from '@containers/Inbox';
import {
  useGetInboxPreferencesQuery,
  useUpdateInboxPreferencesMutation,
} from '@containers/Inbox/apiReducer';

interface Props {
  value: ApprovalStatus;
  label: string;
}

export default function ApprovalBtn({ value, label }: Props) {
  const { control } = useFormContext<FilterData>();
  const [updateInboxPreferences] = useUpdateInboxPreferencesMutation();
  const { isLoading } = useGetInboxPreferencesQuery();
  const {
    field: { value: currentValue, onChange },
  } = useController({ control, name: 'approvalStatus' });
  const [documentType, documentProcessStatus] = useWatch({
    control,
    name: ['documentType', 'documentProcessStatus'],
  });
  const isActive =
    !isLoading &&
    ((currentValue.length === 1 && currentValue.includes(value)) ||
      ((currentValue.length === 0 || currentValue.length === 2) &&
        value === 'all'));

  return (
    <Button
      onClick={() => {
        onChange([value]);
        updateInboxPreferences({
          approval: value,
          view: documentType,
          documentProcess: documentProcessStatus!,
        });
      }}
      w="100%"
      size="sm"
      borderRadius="full"
      colorScheme={isActive ? 'blue' : 'gray'}
      bg={isActive ? undefined : 'blackAlpha.200'}
    >
      {label}
    </Button>
  );
}
