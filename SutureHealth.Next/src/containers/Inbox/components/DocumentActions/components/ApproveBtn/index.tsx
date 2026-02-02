import { useMemo } from 'react';
import { Button } from '@chakra-ui/react';
import { useSelector } from '@redux/hooks';
import useDocumentPermissions from '@hooks/useDocumentPermissions';
import useApproveAction from '@hooks/useApproveAction';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';
import isFaceToFaceDoc from '@utils/isFaceToFaceDoc';
import type { BtnProps } from '../../index';

export default function ApproveBtn({ isDisabled, openModal }: BtnProps) {
  const { canApprove, canAssign } = useDocumentPermissions();
  const { data: member } = useGetMemberIdentityQuery();
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const approve = useApproveAction();
  const isApproved = useMemo(
    () =>
      Boolean(
        viewedDocument?.approvals.find(
          (i) => i.approver.memberId === member?.memberId
        )
      ),
    [viewedDocument, member]
  );

  const isCollabSigner = useMemo(
    () => (member?.canSign && member?.isCollaborator) || false,
    [member]
  );

  const isSignerOnDoc = useMemo(
    () => viewedDocument?.signer.memberId === member?.memberId,
    [viewedDocument, member]
  );

  // TODO: Unapprove is a beta release feature. This is disabled at MB's request.
  // https://dev.azure.com/suture/Suture/_boards/board/t/Engineering/Stories/?workitem=3847
  if (isApproved) return null;

  return member && viewedDocument && (canApprove || canAssign) ? (
    <Button
      isLoading={currentAction === 'approve'}
      isDisabled={
        isDisabled ||
        (canAssign &&
          (isFaceToFaceDoc(viewedDocument) ||
            viewedDocument.template.templateType.shortName ===
              'Certification' ||
            viewedDocument.template.templateType.shortName ===
              'Recertification')) ||
        (isCollabSigner &&
          viewedDocument?.isIncomplete &&
          isSignerOnDoc &&
          (isFaceToFaceDoc(viewedDocument) ||
            viewedDocument.template.templateType.shortName ===
              'Certification' ||
            viewedDocument.template.templateType.shortName ===
              'Recertification'))
      }
      onClick={async () => {
        if (canApprove) {
          approve();
        } else if (
          canAssign ||
          (isCollabSigner && viewedDocument?.isIncomplete && isSignerOnDoc)
        ) {
          openModal('reassign');
        }
      }}
      colorScheme="teal"
      data-cy="approve-btn"
    >
      {isApproved ? 'Unapprove' : 'Approve'}
    </Button>
  ) : null;
}
