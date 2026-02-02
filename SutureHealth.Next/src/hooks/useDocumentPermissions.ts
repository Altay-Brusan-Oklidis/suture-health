import { useMemo } from 'react';
import { useSelector } from '@redux/hooks';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';
import useMemberRole from './useMemberRole';

export default function useDocumentPermissions() {
  const { isSigner, isCollaboratorSigner } = useMemberRole();
  const viewedDocument = useSelector((store) => store.inbox.viewedDocument);
  const { data: member } = useGetMemberIdentityQuery();
  const [canSign, canAssign, canApprove] = useMemo<
    [boolean, boolean, boolean]
  >(() => {
    let userCanSign = false;
    let userCanAssign = false;
    let userCanApprove = false;

    if (member && viewedDocument) {
      if (isSigner || isCollaboratorSigner) {
        if (viewedDocument.signer.memberId === member.memberId) {
          userCanSign = !viewedDocument.isIncomplete;
          userCanAssign = isCollaboratorSigner;
        } else {
          userCanApprove = isCollaboratorSigner;
        }
      } else {
        userCanApprove = true;
      }
    }

    return [userCanSign, userCanAssign, userCanApprove];
  }, [isSigner, isCollaboratorSigner, viewedDocument, member]);

  return { canSign, canAssign, canApprove };
}
