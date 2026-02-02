/// <reference types="cypress" />
// ***********************************************
// This example commands.ts shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
import { faker } from '@faker-js/faker';
import {
  DocumentSummaryItemType,
  UserType,
  RequestDocumentType,
} from '@utils/zodModels';
import { TourSettings } from '@containers/Inbox/apiReducer';
import createDocumentView from '@factories/createDocumentView';
import createDocument from '@factories/createDocument';
import createApproval from '@factories/createApproval';
import createAssociates from '@factories/createAssociates';
import createMe from '@factories/createMe';
import createOrganization from '@factories/createOrganization';
import createMiscSettings from '@factories/createMiscSettings';

declare global {
  namespace Cypress {
    interface Chainable {
      getById(id: string): Chainable<JQuery<HTMLElement>>;
      getByName(name: string): Chainable<JQuery<HTMLElement>>;
      useRole(
        role: 'collaborator' | 'collaboratorSigner' | 'assistant'
      ): Chainable<void>;
      mockInboxInterceptors(): Chainable<void>;
      visitInboxPage(): Chainable<void>;
      approve(): Chainable<void>;
      unapprove(): Chainable<void>;
      mockFaceToFaceDocuments(): Chainable<void>;
      mockApprovedDocuments(): Chainable<void>;
      mockCompletedDocs(useMultipleSigners?: boolean): Chainable<void>;
      mockNonReassignableDocs(): Chainable<void>;
      mockNeedsFillingOutDoc(): Chainable<void>;
      mockStaticOrgs(): Chainable<void>;
      mockFirstInboxTour(): Chainable<void>;
    }
  }
}

let documents: DocumentSummaryItemType[] = [];
let documentObj: { [key: string]: RequestDocumentType } = {};
let user: UserType = createMe({ canSign: true, isCollaborator: false });
let miscSettings = createMiscSettings();

const getId = (url: string) => parseInt(url.match(/\d+/g)?.at(-1) || '0', 10);

Cypress.Commands.add('getById', (id) => cy.get(`[data-cy="${id}"]`));
Cypress.Commands.add('getByName', (name) => cy.get(`[name="${name}"]`));

Cypress.Commands.add('useRole', (role) => {
  Cypress.log({
    name: 'useRole',
    message: [`Use ${role} role`],
  });

  cy.intercept({ method: 'GET', url: '/api/me', middleware: true }, (req) => {
    let roleArgs: Partial<UserType> = {};

    switch (role) {
      case 'assistant': {
        roleArgs = {
          canSign: false,
          isCollaborator: false,
          memberTypeId: 1,
        };
        break;
      }

      case 'collaborator': {
        roleArgs = {
          canSign: false,
          isCollaborator: true,
          memberTypeId: 1,
        };
        break;
      }

      case 'collaboratorSigner': {
        roleArgs = {
          canSign: true,
          isCollaborator: true,
          memberTypeId: 1,
        };
        break;
      }

      default:
        break;
    }

    user = { ...user, ...roleArgs };

    req.reply({ body: user });
  }).as('me');
});

Cypress.Commands.add('mockInboxInterceptors', () => {
  documents = faker.helpers.multiple(
    () => createDocument({ approvals: [], signer: user, isIncomplete: true }),
    { count: 10 }
  );

  cy.intercept('GET', '/api/me', (req) => {
    req.reply({ body: user });
  }).as('me');

  cy.intercept('GET', '/api/organizations', (req) => {
    req.reply({
      body: [
        createOrganization({
          organizationId: documents[0]?.signerOrganization.organizationId,
        }),
        createOrganization(),
      ],
    });
  }).as('getOrganizations');

  cy.intercept(
    { method: 'GET', url: '/api/inbox?*', middleware: true },
    (req) => {
      req.reply({ statusCode: 200, body: documents });
    }
  ).as('getDocs');

  cy.intercept('GET', '/api/inbox/cpo/types', { fixture: 'cpoTypes' }).as(
    'getCpoTypes'
  );

  cy.intercept('GET', '/api/inbox/templates/types', {
    fixture: 'templateTypes',
  });
  cy.intercept({ method: 'POST', url: /\/api\/inbox\/\d+\/type/ }, (req) => {
    req.reply({ statusCode: 200 });
  }).as('updateTemplate');

  cy.intercept({ method: 'GET', url: /\/api\/inbox\/\d+$/ }, (req) => {
    const docId = getId(req.url);
    const doc = documents.find((i) => i.sutureSignRequestId === docId)!;

    documentObj = {
      ...documentObj,
      [docId]: documentObj[docId]
        ? documentObj[docId]
        : createDocumentView({ document: { ...doc } }),
    };

    req.reply({ body: documentObj[docId] });
  }).as('getDoc');

  cy.intercept(
    { method: 'GET', url: /\/api\/inbox\/\d+\/reject\/reasons/ },
    { fixture: 'rejectReasons' }
  ).as('getRejectReasons');
  cy.intercept({ method: 'POST', url: /\/api\/inbox\/\d+\/reject/ }, (req) => {
    documents = documents.filter(
      (i) => i.sutureSignRequestId !== getId(req.url)
    );

    req.reply({ statusCode: 200 });
  }).as('reject');

  cy.intercept('POST', '/api/inbox/*/unapprove', (req) => {
    documents = documents.map((i) =>
      i.sutureSignRequestId === getId(req.url)
        ? {
            ...i,
            approvals: i.approvals.filter(
              (approval) => approval.approver.memberId !== user.memberId
            ),
          }
        : i
    );

    req.reply({ statusCode: 200 });
  }).as('unapprove');

  cy.intercept('POST', '/api/inbox/approve', (req) => {
    const approval = {
      approvedAt: new Date().toUTCString(),
      approver: user,
    };

    documents = documents.map((i) =>
      req.body.requestIds.includes(i.sutureSignRequestId)
        ? {
            ...i,
            approvals: [...i.approvals, approval],
          }
        : i
    );
    req.body.requestIds.forEach((i: string) => {
      documentObj[i]!.document.approvals = [
        ...documentObj[i]!.document.approvals,
        approval,
      ];
    });

    req.reply({ statusCode: 200 });
  }).as('approve');

  cy.intercept('POST', '/api/inbox/sign', (req) => {
    documents = documents.filter(
      (i) => !req.body.requestIds.includes(i.sutureSignRequestId.toString())
    );

    req.reply({ statusCode: 200 });
  }).as('sign');

  cy.intercept({ method: 'POST', url: /\/api\/inbox\/\d+\/sign/ }, (req) => {
    documents = documents.filter(
      (i) => i.sutureSignRequestId !== getId(req.url)
    );

    req.reply({ statusCode: 200 });
  }).as('signDoc');

  cy.intercept({ method: 'POST', url: /\/api\/inbox\/\d+\/view/ }, (req) => {
    documents = documents.map((i) =>
      i.sutureSignRequestId === getId(req.url)
        ? {
            ...i,
            viewedByMemberIds: [...i.viewedByMemberIds, user.memberId],
          }
        : i
    );

    req.reply({ statusCode: 200 });
  }).as('viewDoc');

  cy.intercept('GET', '/api/inbox/metadata/facetoface', {
    fixture: 'faceToFace',
  }).as('getFaceToFace');
  cy.intercept('POST', '/api/inbox/*/metadata', (req) => {
    req.reply({ statusCode: 200 });
  });

  cy.intercept('GET', 'api/inbox/preferences', {
    fixture: 'inboxPreferences',
  }).as('getInboxPreferences');
  cy.intercept('POST', 'api/inbox/preferences', {
    fixture: 'inboxPreferences',
  });

  cy.intercept('GET', 'api/inbox/*/associates', (req) => {
    const docId = getId(req.url);
    const doc = documentObj[docId]!;

    req.reply({
      body: createAssociates(
        doc.document.signerOrganization.organizationId,
        doc.document.signer
      ),
    });
  }).as('getAssociates');
  cy.intercept('POST', 'api/inbox/*/associates', (req) => {
    req.reply({ statusCode: 200 });
  }).as('associates');

  cy.intercept('POST', 'api/inbox/*/cpo', (req) => {
    req.reply({ statusCode: 200 });
  }).as('cpo');
  cy.intercept('POST', 'api/inbox/cpo', (req) => {
    req.reply({ statusCode: 200 });
  }).as('cpo');

  cy.intercept('POST', 'patients/search', { fixture: 'patients' }).as(
    'patientsSearch'
  );

  cy.intercept('GET', '/api/settings/misc', (req) => {
    req.reply({ body: miscSettings });
  });
  cy.intercept('POST', '/api/settings/misc', (req) => {
    miscSettings = { ...miscSettings, ...req.body };
    req.reply({ statusCode: 200 });
  });

  cy.intercept('GET', '/api/inbox/documentcounts?*', {
    fixture: 'documentCounts',
  });

  cy.intercept('GET', '/api/inbox/filteroptions?*', {
    fixture: 'filterOptions',
  });

  cy.intercept('GET', '/api/inbox/patientoptions?*', {
    fixture: 'patientOptions',
  });

  cy.intercept('GET', '/api/settings/firstinboxtour', (req) => {
    const tourSettings: TourSettings = {
      isFirstInboxTourDone: true,
      userId: user.memberId,
    };

    req.reply({ body: tourSettings });
  });
  cy.intercept('POST', '/api/settings/firstinboxtour', (req) => {
    req.reply({ statusCode: 200 });
  });

  cy.intercept('GET', '/api/primaryorganization', (req) => {
    req.reply({ body: createOrganization() });
  });

  cy.setCookie('SutureHealth.AuthenticationCookie.CI', '');
});

Cypress.Commands.add('visitInboxPage', () => {
  cy.visit('/request/sign');

  cy.wait('@getDocs', { timeout: 10_000 });
  cy.wait('@getDoc', { timeout: 10_000 });
});

Cypress.Commands.add('approve', () => {
  cy.getById('approve-btn').click();

  cy.getById('success-animation').should('exist');
  cy.wait(['@approve', '@getDocs']);
  cy.getById('success-animation').should('not.exist');

  cy.getById('doc-card').first().click();
  cy.getById('approve-btn').should('not.exist');
  cy.getById('approved-icon').should('exist');
  cy.getById('header-approved-icon').should('exist');
});

Cypress.Commands.add('unapprove', () => {
  // TODO: beta release
});

Cypress.Commands.add('mockFaceToFaceDocuments', () => {
  documents = [
    createDocument({
      template: {
        templateId: 1001,
        templateType: {
          templateTypeId: 2,
          category: 'Home Health',
          name: 'Home Health Face-to-Face (non-primary)',
          shortName: 'Face-to-Face',
        },
      },
      isIncomplete: false,
      signer: user,
      isPersonalDocument: true,
    }),
  ];

  cy.intercept(
    { method: 'GET', url: /\/api\/inbox\/\d+$/, middleware: true },
    (req) => {
      const docId = getId(req.url);
      const doc = documents.find((i) => i.sutureSignRequestId === docId)!;

      documentObj = {
        ...documentObj,
        [docId]: documentObj[docId]
          ? documentObj[docId]
          : createDocumentView({
              document: { ...doc },
              template: { ...doc.template },
              metadata: {
                encounterDate: '2022-11-18T15:15:52.319Z',
                skilledServices: ['PhysicalTherapy', 'OccupationalTherapy'],
                medicalCondition: 'Abscess of: ',
                clinicalReason:
                  'Recent surgery requires SN for post-acute care (e.g. wound care, care to drains).',
                homeboundReason:
                  'Special transportation is required to leave home.',
                treatmentPlan: 'test',
              },
            }),
      };

      req.reply({ body: documentObj[docId] });
    }
  ).as('getDoc');
});

Cypress.Commands.add('mockApprovedDocuments', () => {
  documents = faker.helpers.multiple(
    () => createDocument({ approvals: [createApproval()], signer: user }),
    { count: 10 }
  );
});

Cypress.Commands.add('mockCompletedDocs', () => {
  documents = faker.helpers.multiple(
    () => createDocument({ isIncomplete: false, signer: user }),
    { count: 10 }
  );
});

Cypress.Commands.add('mockNonReassignableDocs', () => {
  documents = faker.helpers.multiple(
    () =>
      createDocument({
        template: { templateType: { signerChangeAllowed: false } },
        signer: user,
      }),
    { count: 10 }
  );
});

Cypress.Commands.add('mockNeedsFillingOutDoc', () => {
  documents = documents.map((i) => ({
    ...i,
    isIncomplete: true,
    isResent: true,
    approvals: [createApproval()],
    signer: user,
  }));

  cy.intercept(
    { method: 'GET', url: /\/api\/inbox\/\d+$/, middleware: true },
    (req) => {
      const docId = getId(req.url);
      const doc = documents.find((i) => i.sutureSignRequestId === docId)!;

      documentObj = {
        ...documentObj,
        [docId]: documentObj[docId]
          ? documentObj[docId]
          : createDocumentView({
              document: { ...doc, isIncomplete: true, isResent: true },
              template: {
                annotations: [
                  {
                    annotationTypeId: 4,
                    left: 320,
                    bottom: 995,
                    right: 400,
                    top: 954,
                    pageNumber: 1,
                    value: null,
                  },
                ],
              },
            }),
      };

      req.reply({ body: documentObj[docId] });
    }
  ).as('getDoc');
});

Cypress.Commands.add('mockStaticOrgs', () => {
  cy.intercept(
    { method: 'GET', url: '/api/organizations', middleware: true },
    (req) => {
      req.reply({
        body: [
          createOrganization({ organizationId: 1 }),
          createOrganization({ organizationId: 2 }),
        ],
      });
    }
  ).as('getOrganizations');
});

Cypress.Commands.add('mockFirstInboxTour', () => {
  cy.intercept(
    { method: 'GET', url: '/api/settings/firstinboxtour', middleware: true },
    (req) => {
      const tourSettings: TourSettings = {
        isFirstInboxTourDone: false,
        userId: user.memberId,
      };

      req.reply({ body: tourSettings });
    }
  );
});
