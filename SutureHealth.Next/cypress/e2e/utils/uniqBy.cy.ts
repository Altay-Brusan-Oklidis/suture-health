import uniqBy from '@utils/uniqBy';

it('should return a unique array', () => {
  const arr = [
    { id: 1, name: '1' },
    { id: 2, name: '2' },
    { id: 3, name: '3' },
    { id: 4, name: '4' },
    { id: 1, name: '5' },
    { id: 2, name: '6' },
  ];
  const expectedArr = [
    { id: 1, name: '5' },
    { id: 2, name: '6' },
    { id: 3, name: '3' },
    { id: 4, name: '4' },
  ];

  expect(uniqBy(arr, 'id')).to.deep.eq(expectedArr);
});
