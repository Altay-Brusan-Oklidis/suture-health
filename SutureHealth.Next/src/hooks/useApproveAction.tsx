import { useMemo, useCallback } from 'react';
import { useToast } from '@chakra-ui/react';
import { Toast } from 'suture-theme';
import { useSelector } from '@redux/hooks';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useErrorToast from '@hooks/useErrorToast';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { useInboxActions } from '@containers/Inbox/localReducer';
import {
  useGetMemberIdentityQuery,
  useChangeDocumentStatusMutation,
  useChangeDocumentStatusCacheMutation,
} from '@containers/Inbox/apiReducer';
import useRefetchViewedDoc from './useRefetchViewedDoc';

export default function useApproveAction() {
  const toast = useToast({ render: Toast });
  const { setCurrentAction } = useInboxActions();
  const showErrorToast = useErrorToast();
  const { data: member } = useGetMemberIdentityQuery();
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const [changeDocumentStatus] = useChangeDocumentStatusMutation();
  const [changeDocumentStatusCache] = useChangeDocumentStatusCacheMutation();
  const setNextDoc = useSetNextDoc();
  const { syncAnnotations } = useUpdateAnnotations();
  const refetchViewedDoc = useRefetchViewedDoc();
  const isApproved = useMemo(
    () =>
      Boolean(
        viewedDocument?.approvals.find(
          (i) => i.approver.memberId === member?.memberId
        )
      ),
    [viewedDocument, member]
  );

  const approve = useCallback(async () => {
    try {
      setCurrentAction('approve');

      syncAnnotations(true);
      await changeDocumentStatus({
        status: isApproved ? 'unapprove' : 'approve',
        requestIds: [viewedDocument!.sutureSignRequestId],
        approver: member,
      }).unwrap();
      refetchViewedDoc();

      setNextDoc(
        isApproved
          ? undefined
          : {
              animation: {
                type: 'success',
                documentId: viewedDocument!.sutureSignRequestId,
              },
              callback: () => {
                changeDocumentStatusCache({
                  status: 'approve',
                  requestIds: [viewedDocument!.sutureSignRequestId],
                  approver: member,
                });
              },
            }
      );
      toast({
        title: `Document ${
          isApproved ? 'Unapproved' : 'Approved'
        } Successfully`,
        status: 'success',
        duration: 2000,
      });
    } catch (error) {
      showErrorToast(error);
    } finally {
      setCurrentAction(null);
    }
  }, [
    changeDocumentStatus,
    changeDocumentStatusCache,
    isApproved,
    member,
    setCurrentAction,
    setNextDoc,
    showErrorToast,
    toast,
    viewedDocument,
  ]);

  return approve;
}
