import { OrganizationOptionType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';

export default function createOrganization(
  override?: PartialDeep<OrganizationOptionType>
) {
  const organization: OrganizationOptionType = {
    addressLine1: faker.location.streetAddress(),
    addressLine2: null,
    city: faker.location.city(),
    companyId: faker.number.int(),
    countryOrRegion: null,
    medicareNumber: null,
    name: faker.company.name(),
    npi: null,
    organizationId: faker.number.int(),
    organizationTypeId: 10003,
    parentId: null,
    postalCode: faker.location.zipCode(),
    stateOrProvince: faker.location.state({ abbreviated: true }),
    closedAt: null,
    contacts: [],
    createdAt: faker.date.recent().toString(),
    isActive: true,
    isFree: true,
    isSender: false,
    isSigner: false,
    organizationType: null,
    otherDesignation: faker.company.name(),
    updatedAt: faker.date.recent().toString(),
  };

  return merge(organization, override);
}
