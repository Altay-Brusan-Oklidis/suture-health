// https://dev.azure.com/suture/Suture/_workitems/edit/3059
describe('Face-To-Face Elements', () => {
  beforeEach(() => {
    cy.mockInboxInterceptors();
    cy.mockFaceToFaceDocuments();

    cy.visitInboxPage();
  });

  const checkFaceToFaceElements = (isInitialOrderReq = false) => {
    cy.wait('@getCpoTypes');
    cy.getById('face-to-face-form').within(() => {
      cy.getByName('encounterDate').should('exist');
      cy.getByName('medicalCondition').should('exist');
      cy.getByName('skilledServices').should('exist');
      cy.getByName('clinicalReason').should('exist');
      cy.getByName('homeboundReason').should('exist');

      if (isInitialOrderReq) cy.getByName('treatmentPlan').should('exist');
    });
  };

  // Scenario 1
  it('should have face to face elements', () => {
    checkFaceToFaceElements();
  });

  // Scenario 2
  it('should have face to face elements with request for initial orders', () => {
    checkFaceToFaceElements(true);
  });
});

export {};
