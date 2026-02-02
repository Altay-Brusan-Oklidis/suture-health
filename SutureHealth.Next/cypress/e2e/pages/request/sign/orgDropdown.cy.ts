import createOrganization from '@factories/createOrganization';

describe('Organization Dropdown', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should set the organization from sessionStorage', () => {
    cy.mockStaticOrgs();

    cy.window().then(() => {
      sessionStorage.setItem('selectedOrganizationId', '1');
    });

    cy.visitInboxPage();

    cy.wait('@getOrganizations');
    cy.getById('organization-btn')
      .as('orgBtn')
      .invoke('text')
      .should('not.be.eq', 'Organizations');

    cy.get('@orgBtn').click();
    cy.getById('org-menu').within(() => {
      cy.contains('All').click();
    });
    cy.get('@orgBtn').invoke('text').should('eq', 'Organizations');
  });

  it('should set single organization', () => {
    cy.intercept(
      { method: 'GET', url: '/api/organizations', middleware: true },
      (req) => {
        req.reply({
          statusCode: 200,
          body: [
            createOrganization({
              organizationId: 11004,
              otherDesignation: 'Sign1 - Org',
            }),
          ],
        });
      }
    ).as('getOrganizations');
    cy.visitInboxPage();

    cy.wait('@getOrganizations');
    cy.getById('organization-btn').invoke('text').should('eq', 'Sign1 - Org');
  });
});

export {};
