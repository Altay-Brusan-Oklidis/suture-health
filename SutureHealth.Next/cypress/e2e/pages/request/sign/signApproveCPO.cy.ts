describe('CPO Modal', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should be visible and closable', () => {
    cy.visitInboxPage();

    cy.getById('sign-with-cpo').click();
    cy.getById('cpo-modal').should('be.visible');
    cy.getById('cancel-cpo').click();
    cy.getById('cpo-modal').should('not.exist');
  });

  it('should set a selected organization', () => {
    cy.visitInboxPage();

    cy.getById('organization-btn').click();
    cy.getById('org-menu')
      .find('button:last')
      .invoke('text')
      .then((orgName) => {
        cy.getById('org-menu').find('button:last').click();

        cy.getById('sign-with-cpo').click();
        cy.getById('cpo-modal')
          .as('cpoModal')
          .within(() => {
            cy.getByName('organizationId')
              .find(':selected')
              .should('have.text', orgName);
          });
      });
  });

  describe('sign a document with the signer role', () => {
    beforeEach(() => {
      cy.mockCompletedDocs();
    });

    it('should sign with cpo', () => {
      cy.visitInboxPage();

      cy.getById('sign-with-cpo').should('have.text', 'Sign | CPO');
      cy.getById('sign-with-cpo').click();

      cy.getById('cpo-modal')
        .as('cpoModal')
        .within(() => {
          cy.getByName('date').should('exist');
          cy.getByName('patient').should('exist');
          cy.getByName('organizationId').as('location').should('exist');
          cy.getByName('entryTypeId').should('exist');
          cy.getByName('cpoTypeIds').as('types').should('exist');
          cy.getByName('comments').should('exist');
          cy.getByName('minutes').as('minutes').should('exist');

          cy.get('@location').select(1, { force: true });
          cy.get('@types').check('1103', { force: true });
          cy.get('@minutes').should('have.value', '30');

          cy.getById('submit-cpo').should('have.text', 'Sign w/ CPO');
          cy.getById('submit-cpo').click();
        });

      cy.wait('@cpo');
      cy.get('@cpoModal').should('not.exist');
    });
  });

  describe('sign/approve for an assistant', () => {
    beforeEach(() => {
      cy.useRole('assistant');
    });

    it('should approve with cpo', () => {
      cy.visitInboxPage();

      cy.getById('sign-with-cpo').should('have.text', 'Approve | CPO');
      cy.getById('sign-with-cpo').click();
      cy.getById('cpo-modal')
        .as('cpoModal')
        .within(() => {
          cy.getByName('organizationId').select(1, { force: true });
          cy.getByName('signatureEnabled').should('not.exist');
          cy.getByName('cpoTypeIds').check('1103', { force: true });

          cy.getById('submit-cpo').should('have.text', '+ Approve with CPO');
          cy.getById('submit-cpo').click();
        });

      cy.wait('@cpo');
      cy.get('@cpoModal').should('not.exist');
    });

    it('should sign with cpo', () => {
      cy.mockCompletedDocs();
      cy.visitInboxPage();

      cy.getById('sign-with-cpo').should('have.text', 'Sign/Approve | CPO');
      cy.getById('sign-with-cpo').click();
      cy.getById('cpo-modal')
        .as('cpoModal')
        .within(() => {
          cy.getByName('organizationId').select(1, { force: true });
          cy.getById('submit-cpo').should('have.text', '+ Approve with CPO');
          cy.getByName('signatureEnabled')
            .should('exist')
            .click({ force: true });
          cy.getByName('cpoTypeIds').check('1103', { force: true });
          cy.getByName('password').should('exist').type('password');
          cy.getById('submit-cpo').should('have.text', 'Sign w/ CPO');

          cy.getById('submit-cpo').click();
        });

      cy.wait('@cpo');
      cy.get('@cpoModal').should('not.exist');
    });

    it('should validate the form', () => {
      cy.mockCompletedDocs();
      cy.visitInboxPage();

      cy.getById('sign-with-cpo').click();
      cy.getById('cpo-modal').within(() => {
        cy.getByName('signatureEnabled').click({ force: true });
        cy.getById('submit-cpo').click();

        cy.getByName('organizationId').should('have.attr', 'aria-invalid');
        cy.getByName('cpoTypeIds').should('have.attr', 'aria-invalid');
        cy.getByName('password').should('have.attr', 'aria-invalid');
      });
    });
  });

  describe('sign with cpo face-to-face document', () => {
    beforeEach(() => {
      cy.mockFaceToFaceDocuments();
    });

    it('should sign face-to-face document with cpo', () => {
      cy.visitInboxPage();

      cy.wait('@getFaceToFace');

      cy.getById('face-to-face-form').should('exist');
      cy.getById('sign-with-cpo').click();
      cy.getById('cpo-modal')
        .as('cpoModal')
        .within(() => {
          cy.getByName('organizationId').select(1, { force: true });
          cy.getByName('cpoTypeIds').check('1103', { force: true });
        });
      cy.getById('submit-cpo').click();

      cy.wait('@cpo');
      cy.get('@cpoModal').should('not.exist');
    });
  });
});

export {};
