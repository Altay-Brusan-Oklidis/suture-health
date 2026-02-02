import { OrganizationType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';

export default function createMemberOrganization(
  override?: PartialDeep<OrganizationType>
) {
  const internalName = faker.company.name();
  const organization: OrganizationType = {
    addressLine1: faker.location.streetAddress(),
    addressLine2: null,
    city: faker.location.city(),
    companyId: faker.number.int(),
    countryOrRegion: null,
    internalName,
    medicareNumber: null,
    name: internalName,
    npi: null,
    organizationId: faker.number.int(),
    organizationTypeId: 10003,
    parentId: null,
    phoneNumber: faker.phone.number(),
    postalCode: faker.location.zipCode(),
    stateOrProvince: faker.location.state({ abbreviated: true }),
  };

  return merge(organization, override);
}
