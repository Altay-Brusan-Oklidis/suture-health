import { MemberType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';

export default function createMember(override?: PartialDeep<MemberType>) {
  const firstName = faker.person.firstName();
  const lastName = faker.person.lastName();
  const professionalSuffix = 'MD';
  const member: MemberType = {
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
    canSign: faker.datatype.boolean(),
    email: faker.internet.email({ firstName, lastName }),
    isCollaborator: faker.datatype.boolean(),
    mobileNumber: faker.phone.number(),
    npi: null,
  };

  return merge(member, override);
}
