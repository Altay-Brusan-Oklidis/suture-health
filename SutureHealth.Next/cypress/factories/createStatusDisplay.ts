import { DocStatus } from 'suture-theme';
import { faker } from '@faker-js/faker';

export default function createStatusDisplay(): DocStatus {
  const status: DocStatus[] = ['Critical', 'None', 'Warning'];

  return faker.helpers.arrayElement(status);
}
