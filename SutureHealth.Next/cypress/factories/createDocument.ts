import { DocumentSummaryItemType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';
import createApproval from './createApproval';
import createMember from './createMember';
import createPatient from './createPatient';
import createStatusDisplay from './createStatusDisplay';
import createMemberOrganization from './createMemberOrganization';

export default function createDocument(
  override?: PartialDeep<DocumentSummaryItemType>
) {
  const { approvals, ...restOverride } = override || {};
  const templateType = {
    signerChangeAllowed: true,
    dateAssociation: 'EffectiveDate',
    templateTypeId: 1005,
    category: 'Hospice',
    name: 'Hospice Interim Order',
    shortName: 'Interim Order',
  };
  const templateId = 548427;
  const document: DocumentSummaryItemType = {
    approvals:
      faker.helpers.maybe(() =>
        faker.helpers.multiple(createApproval, { count: 1 })
      ) || [],
    assistant: faker.helpers.maybe(createMember) || null,
    collaborator: faker.helpers.maybe(createMember) || null,
    isIncomplete: faker.datatype.boolean(),
    isPersonalDocument: true,
    isResent: faker.datatype.boolean(),
    memberBacklog: null,
    patient: createPatient(),
    relativeValueUnit: 0,
    revenueValue: 0,
    signer: createMember({ canSign: true }),
    signerOrganization: createMemberOrganization(),
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
  };

  if (approvals && approvals.length === 0) {
    document.approvals = [];
  }

  return merge(document, { ...restOverride, approvals });
}
