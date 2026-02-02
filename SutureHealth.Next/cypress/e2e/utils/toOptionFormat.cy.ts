import toOptionFormat from '@utils/toOptionFormat';

describe('toOptionFormat', () => {
  const arr = [
    { id: 1, name: 'foo', lastName: 'fooLastName' },
    { id: 2, name: 'bar', lastName: 'barLastName' },
  ];

  it('should return option format', () => {
    const expectedArr = [
      { value: '1', label: 'foo' },
      { value: '2', label: 'bar' },
    ];

    expect(toOptionFormat(arr, 'id', 'name')).to.deep.eq(expectedArr);
  });

  it('should return option format with combined label', () => {
    const expectedArr = [
      { value: '1', label: 'foo fooLastName' },
      { value: '2', label: 'bar barLastName' },
    ];

    expect(
      toOptionFormat(arr, 'id', (i) => `${i.name} ${i.lastName}`)
    ).to.deep.eq(expectedArr);
  });
});
