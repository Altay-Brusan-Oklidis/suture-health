describe('Scroll Buttons', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should be scrollable', () => {
    cy.visitInboxPage();

    cy.getById('doc-card')
      .first()
      .should('have.css', 'background-color', 'rgb(30, 107, 232)');
    cy.getById('prev-doc-btn').as('prevBtn').should('be.disabled');
    cy.getById('next-doc-btn').as('nextBtn').click();
    cy.getById('doc-card')
      .eq(1)
      .should('have.css', 'background-color', 'rgb(30, 107, 232)');
    cy.getById('doc-card').last().click();
    cy.get('@nextBtn').should('be.disabled');
    cy.get('@prevBtn').click();
    cy.getById('doc-card')
      .eq(-1)
      .should('have.css', 'background-color', 'rgb(30, 107, 232)');
  });
});

export {};
