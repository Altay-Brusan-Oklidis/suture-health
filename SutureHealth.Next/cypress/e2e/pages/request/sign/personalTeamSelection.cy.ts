// https://dev.azure.com/suture/Suture/_workitems/edit/3062
describe('Personal/Team Selection', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  // Scenario 1-2
  it('personal/team filter should be selected on click', () => {
    cy.visitInboxPage();

    cy.wait('@getInboxPreferences');

    cy.getById('documents-filters').within(() => {
      cy.contains('team', { matchCase: false })
        .click()
        .children('[data-checked]')
        .should('exist');

      cy.contains('personal', { matchCase: false })
        .click()
        .children('[data-checked]')
        .should('exist');
    });
  });
});

export {};
