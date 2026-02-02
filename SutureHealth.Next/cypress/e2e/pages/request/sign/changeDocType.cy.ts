describe('Change the document template type modal', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should change the document template type', () => {
    cy.visitInboxPage();

    cy.getById('edit-template-type').click();
    cy.getById('template-select').click();
    cy.getById('template-list')
      .should('be.visible')
      .within(() => {
        cy.contains('Discharge Summary - Hospital').click();
      });
    cy.getById('template-type')
      .invoke('text')
      .should('eq', 'Discharge Summary - Hospital');
  });
});

export {};
