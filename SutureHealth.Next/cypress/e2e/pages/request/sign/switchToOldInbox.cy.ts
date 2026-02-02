import createMiscSettings from '@factories/createMiscSettings';

describe('Switch to old inbox button', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should be visible if showNewInbox = true', () => {
    cy.visitInboxPage();

    cy.getById('switch-to-old-inbox').click();
    cy.getById('env-swap-modal').as('modal').should('be.visible');
    cy.getById('submit-env-swap-modal').should('be.disabled');
    cy.getById('close-env-swap-modal').click();
    cy.get('@modal').should('not.exist');
    cy.getById('switch-to-old-inbox').click();

    cy.getByName('reason').type('a');
    cy.getByName('reason').type('{backspace}');
    cy.getById('submit-env-swap-modal').should('be.disabled');

    cy.getById('reason')
      .first()
      .invoke('text')
      .then((optionText) => {
        cy.getById('reason').first().click();
        cy.getByName('reason').invoke('text').should('eq', optionText);
        cy.getById('submit-env-swap-modal').should('not.be.disabled');
      });
  });

  it("shouldn't be visible if showNewInbox = false", () => {
    cy.intercept(
      { method: 'GET', url: '/api/settings/misc', middleware: true },
      (req) => {
        req.reply({ body: createMiscSettings({ showNewInbox: false }) });
      }
    );

    cy.visitInboxPage();
    cy.getById('switch-to-old-inbox').should('not.exist');
  });
});

export {};
