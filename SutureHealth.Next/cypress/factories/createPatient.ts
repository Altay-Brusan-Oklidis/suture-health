import { RequestDocumentType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';

export default function createPatient(
  override?: PartialDeep<RequestDocumentType['document']['patient']>
) {
  const firstName = faker.person.firstName();
  const lastName = faker.person.lastName();
  const patient: RequestDocumentType['document']['patient'] = {
    firstName,
    lastName,
    patientId: faker.number.int(),
    middleName: faker.person.middleName(),
    suffix: '',
    birthdate: faker.date.birthdate().toString(),
    payerMix: faker.helpers.multiple(
      () => {
        const payerName = faker.helpers.arrayElement([
          'Medicare',
          'Unavailable',
        ]);

        return {
          name: payerName,
          value: payerName !== 'Unavailable' ? faker.string.uuid() : payerName,
        };
      },
      { count: faker.number.int({ min: 1, max: 2 }) }
    ),
  };

  return merge(patient, override);
}
