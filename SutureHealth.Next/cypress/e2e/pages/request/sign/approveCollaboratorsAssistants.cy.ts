// https://dev.azure.com/suture/Suture/_workitems/edit/3111
describe('Approve Action (Collaborators and Assistants)', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  // Scenario 1-2
  describe('approve a document for a non-Signer user', () => {
    beforeEach(() => {
      cy.useRole('assistant');

      cy.visitInboxPage();
    });

    it('should approve a document for an assistant', () => {
      cy.approve();
    });
  });

  // Scenario 3
  describe('approve a document for a collaborator', () => {
    beforeEach(() => {
      cy.useRole('collaborator');

      cy.visitInboxPage();
    });

    it('should approve a document for a collaborator', () => {
      cy.approve();
    });
  });

  // Scenario 4-6
  it('the approve button should be hidden for the signer', () => {
    cy.visitInboxPage();
    cy.getById('approve-btn').should('not.exist');
  });

  // Scenario 7
  describe('approve a document for a collaboratorSigner', () => {
    beforeEach(() => {
      cy.useRole('collaboratorSigner');

      cy.visitInboxPage();
    });

    it('should approve a document for a collaboratorSigner', () => {
      cy.getById('approve-btn').click();

      cy.getById('forward-modal')
        .as('reassignModal')
        .within(() => {
          cy.contains('OK & Approve', { matchCase: false })
            .as('submit')
            .should('be.disabled');
          cy.getByName('signerMemberId').select(1);
          cy.get('@submit').click({ force: true });
          cy.wait('@associates');
        });

      cy.get('@reassignModal').should('not.exist');
    });
  });
});

export {};
