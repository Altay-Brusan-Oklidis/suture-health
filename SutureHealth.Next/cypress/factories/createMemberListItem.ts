import { MemberListItemType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';

export default function createMemberListItem(
  override?: PartialDeep<MemberListItemType>
) {
  const firstName = faker.person.firstName();
  const lastName = faker.person.lastName();
  const professionalSuffix = 'MD';
  const member: MemberListItemType = {
    firstName,
    lastName,
    maidenName: '',
    memberId: faker.number.int(),
    memberTypeId: 2000,
    middleName: faker.person.middleName(),
    professionalSuffix,
    signingName: `${firstName} ${lastName}, ${professionalSuffix}`,
    suffix: '',
    userName: faker.internet.userName({ firstName, lastName }),
    organizationId: faker.number.int(),
  };

  return merge(member, override);
}
