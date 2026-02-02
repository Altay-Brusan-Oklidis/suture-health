/* eslint-disable cypress/no-unnecessary-waiting */
import throttle from '@utils/throttle';

describe('throttle', () => {
  it('execute just once', () => {
    const throttledFunc = throttle(cy.stub().as('fn'), 10);

    throttledFunc();
    cy.wait(4).then(() => {
      throttledFunc();
    });

    cy.get('@fn').should('be.calledOnce');
  });

  it('execute twice', () => {
    const throttledFunc = throttle(cy.stub().as('fn'), 10);

    throttledFunc();
    cy.wait(10).then(() => {
      throttledFunc();
    });

    cy.get('@fn').should('be.calledTwice');
  });
});
