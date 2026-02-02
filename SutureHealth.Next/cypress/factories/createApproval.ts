import { ApprovalType } from '@utils/zodModels';
import merge from 'lodash.merge';
import { faker } from '@faker-js/faker';
import createMemberListItem from './createMemberListItem';

export default function createApproval(override?: PartialDeep<ApprovalType>) {
  const approval: ApprovalType = {
    approvedAt: faker.date.recent().toString(),
    approver: createMemberListItem(),
  };

  return merge(approval, override);
}
