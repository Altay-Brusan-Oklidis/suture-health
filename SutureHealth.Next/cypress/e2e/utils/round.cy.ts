import round from '@utils/round';

describe('rounding', () => {
  it('adds 0.1 + 0.2 with round function to equal 0.3', () => {
    expect(round(0.1 + 0.2)).to.eq(0.3);
  });

  it('adds 0.1 + 0.2 to unequal 0.3', () => {
    expect(0.1 + 0.2).not.to.eq(0.3);
  });
});
