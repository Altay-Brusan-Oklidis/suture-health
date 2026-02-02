// https://dev.azure.com/suture/Suture/_workitems/edit/3174
describe('Approved Status Tool-Tip', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
    cy.mockApprovedDocuments();
  });

  it('approved indicator should be visible', () => {
    cy.visitInboxPage();

    cy.getById('approved-icon').first().should('exist').trigger('pointerover');
    cy.getById('approved-tooltip').should('be.visible');
    cy.getById('header-approved-icon').should('exist').trigger('pointerover');
    cy.getById('approved-tooltip').should('be.visible');
  });
});

export {};
