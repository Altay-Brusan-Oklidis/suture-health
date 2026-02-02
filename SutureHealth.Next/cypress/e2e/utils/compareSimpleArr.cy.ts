import compareSimpleArr from '@utils/compareSimpleArr';

describe('compare numeric or string arrays', () => {
  it('numeric arrays are equal', () => {
    const arr1 = [1, 2, 3];
    const arr2 = [1, 2, 3];

    expect(compareSimpleArr(arr1, arr2)).to.equal(true);
  });

  it('numeric arrays are not equal', () => {
    const arr1 = [1, 2, 3];
    const arr2 = [3, 2, 1];

    expect(compareSimpleArr(arr1, arr2)).to.equal(false);
  });

  it('string arrays are equal', () => {
    const arr1 = ['1', '2', '3'];
    const arr2 = ['1', '2', '3'];

    expect(compareSimpleArr(arr1, arr2)).to.equal(true);
  });

  it('string arrays are not equal', () => {
    const arr1 = ['1', '2', '3'];
    const arr2 = ['3', '2', '1'];

    expect(compareSimpleArr(arr1, arr2)).to.equal(false);
  });
});
