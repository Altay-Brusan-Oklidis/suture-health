import { useMemo } from 'react';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';

export default function useMemberRole() {
  const { data } = useGetMemberIdentityQuery();
  const role = useMemo(() => {
    if (data) {
      if (
        !data.isCollaborator &&
        (data.memberTypeId === 2000 || data.canSign)
      ) {
        return 'Signer';
      }
      if (data.isCollaborator) {
        return data.canSign ? 'CollaboratorSigner' : 'Collaborator';
      }
      if (data.memberTypeId === 2003) {
        return 'Sender';
      }

      return 'Assistant';
    }

    return '';
  }, [data]);
  const roles = useMemo(
    () => ({
      isSigner: role === 'Signer',
      isCollaborator: role === 'Collaborator',
      isCollaboratorSigner: role === 'CollaboratorSigner',
      isSender: role === 'Sender',
      isAssistant: role === 'Assistant',
    }),
    [role]
  );

  return { role, ...roles };
}
