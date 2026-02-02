import { createApi } from '@reduxjs/toolkit/query/react';
import { groupBy } from 'suture-theme';
import queryString from 'query-string';
import {
  AssociatesDataType,
  DocumentSummaryItemType,
  CPOTypesDataType,
  FaceToFaceMetadaType,
  OrganizationsType,
  ReasonsDataType,
  TemplateTypesDataType,
  UserType,
  AnnotationItem,
  MemberListItemType,
  PrimaryOrgType,
} from '@utils/zodModels';
import toOptionFormat from '@utils/toOptionFormat';
import baseQueryWithCamelize from '@utils/baseQueryWithCamelize';
import getQueryParams from '@utils/getQueryParams';
import fullNameWithSuffix from '@utils/fullNameWithSuffix';
import type {
  FilterData,
  ApprovalStatus,
  DocumentProcessStatus,
} from '@containers/Inbox';
import { Option } from '@formAdapters/Checkbox';

type DocumentsCount = {
  all: number;
  needsSignature: number;
  needsFillingOut: number;
  savedForLater: number;
};

export type AnnotationChangeAction = {
  documentReqId: number;
  annotations: AnnotationItem[];
};

export type RegisterCPOAction = {
  url: string;
  method: 'POST';
  body?: {};
};

export type CPOData = {
  data: {
    date: Date;
    organizationId: string;
    patientId: number;
    entryTypeId: string;
    cpoTypeIds: string[];
    comments: string;
  };
  documentId: number;
};

type ChangeDocStatus = {
  requestIds: number[];
  status: 'approve' | 'unapprove';
  approver?: UserType;
};

type InboxPreferences = {
  view: FilterData['documentType'];
  approval: ApprovalStatus;
  documentProcess: DocumentProcessStatus;
};

type UpdateTemplateTypeAction = {
  id: number;
  templateTypeId: number;
};

type SaveForLater = {
  id: number;
  memberId: number;
  expirationDate: string;
  documentProcessStatus: FilterData['documentProcessStatus'];
};

type MoveToInbox = {
  id: number;
  documentProcessStatus: FilterData['documentProcessStatus'];
};

type RemoveDocs = {
  documentIds: number[];
};

type RejectDocument = {
  id: number;
  reason: string;
  rejectionReasonIds?: number[];
};

type RejectDocuments = {
  requestIds: (string | number)[];
  reason: string;
  rejectionReasonIds?: number[];
};

type SignDocument = {
  id: number;
  signerPassword: string;
};

type SignDocuments = {
  ids: (string | number)[];
  signerPassword: string;
};

type ReassignDocument = {
  id: number;
  signerMemberId: string | number;
  organizationId: string | number;
  collaboratorMemberId?: string | number;
  assistantMemberId?: string | number;
};

type FaceToFaceMetadata = {
  id: number;
  type: number;
  metadata: {
    encounterDate: Date;
    medicalCondition: string;
    skilledServices: string[];
    clinicalReason: string;
    homeboundReason: string;
    treatmentPlan?: string | null;
  };
};

type Template = {
  templateTypeId: number;
  category: string;
  categoryName: string;
  name: string;
  shortName: string;
  documentCount: number;
};

type Patient = {
  patientId: number;
  firstName: string;
  middleName: string;
  lastName: string;
  suffix: string;
  birthdate: string;
  documentCount: number;
};

type SenderOrganization = {
  organizationId: number;
  organizationName: string;
  documentCount: number;
};

type FilterOptions = {
  templateTypes: Template[];
  signers: MemberListItemType[];
  collaborators: MemberListItemType[];
  senderOrganizations: SenderOrganization[];
  documentIds: number[];
};

type TransformedServerFilterOptions = Record<keyof FilterOptions, Option[]>;

export type TransformedFilterOptions = Record<
  keyof FilterOptions | 'isResent' | 'approval',
  Option[]
>;

type PatientOptions = {
  orgIds: string[];
  pageNumber: number;
  searchParam?: string;
};

export type MiscSettings = {
  showNewInbox: boolean;
  allowOtherToSignFromScreen: boolean;
  documentViewDuration: number;
  inboxPreference: number;
  lastSavedForLaterDate: string;
};

export type TourSettings = {
  userId: number;
  isFirstInboxTourDone: boolean;
};

export type TaskHistory = {
  taskId: number;
  requestId: number;
  userId: number;
  facilityId: number;
  actionId: number;
  templateId: number;
  patientId: number;
  submittedBy: number;
  submitterFirstName: string;
  submitterMiddleName: string;
  submitterLastName: string;
  submitterSuffix: string;
  organizationName: string;
  createDate: Date;
  submittedByFacility: number;
  actionText: string;
  actionDetail: string;
  actionDetailDateTime?: string;
  additionalInfoValue: string;
};

export type InboxMarketingSubscriptionRequest = {
  organizationId: number;
  active: boolean;
};

type DocumentHistory = {
  requestId: number;
  requestStatus: string;
  age: number;
  templateTypes: Template[];
  taskHistory: TaskHistory[];
};

type TaskHistoryOptions = {
  requestId: number;
  page: number;
};

export const inboxApiSlice = createApi({
  reducerPath: 'inboxApi',
  baseQuery: baseQueryWithCamelize,
  tagTypes: ['Documents', 'DocumentsCount', 'Document', 'TourSettings'],
  endpoints: (builder) => ({
    getDocuments: builder.query<DocumentSummaryItemType[], string>({
      query: (query) => `/inbox?${query}`,
      providesTags: ['Documents'],
    }),
    getAllDocuments: builder.query<DocumentSummaryItemType[], string>({
      query: (query) => `/inbox?${query}`,
    }),
    getDocumentsCount: builder.query<DocumentsCount, string[]>({
      query: (orgIds) => ({
        url: `/inbox/documentcounts?${queryString.stringify(
          { organizationIds: orgIds },
          { arrayFormat: 'index' }
        )}`,
      }),
      providesTags: ['DocumentsCount'],
    }),
    updateFaceToFaceMetadata: builder.mutation<void, FaceToFaceMetadata>({
      query: ({ id, type, metadata }) => ({
        url: `/inbox/${id}/metadata`,
        method: 'POST',
        body: {
          type,
          metadata,
        },
      }),
    }),
    changeDocumentAnnotations: builder.mutation<void, AnnotationChangeAction>({
      query: (action) => ({
        url: `/inbox/${action.documentReqId}/annotations`,
        method: 'POST',
        body: { annotations: action.annotations },
      }),
      invalidatesTags: ['Documents', 'DocumentsCount'],
    }),
    changeDocumentStatus: builder.mutation<void, ChangeDocStatus>({
      query: (action) => ({
        url:
          action.status === 'approve'
            ? `/inbox/${action.status}`
            : `/inbox/${action.requestIds[0]}/unapprove`,
        body:
          action.status === 'approve'
            ? { requestIds: action.requestIds }
            : undefined,
        method: 'POST',
      }),
      async onQueryStarted(
        { requestIds, status, approver },
        { dispatch, getState, queryFulfilled }
      ) {
        await queryFulfilled;

        if (status === 'unapprove') {
          const docs = getState().inboxApi.queries[
            `getDocuments("${getQueryParams()}")`
          ]?.data as DocumentSummaryItemType[];
          const updatedList = docs.map((doc) =>
            requestIds.includes(doc.sutureSignRequestId)
              ? ({
                  ...doc,
                  approvals: doc.approvals.filter(
                    (i) => i.approver.memberId === approver?.memberId
                  ),
                } as DocumentSummaryItemType)
              : doc
          );

          await dispatch(
            inboxApiSlice.util.upsertQueryData(
              'getDocuments',
              getQueryParams(),
              updatedList
            )
          );

          dispatch(
            inboxApiSlice.util.invalidateTags(['Documents', 'DocumentsCount'])
          );
        }
      },
    }),
    changeDocumentStatusCache: builder.mutation<object, ChangeDocStatus>({
      queryFn: () => ({ data: {} }),
      async onQueryStarted(
        { requestIds, status, approver },
        { dispatch, getState }
      ) {
        const docs = getState().inboxApi.queries[
          `getDocuments("${getQueryParams()}")`
        ]?.data as DocumentSummaryItemType[];
        const updatedList = docs.map((doc) =>
          requestIds.includes(doc.sutureSignRequestId)
            ? status === 'approve'
              ? ({
                  ...doc,
                  approvals: [
                    ...doc.approvals,
                    {
                      approvedAt: new Date().toUTCString(),
                      approver,
                    },
                  ],
                } as DocumentSummaryItemType)
              : ({
                  ...doc,
                  approvals: doc.approvals.filter(
                    (i) => i.approver.memberId === approver?.memberId
                  ),
                } as DocumentSummaryItemType)
            : doc
        );

        await dispatch(
          inboxApiSlice.util.upsertQueryData(
            'getDocuments',
            getQueryParams(),
            updatedList
          )
        );

        dispatch(
          inboxApiSlice.util.invalidateTags(['Documents', 'DocumentsCount'])
        );
      },
    }),
    submitCpoData: builder.mutation<void, CPOData>({
      query: (action) => ({
        url: `/inbox/${action.documentId}/cpo`,
        method: 'POST',
        body: action.data,
      }),
    }),
    registerCpoData: builder.mutation<void, RegisterCPOAction>({
      query: (action) => ({
        url: action.url,
        method: action.method,
        body: action.body,
      }),
    }),
    registerDocumentView: builder.mutation<void, number>({
      query: (action) => ({
        url: `/inbox/${action}/view`,
        method: 'POST',
      }),
    }),
    getAssociates: builder.query<AssociatesDataType, number>({
      query: (documentId) => `/inbox/${documentId}/associates`,
    }),
    getCPOTypes: builder.query<CPOTypesDataType, void>({
      query: () => '/inbox/cpo/types',
    }),
    getFaceToFaceMetadata: builder.query<FaceToFaceMetadaType, void>({
      query: () => '/inbox/metadata/facetoface',
    }),
    getOrganizations: builder.query<
      { organizations: OrganizationsType; organizationOptions: Option[] },
      void
    >({
      query: () => '/organizations',
      transformResponse: (data: OrganizationsType) => {
        const organizationOptions = toOptionFormat(
          data,
          'organizationId',
          'otherDesignation'
        );

        return { organizations: data, organizationOptions };
      },
    }),
    getRejectReasons: builder.query<ReasonsDataType, number>({
      query: (documentId) => `/inbox/${documentId}/reject/reasons`,
    }),
    getTemplateTypes: builder.query<TemplateTypesDataType, void>({
      query: () => '/inbox/templates/types',
    }),
    getMemberIdentity: builder.query<UserType, void>({
      query: () => '/me',
    }),
    getPrimaryOrgDetails: builder.query<PrimaryOrgType, void>({
      query: () => '/primaryorganization',
    }),
    getInboxMarketingSubscription: builder.query<Boolean, number>({
      query: (orgId) => `/issubscribedtoinboxmarketing?organizationId=${orgId}`,
    }),
    updateInboxMarketingSubscription: builder.mutation<
      void,
      InboxMarketingSubscriptionRequest
    >({
      query: (request) => ({
        url: `/updateinboxmarketingsubscription?organizationId=${request.organizationId}&active=${request.active}`,
        method: 'POST',
      }),
    }),
    updateMiscUserSettings: builder.mutation<void, Partial<MiscSettings>>({
      query: (settings) => ({
        url: '/settings/misc',
        method: 'POST',
        body: settings,
      }),
    }),
    getInboxPreferences: builder.query<InboxPreferences, void>({
      query: () => '/inbox/preferences',
      transformResponse: (data: InboxPreferences) =>
        Object.entries(data).reduce(
          (acc, [key, value]) => ({ ...acc, [key]: value.toLowerCase() }),
          {}
        ) as InboxPreferences,
    }),
    updateInboxPreferences: builder.mutation<void, InboxPreferences>({
      query: (preferences) => ({
        url: '/inbox/preferences',
        method: 'POST',
        body: preferences,
      }),
    }),
    updateDocumentTemplateType: builder.mutation<
      void,
      UpdateTemplateTypeAction
    >({
      query: ({ id, templateTypeId }) => ({
        url: `/inbox/${id}/type`,
        method: 'POST',
        body: { templateTypeId },
      }),
      invalidatesTags: ['Documents'],
    }),
    saveForLater: builder.mutation<void, SaveForLater>({
      query: ({ id, memberId, expirationDate }) => ({
        url: '/inbox/backlog',
        method: 'POST',
        body: {
          expirationDate,
          entries: [
            {
              memberId,
              sutureSignRequestId: id,
            },
          ],
        },
      }),
      async onQueryStarted(
        { id, expirationDate, documentProcessStatus },
        { dispatch, queryFulfilled, getState }
      ) {
        await queryFulfilled;

        const docs = getState().inboxApi.queries[
          `getDocuments("${getQueryParams()}")`
        ]?.data as DocumentSummaryItemType[];
        const miscSettings = getState().inboxApi.queries[
          'getMiscSettings()'
        ] as unknown as MiscSettings;

        if (documentProcessStatus === 'savedforlater') {
          const updatedList = docs.map((doc) =>
            doc.sutureSignRequestId === id
              ? {
                  ...doc,
                  memberBacklog: {
                    expiresAt: expirationDate,
                    closedAt: null,
                  },
                }
              : doc
          );

          await dispatch(
            inboxApiSlice.util.upsertQueryData(
              'getDocuments',
              getQueryParams(),
              updatedList
            )
          );
        } else {
          const updatedList = docs.filter(
            (doc) => doc.sutureSignRequestId !== id
          );

          await dispatch(
            inboxApiSlice.util.upsertQueryData(
              'getDocuments',
              getQueryParams(),
              updatedList
            )
          );
        }

        await dispatch(
          inboxApiSlice.util.upsertQueryData('getMiscSettings', undefined, {
            ...miscSettings,
            lastSavedForLaterDate: expirationDate,
          })
        );

        dispatch(
          inboxApiSlice.util.invalidateTags(['Documents', 'DocumentsCount'])
        );
      },
    }),
    moveToInbox: builder.mutation<void, MoveToInbox>({
      query: ({ id }) => ({
        url: '/inbox/backlog/complete',
        method: 'POST',
        body: {
          sutureSignRequestIds: [id],
        },
      }),
      async onQueryStarted(
        { id, documentProcessStatus },
        { dispatch, queryFulfilled, getState }
      ) {
        await queryFulfilled;

        const docs = getState().inboxApi.queries[
          `getDocuments("${getQueryParams()}")`
        ]?.data as DocumentSummaryItemType[];

        if (documentProcessStatus === 'savedforlater') {
          const updatedList = docs.filter(
            (doc) => doc.sutureSignRequestId !== id
          );

          await dispatch(
            inboxApiSlice.util.upsertQueryData(
              'getDocuments',
              getQueryParams(),
              updatedList
            )
          );
        } else {
          const updatedList = docs.map((doc) =>
            doc.sutureSignRequestId === id
              ? { ...doc, memberBacklog: null }
              : doc
          );

          await dispatch(
            inboxApiSlice.util.upsertQueryData(
              'getDocuments',
              getQueryParams(),
              updatedList
            )
          );
        }

        dispatch(
          inboxApiSlice.util.invalidateTags(['Documents', 'DocumentsCount'])
        );
      },
    }),
    removeDocs: builder.mutation<object, RemoveDocs>({
      queryFn: () => ({ data: {} }),
      async onQueryStarted({ documentIds }, { dispatch, getState }) {
        const docs = getState().inboxApi.queries[
          `getDocuments("${getQueryParams()}")`
        ]?.data as DocumentSummaryItemType[];
        let updatedList = [];

        updatedList = docs.filter(
          (doc) => !documentIds.includes(doc.sutureSignRequestId)
        );

        await dispatch(
          inboxApiSlice.util.upsertQueryData(
            'getDocuments',
            getQueryParams(),
            updatedList
          )
        );

        dispatch(
          inboxApiSlice.util.invalidateTags(['Documents', 'DocumentsCount'])
        );
      },
    }),
    rejectDocument: builder.mutation<void, RejectDocument>({
      query: ({ id, reason, rejectionReasonIds }) => ({
        url: `/inbox/${id}/reject`,
        method: 'POST',
        body: {
          reason,
          rejectionReasonIds,
        },
      }),
    }),
    rejectDocuments: builder.mutation<void, RejectDocuments>({
      query: ({ requestIds, reason, rejectionReasonIds }) => ({
        url: `/inbox/reject`,
        method: 'POST',
        body: {
          requestIds,
          reason,
          rejectionReasonIds,
        },
      }),
    }),
    signDocument: builder.mutation<void, SignDocument>({
      query: ({ id, signerPassword }) => ({
        url: `/inbox/${id}/sign`,
        method: 'POST',
        body: { signerPassword },
      }),
    }),
    signDocuments: builder.mutation<void, SignDocuments>({
      query: ({ ids, signerPassword }) => ({
        url: '/inbox/sign',
        method: 'POST',
        body: {
          requestIds: ids,
          signerPassword,
        },
      }),
    }),
    reassignDocument: builder.mutation<void, ReassignDocument>({
      query: ({
        id,
        organizationId,
        signerMemberId,
        assistantMemberId,
        collaboratorMemberId,
      }) => ({
        url: `/inbox/${id}/associates`,
        body: {
          organizationId,
          signerMemberId,
          assistantMemberId,
          collaboratorMemberId,
        },
        method: 'POST',
      }),
      invalidatesTags: ['Documents', 'DocumentsCount'],
    }),
    getFilterOptions: builder.query<TransformedFilterOptions, string[]>({
      query: (orgIds: []) =>
        `/inbox/filteroptions?${queryString.stringify(
          { organizationIds: orgIds },
          { arrayFormat: 'index' }
        )}`,
      transformResponse: (data: FilterOptions) =>
        ({
          ...Object.entries(data).reduce((acc, [key, value]) => {
            const typedKey = key as keyof FilterOptions;

            switch (typedKey) {
              case 'templateTypes': {
                return {
                  ...acc,
                  [typedKey]: Object.entries(
                    groupBy(value as Template[], 'categoryName')
                  ).reduce(
                    (nestedAcc, [nestedKey, nestedValue]) => [
                      ...nestedAcc,
                      {
                        label: nestedKey,
                        value: nestedValue.map((i) =>
                          i.templateTypeId.toString()
                        ),
                        options: toOptionFormat(
                          nestedValue,
                          'templateTypeId',
                          'shortName'
                        ),
                      },
                    ],
                    [] as Option[]
                  ),
                };
              }

              case 'collaborators': {
                return {
                  ...acc,
                  [typedKey]: [
                    { value: '0', label: 'Unassigned' },
                    ...toOptionFormat(
                      value as MemberListItemType[],
                      'memberId',
                      (i) => fullNameWithSuffix(i)
                    ),
                  ],
                };
              }

              case 'senderOrganizations': {
                return {
                  ...acc,
                  [typedKey]: toOptionFormat(
                    value as SenderOrganization[],
                    'organizationId',
                    'organizationName'
                  ),
                };
              }

              case 'signers': {
                return {
                  ...acc,
                  [typedKey]: toOptionFormat(
                    value as MemberListItemType[],
                    'memberId',
                    (i) => fullNameWithSuffix(i)
                  ),
                };
              }

              case 'documentIds': {
                return {
                  ...acc,
                  [typedKey]: toOptionFormat(
                    value as number[],
                    (i) => i,
                    (i) => i
                  ),
                };
              }

              default:
                return acc;
            }
          }, {} as TransformedServerFilterOptions),
          isResent: [{ label: 'Resent', value: 'true' }],
          approval: [
            { label: 'Approved', value: 'approved' },
            { label: 'Unapproved', value: 'unapproved' },
          ],
        } as TransformedFilterOptions),
    }),
    getPatientOptions: builder.query<Option[], PatientOptions>({
      query: ({ orgIds, pageNumber, searchParam }) =>
        `/inbox/patientoptions?${queryString.stringify(
          {
            organizationIds: orgIds,
            pageNumber,
            searchParam: searchParam || undefined,
          },
          { arrayFormat: 'index' }
        )}`,
      transformResponse: (data: Patient[]) =>
        toOptionFormat(
          data,
          'patientId',
          (i) => `${i.firstName}, ${i.lastName}`
        ),
    }),
    getMiscSettings: builder.query<MiscSettings, void>({
      query: () => ({
        url: '/settings/misc',
        method: 'GET',
      }),
    }),
    associateMe: builder.mutation<void, number[]>({
      query: (documentIds) => ({
        url: '/inbox/associateme',
        body: documentIds,
        method: 'POST',
      }),
    }),
    getTourSettings: builder.query<TourSettings, void>({
      query: () => ({
        url: '/settings/firstinboxtour',
        method: 'GET',
      }),
      providesTags: ['TourSettings'],
    }),
    doneTourSettings: builder.mutation<TourSettings, void>({
      query: () => ({
        url: '/settings/firstinboxtour',
        method: 'POST',
      }),
      invalidatesTags: ['TourSettings'],
    }),
    getDocumentHistory: builder.query<DocumentHistory, TaskHistoryOptions>({
      query: ({ requestId, page }) => ({
        url: `/inbox/${requestId}/documenthistory?pageNumber=${page}`,
        method: 'GET',
      }),
    }),
  }),
});

export const {
  useLazyGetAllDocumentsQuery,
  useLazyGetDocumentsQuery,
  useGetDocumentsCountQuery,
  useGetAssociatesQuery,
  useGetCPOTypesQuery,
  useGetFaceToFaceMetadataQuery,
  useGetOrganizationsQuery,
  useGetRejectReasonsQuery,
  useGetTemplateTypesQuery,
  useGetMemberIdentityQuery,
  useGetPrimaryOrgDetailsQuery,
  useChangeDocumentAnnotationsMutation,
  useUpdateFaceToFaceMetadataMutation,
  useSubmitCpoDataMutation,
  useRegisterDocumentViewMutation,
  useRegisterCpoDataMutation,
  useChangeDocumentStatusMutation,
  useChangeDocumentStatusCacheMutation,
  useGetInboxPreferencesQuery,
  useUpdateMiscUserSettingsMutation,
  useUpdateInboxPreferencesMutation,
  useUpdateDocumentTemplateTypeMutation,
  useLazyGetTemplateTypesQuery,
  useSaveForLaterMutation,
  useMoveToInboxMutation,
  useRemoveDocsMutation,
  useRejectDocumentMutation,
  useRejectDocumentsMutation,
  useSignDocumentMutation,
  useSignDocumentsMutation,
  useReassignDocumentMutation,
  useGetFilterOptionsQuery,
  useGetPatientOptionsQuery,
  useLazyGetPatientOptionsQuery,
  useGetMiscSettingsQuery,
  useAssociateMeMutation,
  useGetTourSettingsQuery,
  useDoneTourSettingsMutation,
  useLazyGetDocumentHistoryQuery,
  useGetInboxMarketingSubscriptionQuery,
  useUpdateInboxMarketingSubscriptionMutation,
} = inboxApiSlice;
