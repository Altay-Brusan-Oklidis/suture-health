// https://dev.azure.com/suture/Suture/_workitems/edit/3110
describe('Reject action', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
    cy.visitInboxPage();
  });

  // Scenario 1-5
  it('should reject a document', () => {
    cy.getById('reject-btn').click();
    cy.wait('@getRejectReasons');
    cy.getById('reject-reasons').children().first().as('firstReason').click();
    cy.get('@firstReason')
      .invoke('text')
      .then((reason) => {
        cy.getById('reject-modal')
          .as('rejectModal')
          .find('textarea')
          .as('rejectReason')
          .should('have.text', reason);
      });
    cy.get('@rejectReason').type('test');
    cy.getById('submit-rejection').click();
    cy.get('@rejectModal').should('not.exist');
    cy.wait('@reject');
  });

  // Scenario 6
  it('should be closable', () => {
    cy.getById('reject-btn').click();
    cy.getById('rejection-modal-close').click();
    cy.getById('reject-modal').should('not.exist');
  });

  // Scenario 7
  it('should validate the form', () => {
    cy.getById('reject-btn').click();
    cy.getById('submit-rejection').click();
    cy.getById('reject-modal')
      .as('rejectModal')
      .find('textarea')
      .should('have.attr', 'aria-invalid');
    cy.contains(
      'You must provide at least one rejection reason in order to reject a document'
    ).should('exist');
  });
});

export {};
