/* eslint-disable cypress/no-unnecessary-waiting */
import round from '@utils/round';

const compareWidths = (width: number, scale: number) => {
  cy.get('@docView')
    .invoke('outerWidth')
    .then((newWidth) => {
      expect(round(newWidth as number, 1)).to.eq(round(width * scale, 1));
    });
};

describe('Zoom', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
  });

  it('should zoom in', () => {
    cy.visitInboxPage();

    cy.getById('doc-view').as('docView').should('exist');

    cy.getById('zoom-in').as('zoomIn');
    cy.get('@docView')
      .invoke('outerWidth')
      .then((width) => {
        cy.get('@zoomIn').click();
        cy.wait(200); // zoom animation
        compareWidths(width as number, 1.2);
        cy.get('@zoomIn').click();
        cy.wait(200);
        compareWidths(width as number, 1.4);
        cy.get('@zoomIn').click();
        cy.get('@zoomIn').click();
        cy.get('@zoomIn').should('be.disabled');
      });
  });

  it('should zoom out', () => {
    cy.visitInboxPage();

    cy.getById('doc-view').as('docView').should('exist');

    cy.getById('zoom-out').as('zoomOut');
    cy.getById('doc-view')
      .invoke('outerWidth')
      .then((width) => {
        cy.get('@zoomOut').click();
        cy.wait(200);
        compareWidths(width as number, 0.8);
        cy.get('@zoomOut').click();
        cy.wait(200);
        compareWidths(width as number, 0.6);
        cy.get('@zoomOut').click();
        cy.get('@zoomOut').should('be.disabled');
      });
  });
});
