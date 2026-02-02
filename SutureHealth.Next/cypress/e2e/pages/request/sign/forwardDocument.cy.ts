describe('Forward Modal', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should be visible and closable', () => {
    cy.visitInboxPage();

    cy.getById('forward-open-btn').click();
    cy.getById('forward-modal').as('modal').should('be.visible');
    cy.getById('forward-cancel').click();
    cy.get('@modal').should('not.exist');
  });

  it('should forward the document', () => {
    cy.visitInboxPage();

    cy.getById('forward-open-btn').click();

    cy.getById('forward-modal')
      .as('modal')
      .within(() => {
        cy.getByName('signerMemberId').should('exist').select(1);
        cy.getByName('organizationId').should('exist').select(1);
        cy.getByName('collaboratorMemberId').should('exist');
        cy.getByName('assistantMemberId').should('exist');
        cy.getById('forward-submit').click();
      });

    cy.get('@modal').should('not.exist');
  });
});

export {};
