describe('Sorting', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should change the sort value', () => {
    cy.visitInboxPage();

    cy.getById('sort-btn')
      .as('sortBtn')
      .invoke('text')
      .should('be.eq', 'Date Received');
    cy.get('@sortBtn').click();

    cy.getById('sort-dropdown').within(() => {
      cy.contains('patient', { matchCase: false }).click();
      cy.get('@sortBtn').invoke('text').should('be.eq', 'Patient');

      cy.get('@sortBtn').click();
      cy.contains('sender', { matchCase: false }).click();
      cy.get('@sortBtn').invoke('text').should('be.eq', 'Sender');

      cy.get('@sortBtn').click();
      cy.contains('signer', { matchCase: false }).click();
      cy.get('@sortBtn').invoke('text').should('be.eq', 'Signer');
    });

    cy.get('[aria-label="sort direction"]').as('sortDirectionBtn');
    cy.get('[aria-label="sort direction"] path')
      .invoke('attr', 'd')
      .then((path) => {
        cy.get('@sortDirectionBtn').click();
        cy.get('[aria-label="sort direction"] path')
          .invoke('attr', 'd')
          .should('not.eq', path);
      });
  });
});

export {};
