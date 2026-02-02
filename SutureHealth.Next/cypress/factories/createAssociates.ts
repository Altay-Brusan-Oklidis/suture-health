import { AssociatesDataType, MemberListItemType } from '@utils/zodModels';
import { faker } from '@faker-js/faker';
import createMemberListItem from './createMemberListItem';
import createMemberOrganization from './createMemberOrganization';

export default function createAssociates(
  organizationId: number,
  signer: MemberListItemType
) {
  const associates: AssociatesDataType = faker.helpers.multiple(
    () => ({
      assistants: faker.helpers.multiple(
        () => createMemberListItem({ organizationId }),
        { count: 5 }
      ),
      collaborators: faker.helpers.multiple(
        () => createMemberListItem({ organizationId }),
        { count: 5 }
      ),
      organizations: faker.helpers.multiple(
        () => createMemberOrganization({ organizationId }),
        { count: 5 }
      ),
      signer: createMemberListItem({ organizationId }),
    }),
    { count: 5 }
  );

  associates.push({
    assistants: [createMemberListItem({ organizationId })],
    collaborators: [createMemberListItem({ organizationId })],
    organizations: [createMemberOrganization({ organizationId })],
    signer: createMemberListItem({ ...signer, organizationId }),
  });

  return associates;
}
