describe('Profile Menu', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should remove organization on logout', () => {
    cy.visitInboxPage();

    cy.window().then(() => {
      sessionStorage.setItem('selectedOrganizationId', '1');
    });

    cy.getById('user-badge').click();
    cy.getById('profile-menu').should('be.visible');
    cy.getById('logout-btn').click();
    cy.window().then(() => {
      expect(sessionStorage.getItem('selectedOrganizationId')).to.eq(null);
    });
  });
});

export {};
