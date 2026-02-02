import { useCallback } from 'react';
import { useToast } from '@chakra-ui/react';
import { Toast } from 'suture-theme';
import isFetchBaseQueryError from '@utils/isFetchBaseQueryError';

export default function useErrorToast() {
  const toast = useToast({ render: Toast });

  const showErrorToast = useCallback(
    (error: unknown) => {
      if (isFetchBaseQueryError(error)) {
        toast({
          title: error.status,
          description:
            'error' in error
              ? error.error
              : 'statusText' in error
              ? (error.statusText as string)
              : JSON.stringify(error.data),
          status: 'error',
          duration: 6000,
        });
      }
    },
    [toast]
  );

  return showErrorToast;
}
