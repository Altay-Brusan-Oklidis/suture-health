describe('Document Header', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  describe('UI stuff', () => {
    beforeEach(() => {
      cy.mockApprovedDocuments();
      cy.mockNeedsFillingOutDoc();
    });

    it('should have header tags', () => {
      cy.visitInboxPage();

      cy.getById('resent-tag').should('exist');
      cy.contains('This document has been resent').should('be.visible');
      cy.getById('header-approved-icon').as('approvedIcon').should('exist');
      cy.get('@approvedIcon');
      cy.getById('required-tag')
        .invoke('text')
        .should('eq', '1 Required Fields');
    });
  });

  it('should change signer', () => {
    cy.visitInboxPage();
    cy.wait('@getAssociates');

    cy.getById('signer-menu').click();

    cy.getById('portal-signer-menu').within(() => {
      cy.getById('menu-member-name').first().invoke('text').as('signerText');
      cy.getById('menu-member-name').first().click({ force: true });
      cy.contains('Confirm').click({ force: true });
    });

    cy.getById('portal-signer-menu').should('not.be.visible');
    cy.get('@signerText').then((text) => {
      cy.getById('signer-menu').within(() => {
        cy.getById('selected-member-menu').invoke('text').should('be.eq', text);
      });
    });
  });

  it('should change collaborator', () => {
    cy.visitInboxPage();
    cy.wait('@getAssociates');

    cy.getById('collaborator-menu').click();

    cy.getById('portal-collaborator-menu').within(() => {
      cy.getById('none-option').should('exist');
      cy.getById('menu-member-name')
        .first()
        .invoke('text')
        .as('collaboratorText');
      cy.getById('menu-member-name').first().click({ force: true });
    });

    cy.getById('portal-collaborator-menu').should('not.be.visible');
    cy.get('@collaboratorText').then((text) => {
      cy.getById('collaborator-menu').within(() => {
        cy.getById('selected-member-menu').invoke('text').should('be.eq', text);
      });
    });
  });

  it('should change assistant', () => {
    cy.visitInboxPage();
    cy.wait('@getAssociates');

    cy.getById('assistant-menu').click();

    cy.getById('portal-assistant-menu').within(() => {
      cy.getById('none-option').should('exist');
      cy.getById('menu-member-name').first().invoke('text').as('assistantText');
      cy.getById('menu-member-name').first().click({ force: true });
    });

    cy.getById('portal-assistant-menu').should('not.be.visible');
    cy.get('@assistantText').then((text) => {
      cy.getById('assistant-menu').within(() => {
        cy.getById('selected-member-menu').invoke('text').should('be.eq', text);
      });
    });
  });

  describe('reassign error', () => {
    beforeEach(() => {
      cy.mockNonReassignableDocs();
    });

    it('should show reassign error', { scrollBehavior: false }, () => {
      cy.visitInboxPage();
      cy.wait('@getAssociates');

      cy.getById('signer-menu').click();

      cy.getById('portal-signer-menu').within(() => {
        cy.getById('menu-member-name').first().click({ force: true });
        cy.contains('Confirm').click({ force: true });
      });
      cy.contains('You are unable to reassign this document type.').should(
        'be.visible'
      );
    });
  });
});

export {};
