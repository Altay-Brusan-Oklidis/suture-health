describe('Add CPO Modal', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should be visible and closable', () => {
    cy.visitInboxPage();

    cy.getById('add-cpo').click();
    cy.getById('add-cpo-modal').should('be.visible');
    cy.getById('cancel-add-cpo').click();
    cy.getById('add-cpo-modal').should('not.exist');
  });

  it('should add cpo', () => {
    cy.visitInboxPage();

    cy.getById('add-cpo').click();
    cy.getById('add-cpo-modal')
      .as('cpoModal')
      .within(() => {
        cy.getByName('date').should('exist');
        cy.getByName('patient').should('exist');
        cy.getByName('organizationId').as('location').should('exist');
        cy.getByName('entryTypeId').as('entry').should('exist');
        cy.getByName('cpoTypeIds').as('types').should('exist');
        cy.getByName('comments').should('exist');
        cy.getByName('minutes').as('minutes').should('exist');

        cy.getByName('patient')
          .parent()
          .within(() => {
            cy.get('input').first().type('test', { force: true }).focus();
            cy.wait('@patientsSearch');
            cy.get('[role="button"]')
              .contains('Amit Test Eight Amit Test Eight')
              .should('be.visible')
              .click({ force: true });
          });
        cy.get('@location').select(1, { force: true });
        cy.get('@entry').check('1', { force: true });
        cy.get('@types').check('1103', { force: true });
        cy.get('@minutes').should('have.value', '30');

        cy.getById('submit-add-cpo').click();
      });

    cy.get('@cpoModal').should('not.exist');
  });
});

export {};
