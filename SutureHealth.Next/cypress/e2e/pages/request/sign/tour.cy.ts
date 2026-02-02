/* eslint-disable cypress/no-unnecessary-waiting */
describe('Tour', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it("shouldn't be visible if isFirstInboxTourDone = true", () => {
    cy.visitInboxPage();

    cy.get('[id="react-joyride-portal"]').should('not.exist');
  });

  it.only('should be visible if isFirstInboxTourDone = false and closable', () => {
    cy.mockFirstInboxTour();
    cy.visitInboxPage();

    cy.get('[id="react-joyride-portal"]').as('tour').should('exist');
    cy.getById('tour-step')
      .first()
      .invoke('attr', 'class')
      .should('include', 'active-tour-step');
    cy.getById('prev-tour-step').should('be.disabled');
    cy.getById('next-tour-step').click();
    cy.getById('tour-step')
      .first()
      .invoke('attr', 'class')
      .should('include', 'tour-step');
    cy.getById('tour-step')
      .eq(1)
      .invoke('attr', 'class')
      .should('include', 'active-tour-step');

    cy.getById('next-tour-step').click();
    cy.wait(100);
    cy.getById('next-tour-step').click();
    cy.wait(100);
    cy.getById('next-tour-step').click();
    cy.wait(100);
    cy.getById('next-tour-step').click();
    cy.wait(200);
    cy.getById('filter-sidebar')
      .invoke('attr', 'class')
      .should('include', 'opened');
    cy.getById('next-tour-step').click();
    cy.wait(200);
    cy.getById('filter-sidebar')
      .invoke('attr', 'class')
      .should('not.include', 'opened');
    cy.getById('prev-tour-step').click();
    cy.wait(200);
    cy.getById('filter-sidebar')
      .invoke('attr', 'class')
      .should('include', 'opened');
    cy.getById('prev-tour-step').click();
    cy.wait(200);
    cy.getById('filter-sidebar')
      .invoke('attr', 'class')
      .should('not.include', 'opened');

    cy.getById('close-tour').click();
    cy.get('@tour').should('not.exist');
  });
});

export {};
