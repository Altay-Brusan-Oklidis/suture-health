describe('Sign a document', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
    cy.mockCompletedDocs();
  });

  it('should sign a document', () => {
    cy.visitInboxPage();

    cy.getById('sign-btn').click();
    cy.wait('@signDoc');
  });

  describe('sign the face-to-face document', () => {
    beforeEach(() => {
      cy.mockFaceToFaceDocuments();
    });

    it('should sign the face-to-face document', () => {
      cy.mockFaceToFaceDocuments();
      cy.visitInboxPage();

      cy.getById('face-to-face-form').should('exist');
      cy.getById('sign-btn').click();
    });
  });

  describe('sign a document for an assistant', () => {
    beforeEach(() => {
      cy.useRole('assistant');
    });

    it('should sign a document for an assistant', () => {
      cy.visitInboxPage();

      cy.getById('sign-btn').click();
      cy.getById('sign-modal').as('signModal').should('be.visible');
      cy.getByName('password').type('password');
      cy.getById('sign-submit').click();

      cy.get('@signModal').should('not.exist');
    });

    it('should show an error', () => {
      cy.on('uncaught:exception', () => false);
      cy.intercept(
        { method: 'POST', url: /\/api\/inbox\/\d+\/sign/, middleware: true },
        (req) => {
          req.reply({
            statusCode: 400,
            body: { signerPassword: ['incorrect'] },
          });
        }
      ).as('signDoc');
      cy.visitInboxPage();

      cy.getById('sign-btn').click();
      cy.getById('sign-modal').as('signModal').should('be.visible');
      cy.getByName('password').as('password').type('password');
      cy.getById('sign-submit').click();
      cy.wait('@signDoc');
      cy.getByName('password').should('have.attr', 'aria-invalid');
    });

    it('should show the error modal', () => {
      cy.on('uncaught:exception', () => false);
      cy.intercept(
        { method: 'POST', url: /\/api\/inbox\/\d+\/sign/, middleware: true },
        (req) => {
          req.reply({
            statusCode: 400,
            body: { signerPassword: ['must reset their password'] },
          });
        }
      ).as('signDoc');
      cy.visitInboxPage();

      cy.getById('sign-btn').click();
      cy.getById('sign-modal').as('signModal').should('be.visible');
      cy.getByName('password').as('password').type('password');
      cy.getById('sign-submit').click();
      cy.wait('@signDoc');
      cy.getById('sign-error-modal').as('errorModal').should('exist');
      cy.getById('close-sign-error-modal').click();
      cy.get('@errorModal').should('not.exist');
    });
  });
});

export {};
