import { RequestDocumentType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';
import createApproval from './createApproval';
import createMember from './createMember';
import createPatient from './createPatient';
import createStatusDisplay from './createStatusDisplay';
import createMemberOrganization from './createMemberOrganization';

export default function createDocumentView(
  override?: PartialDeep<RequestDocumentType>
) {
  const templateType = {
    signerChangeAllowed: true,
    dateAssociation: 'EffectiveDate',
    templateTypeId: 1005,
    category: 'Hospice',
    name: 'Hospice Interim Order',
    shortName: 'Interim Order',
  };
  const templateId = 548427;
  const documentView: RequestDocumentType = {
    document: {
      age: faker.number.int({ min: 18, max: 100 }),
      approvals:
        faker.helpers.maybe(() =>
          faker.helpers.multiple(createApproval, { count: 1 })
        ) || [],
      assistant: faker.helpers.maybe(createMember) || null,
      collaborator: faker.helpers.maybe(createMember) || null,
      diagnosisCode: null,
      effectiveDate: faker.date.recent().toString(),
      isIncomplete: faker.datatype.boolean(),
      isPersonalDocument: faker.datatype.boolean(),
      isResent: faker.datatype.boolean(),
      memberBacklog: null,
      patient: createPatient(),
      relativeValueUnit: 0,
      revenueValue: 0,
      signer: createMember({ canSign: true }),
      signerOrganization: createMemberOrganization(),
      startOfCare: null,
      statusDisplay: createStatusDisplay(),
      submittedAt: faker.date.recent().toString(),
      submitter: createMember({ canSign: false }),
      submitterOrganization: createMemberOrganization(),
      sutureSignRequestId: faker.number.int(),
      template: {
        templateId,
        templateType,
      },
      viewedByMemberIds: [],
    },
    metadata: null,
    template: {
      annotations: [],
      templateType,
      templateId,
    },
    pages: [
      {
        base64: '',
        dimensions: {
          width: 826,
          height: 1169,
        },
        pageNumber: 1,
      },
    ],
  };

  return merge(documentView, override);
}
