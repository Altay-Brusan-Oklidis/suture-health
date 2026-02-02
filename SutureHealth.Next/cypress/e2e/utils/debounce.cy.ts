/* eslint-disable cypress/no-unnecessary-waiting */
import debounce from '@utils/debounce';

describe('debounce', () => {
  it('execute just once', () => {
    const debouncedFunc = debounce(cy.stub().as('fn'), 10);

    debouncedFunc();
    cy.wait(4).then(() => {
      debouncedFunc();
    });

    cy.get('@fn').should('be.calledOnce');
  });

  it('execute twice', () => {
    const debouncedFunc = debounce(cy.stub().as('fn'), 10);

    debouncedFunc();
    cy.wait(10).then(() => {
      debouncedFunc();
    });

    cy.get('@fn').should('be.calledTwice');
  });
});
