import { z } from 'zod';

export const MemberListItem = z.object({
  memberId: z.number(),
  memberTypeId: z.number(),
  signingName: z.string(),
  userName: z.string(),
  firstName: z.string(),
  lastName: z.string(),
  middleName: z.string(),
  maidenName: z.string(),
  suffix: z.string(),
  professionalSuffix: z.string(),
  organizationId: z.number().optional(),
});

export type MemberListItemType = z.infer<typeof MemberListItem>;

export const OrganizationItem = z.object({
  organizationId: z.number(),
  city: z.string().nullable(),
  name: z.string().nullable(),
  stateOrProvince: z.string().nullable(),
});

export const OrganizationItemWithInternalName = OrganizationItem.extend({
  internalName: z.string().nullable(),
});

export const Template = z.object({
  templateId: z.number(),
  templateType: z.object({
    category: z.string(),
    name: z.string(),
    shortName: z.string(),
    templateTypeId: z.number(),
    signerChangeAllowed: z.boolean(),
  }),
});

export const HasOrganizationId = z.object({ organizationId: z.number() });

export const Associate = z.object({
  signer: MemberListItem,
  organizations: z.array(OrganizationItem),
  collaborators: z.array(MemberListItem),
  assistants: z.array(MemberListItem),
});

const Member = z.object({
  npi: z.string().nullable(),
  email: z.string(),
  mobileNumber: z.string(),
  canSign: z.boolean(),
  isCollaborator: z.boolean(),
  memberId: z.number(),
  memberTypeId: z.number(),
  signingName: z.string(),
  userName: z.string(),
  firstName: z.string(),
  lastName: z.string(),
  middleName: z.string(),
  maidenName: z.string(),
  suffix: z.string(),
  professionalSuffix: z.string(),
});

const Approval = z.object({
  approvedAt: z.string(),
  approver: MemberListItem,
});

const Approvals = z.array(Approval);

export type ApprovalType = z.infer<typeof Approval>;
export type ApprovalsType = z.infer<typeof Approvals>;

const statusDisplay = z.enum(['None', 'Warning', 'Critical']);

export const DocumentSummaryItem = z.object({
  sutureSignRequestId: z.number(),
  submittedAt: z.string(),
  memberBacklog: z
    .object({
      expiresAt: z.string().nullable(),
      closedAt: z.string().nullable(),
    })
    .nullable(),
  template: Template,
  approvals: Approvals,
  isIncomplete: z.boolean(),
  isPersonalDocument: z.boolean(),
  isResent: z.boolean(),
  revenueValue: z.number(),
  relativeValueUnit: z.number(),
  collaborator: MemberListItem.nullable(),
  signer: MemberListItem,
  signerOrganization: OrganizationItemWithInternalName,
  submitter: MemberListItem,
  submitterOrganization: z.object({
    organizationId: z.number(),
    name: z.string().nullable(),
    internalName: z.string().nullable(),
    city: z.string().nullable(),
    stateOrProvince: z.string().nullable(),
  }),
  patient: z.object({
    patientId: z.number(),
    firstName: z.string().nullable(),
    middleName: z.string().nullable(),
    lastName: z.string().nullable(),
    suffix: z.string().nullable(),
    birthdate: z.string().nullable(),
  }),
  viewedByMemberIds: z.array(z.number()),
  statusDisplay,
  assistant: Member.nullable(),
});

const Organization = z.object({
  parentId: z.number().nullable(),
  companyId: z.number(),
  organizationTypeId: z.number().nullable(),
  npi: z.string().nullable(),
  medicareNumber: z.string().nullable(),
  phoneNumber: z.string().nullable(),
  addressLine1: z.string().nullable(),
  addressLine2: z.string().nullable(),
  countryOrRegion: z.string().nullable(),
  postalCode: z.string().nullable(),
  organizationId: z.number(),
  name: z.string().nullable(),
  internalName: z.string().nullable(),
  city: z.string().nullable(),
  stateOrProvince: z.string().nullable(),
});

const PayerMix = z.object({
  name: z.string(),
  value: z.string().nullable(),
});

const Patient = z.object({
  payerMix: z.array(PayerMix),
  patientId: z.number(),
  firstName: z.string().nullable(),
  middleName: z.string().nullable(),
  lastName: z.string().nullable(),
  suffix: z.string().nullable(),
  birthdate: z.string().nullable(),
});

export type PatientType = z.infer<typeof Patient>;

const Page = z.object({
  pageNumber: z.number(),
  base64: z.string().nullable(),
  dimensions: z.object({ height: z.number(), width: z.number() }),
});

export enum AnnotationType {
  VisibleSignature = 1,
  DateSigned = 2,
  CheckBox = 3,
  TextArea = 4,
}

const Annotation = z.object({
  annotationTypeId: z.nativeEnum(AnnotationType),
  left: z.number(),
  bottom: z.number(),
  right: z.number(),
  top: z.number(),
  pageNumber: z.number(),
  value: z.string().or(z.boolean()).nullable(),
  id: z.string().optional(),
});

const template = z.object({
  templateId: z.number(),
  templateType: z.object({
    category: z.string(),
    name: z.string(),
    shortName: z.string(),
    signerChangeAllowed: z.boolean(),
    templateTypeId: z.number(),
    dateAssociation: z.string().nullable(),
  }),
  annotations: z.array(Annotation),
});

const document = z.object({
  age: z.number(),
  approvals: Approvals,
  effectiveDate: z.string().nullable(),
  diagnosisCode: z
    .object({
      code: z.string().nullable(),
      description: z.string().nullable(),
    })
    .nullable(),
  startOfCare: z.string().nullable(),
  memberBacklog: z
    .object({
      expiresAt: z.string().nullable(),
      closedAt: z.string().nullable(),
    })
    .nullable(),
  collaborator: Member.nullable(),
  assistant: Member.nullable(),
  patient: Patient,
  signer: Member,
  submitter: Member,
  signerOrganization: Organization,
  submitterOrganization: Organization,
  sutureSignRequestId: z.number(),
  submittedAt: z.string(),
  statusDisplay,
  template: Template,
  isIncomplete: z.boolean(),
  isPersonalDocument: z.boolean(),
  isResent: z.boolean(),
  revenueValue: z.number(),
  relativeValueUnit: z.number(),
  viewedByMemberIds: z.array(z.number()),
});

const RequestDocument = z.object({
  document,
  pages: z.array(Page),
  template,
  metadata: z
    .string()
    .or(
      z.object({
        clinicalReason: z.string(),
        encounterDate: z.string(),
        homeboundReason: z.string(),
        medicalCondition: z.string(),
        skilledServices: z.array(z.string()),
        treatmentPlan: z.string().nullable(),
      })
    )
    .nullable(),
});

const MetaElement = z.object({
  faceToFaceDescriptorId: z.number(),
  description: z.string(),
  sequenceNumber: z.number(),
  isPlaceholder: z.boolean(),
});

const HasCategory = z.object({ category: z.string() });

const ClinicalReason = MetaElement.merge(HasCategory);

const SkillService = z.record(z.string(), z.string());

const FaceToFaceMetada = z.object({
  medicalConditions: z.array(MetaElement),
  clinicalReasons: z.array(ClinicalReason),
  homeboundReasons: z.array(MetaElement),
  skilledServices: SkillService,
});

const OrganizationContact = z.object({
  isActive: z.boolean(),
  isPrimary: z.boolean(),
  parentId: z.number().nullable(),
  id: z.number(),
  type: z.number(),
  value: z.string().nullable(),
});

const OrganizationOption = z.object({
  organizationId: z.number(),
  npi: z.string().nullable(),
  medicareNumber: z.string().nullable(),
  parentId: z.number().nullable(),
  companyId: z.number(),
  organizationTypeId: z.number(),
  name: z.string(),
  otherDesignation: z.string().nullable(),
  isActive: z.boolean(),
  isFree: z.boolean(),
  isSender: z.boolean(),
  isSigner: z.boolean(),
  addressLine1: z.string().nullable(),
  addressLine2: z.string().nullable(),
  city: z.string().nullable(),
  stateOrProvince: z.string().nullable(),
  countryOrRegion: z.string().nullable(),
  postalCode: z.string().nullable(),
  createdAt: z.string().nullable(),
  updatedAt: z.string().nullable(),
  closedAt: z.string().nullable(),
  organizationType: z.string().nullable(),
  contacts: z.array(OrganizationContact),
});

const Reason = z.object({
  rejectionReasonId: z.number(),
  description: z.string(),
  sequenceNumber: z.number(),
});

const User = z.object({
  memberId: z.number(),
  firstName: z.string(),
  lastName: z.string(),
  suffix: z.string(),
  signingName: z.string(),
  email: z.string(),
  isCollaborator: z.boolean(),
  isPayingClient: z.boolean(),
  memberTypeId: z.number(),
  npi: z.string().nullable(),
  userName: z.string(),
  lastLoggedInAt: z.string(),
  canSign: z.boolean(),
  isActive: z.boolean(),
  maidenName: z.string(),
  middleName: z.string(),
  mobileNumber: z.string(),
  professionalSuffix: z.string(),
});

export const CPOType = z.object({
  cpoTypeId: z.number(),
  description: z.string().nullable(),
  sequenceNumber: z.number(),
  minutes: z.number(),
  organizationId: z.number().nullable(),
});

const Contact = z.object({
  isActive: z.boolean(),
  isPrimary: z.boolean(),
  parentId: z.number(),
  id: z.number(),
  type: z.number(),
  value: z.string(),
});

const PrimaryOrg = z.object({
  organizationId: z.number(),
  npi: z.string(),
  medicareNumber: z.string(),
  phoneNumber: z.string(),
  parentId: z.number().nullable(),
  companyId: z.number(),
  organizationTypeId: z.number(),
  name: z.string(),
  otherDesignation: z.string(),
  isActive: z.boolean(),
  isFree: z.boolean(),
  addressLine1: z.string(),
  addressLine2: z.string(),
  city: z.string(),
  stateOrProvince: z.string(),
  countryOrRegion: z.string(),
  postalCode: z.string(),
  createdAt: z.string(),
  updatedAt: z.string(),
  closedAt: z.string().nullable(),
  organizationType: z.unknown().nullable(),
  contacts: z.array(Contact),
});

export const ReasonsData = z.array(Reason);
export const TemplateTypesData = z.array(Template.shape.templateType);
export const Organizations = z.array(OrganizationOption);
export const CPOTypesData = z.array(CPOType);
export const AssociatesData = z.array(Associate);

export type SignerOrganizationType = {
  organizationId: number;
  name: string;
};

export type PrimaryOrgType = z.infer<typeof PrimaryOrg>;
export type UserType = z.infer<typeof User>;
export type TemplateTypesDataType = z.infer<typeof TemplateTypesData>;
export type ReasonsDataType = z.infer<typeof ReasonsData>;
export type OrganizationsType = z.infer<typeof Organizations>;
export type OrganizationOptionType = z.infer<typeof OrganizationOption>;
export type FaceToFaceMetadaType = z.infer<typeof FaceToFaceMetada>;
export type CPOTypesDataType = z.infer<typeof CPOTypesData>;
export type AssociatesDataType = z.infer<typeof AssociatesData>;
export type PageType = z.infer<typeof Page>;
export type AnnotationItem = z.infer<typeof Annotation>;
export type RequestDocumentType = z.infer<typeof RequestDocument>;
export type DocumentSummaryItemType = z.infer<typeof DocumentSummaryItem>;
export type MemberType = z.infer<typeof Member>;
export type OrganizationType = z.infer<typeof Organization>;
