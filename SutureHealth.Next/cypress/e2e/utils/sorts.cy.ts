import sortBy from '@utils/sortBy';

const dayjs = require('dayjs');

describe('sorts', () => {
  describe('sort by date', () => {
    it('should sort simple array by date (asc)', () => {
      const arr = [
        dayjs().format(),
        dayjs().subtract(1, 'd').format(),
        dayjs().subtract(2, 'd').format(),
      ];
      const expectedArr = [
        dayjs().subtract(2, 'd').format(),
        dayjs().subtract(1, 'd').format(),
        dayjs().format(),
      ];

      expect(sortBy(arr, [(i) => i], [1])).to.deep.eq(expectedArr);
    });

    it('should sort simple array by date (desc)', () => {
      const arr = [
        dayjs().subtract(2, 'd').format(),
        dayjs().format(),
        dayjs().subtract(1, 'd').format(),
      ];
      const expectedArr = [
        dayjs().format(),
        dayjs().subtract(1, 'd').format(),
        dayjs().subtract(2, 'd').format(),
      ];

      expect(sortBy(arr, [(i) => i], [-1])).to.deep.eq(expectedArr);
    });

    it('should sort array of objects by date (asc)', () => {
      const arr = [
        { date: dayjs().format() },
        { date: dayjs().subtract(1, 'd').format() },
        { date: dayjs().subtract(2, 'd').format() },
      ];
      const expectedArr = [
        { date: dayjs().subtract(2, 'd').format() },
        { date: dayjs().subtract(1, 'd').format() },
        { date: dayjs().format() },
      ];

      expect(sortBy(arr, ['date'], [1])).to.deep.eq(expectedArr);
    });

    it('should sort simple array of objects by date (desc)', () => {
      const arr = [
        { date: dayjs().subtract(2, 'd').format() },
        { date: dayjs().format() },
        { date: dayjs().subtract(1, 'd').format() },
      ];
      const expectedArr = [
        { date: dayjs().format() },
        { date: dayjs().subtract(1, 'd').format() },
        { date: dayjs().subtract(2, 'd').format() },
      ];

      expect(sortBy(arr, ['date'], [-1])).to.deep.eq(expectedArr);
    });
  });

  describe('sort by string', () => {
    const arr = ['Joe', 'Alex', 'Alexander', 'Barney', 'Joseph'];
    const users = [
      { firstName: 'Bob', lastName: 'Adler' },
      { firstName: 'Barney', lastName: 'Jones' },
      { firstName: 'Freddie', lastName: 'Crougar' },
      { firstName: 'Bob', lastName: 'Adams' },
      { firstName: 'Joe', lastName: 'Lewis' },
      { firstName: 'Joseph', lastName: 'Lewis' },
    ];

    it('should sort simple array by string (asc)', () => {
      const expectedArr = ['Alex', 'Alexander', 'Barney', 'Joe', 'Joseph'];

      expect(sortBy(arr, [(i) => i], [1])).to.deep.eq(expectedArr);
    });

    it('should sort simple array by string (desc)', () => {
      const expectedArr = ['Joseph', 'Joe', 'Barney', 'Alexander', 'Alex'];

      expect(sortBy(arr, [(i) => i], [-1])).to.deep.eq(expectedArr);
    });

    it('should sort array of objects by string (asc)', () => {
      const expectedArr = [
        { firstName: 'Bob', lastName: 'Adams' },
        { firstName: 'Bob', lastName: 'Adler' },
        { firstName: 'Freddie', lastName: 'Crougar' },
        { firstName: 'Barney', lastName: 'Jones' },
        { firstName: 'Joe', lastName: 'Lewis' },
        { firstName: 'Joseph', lastName: 'Lewis' },
      ];

      expect(
        sortBy(users, [(i) => `${i.lastName} ${i.firstName}`], [1])
      ).to.deep.eq(expectedArr);
    });

    it('should sort array of objects by string (desc)', () => {
      const expectedArr = [
        { firstName: 'Joseph', lastName: 'Lewis' },
        { firstName: 'Joe', lastName: 'Lewis' },
        { firstName: 'Barney', lastName: 'Jones' },
        { firstName: 'Freddie', lastName: 'Crougar' },
        { firstName: 'Bob', lastName: 'Adler' },
        { firstName: 'Bob', lastName: 'Adams' },
      ];

      expect(
        sortBy(users, [(i) => `${i.lastName} ${i.firstName}`], [-1])
      ).to.deep.eq(expectedArr);
    });
  });

  describe('sort by number', () => {
    const arr = [2, 3, -1, 1, 0];
    const objectsArr = arr.map((i) => ({ id: i }));

    it('should sort simple array by number (asc)', () => {
      const expectedArr = [-1, 0, 1, 2, 3];

      expect(sortBy(arr, [(i) => i], [1])).to.deep.eq(expectedArr);
    });

    it('should sort simple array by number (desc)', () => {
      const expectedArr = [3, 2, 1, 0, -1];

      expect(sortBy(arr, [(i) => i], [-1])).to.deep.eq(expectedArr);
    });

    it('should sort array of objects by number (asc)', () => {
      const expectedArr = [
        { id: -1 },
        { id: 0 },
        { id: 1 },
        { id: 2 },
        { id: 3 },
      ];

      expect(sortBy(objectsArr, ['id'], [1])).to.deep.eq(expectedArr);
    });

    it('should sort array of objects by number (desc)', () => {
      const expectedArr = [
        { id: 3 },
        { id: 2 },
        { id: 1 },
        { id: 0 },
        { id: -1 },
      ];

      expect(sortBy(objectsArr, ['id'], [-1])).to.deep.eq(expectedArr);
    });
  });

  describe('sort by bool', () => {
    const arr = [false, true, false];
    const objectsArr = arr.map((i) => ({ prop: i }));

    it('should sort simple array by bool (asc)', () => {
      const expectedArr = [true, false, false];

      expect(sortBy(arr, [(i) => i], [1])).to.deep.eq(expectedArr);
    });

    it('should sort simple array by number (desc)', () => {
      const expectedArr = [false, false, true];

      expect(sortBy(arr, [(i) => i], [-1])).to.deep.eq(expectedArr);
    });

    it('should sort array of objects by number (asc)', () => {
      const expectedArr = [{ prop: true }, { prop: false }, { prop: false }];

      expect(sortBy(objectsArr, ['prop'], [1])).to.deep.eq(expectedArr);
    });

    it('should sort array of objects by number (desc)', () => {
      const expectedArr = [{ prop: false }, { prop: false }, { prop: true }];

      expect(sortBy(objectsArr, ['prop'], [-1])).to.deep.eq(expectedArr);
    });
  });

  describe('sort by number and string', () => {
    const users = [
      { salary: 400, firstName: 'Barney' },
      { salary: 500, firstName: 'Joe' },
      { salary: 100, firstName: 'Alex' },
      { salary: 600, firstName: 'Joseph' },
      { salary: 300, firstName: 'Freddie' },
      { salary: 100, firstName: 'Alexander' },
    ];

    it('should sort by number (asc) and string (asc)', () => {
      const expectedArr = [
        { salary: 100, firstName: 'Alex' },
        { salary: 100, firstName: 'Alexander' },
        { salary: 300, firstName: 'Freddie' },
        { salary: 400, firstName: 'Barney' },
        { salary: 500, firstName: 'Joe' },
        { salary: 600, firstName: 'Joseph' },
      ];

      expect(sortBy(users, ['salary', 'firstName'], [1, 1])).to.deep.eq(
        expectedArr
      );
    });

    it('should sort by number (asc) and string (desc)', () => {
      const expectedArr = [
        { salary: 100, firstName: 'Alexander' },
        { salary: 100, firstName: 'Alex' },
        { salary: 300, firstName: 'Freddie' },
        { salary: 400, firstName: 'Barney' },
        { salary: 500, firstName: 'Joe' },
        { salary: 600, firstName: 'Joseph' },
      ];

      expect(sortBy(users, ['salary', 'firstName'], [1, -1])).to.deep.eq(
        expectedArr
      );
    });
  });

  describe('sort by number and date', () => {
    it('should sort by number (asc) and date (asc)', () => {
      const users = [
        {
          salary: 400,
          firstName: 'Barney',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 500,
          firstName: 'Joe',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
        {
          salary: 100,
          firstName: 'Alex',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 600,
          firstName: 'Joseph',
          updatedAt: dayjs().subtract(3, 'd').format(),
        },
        {
          salary: 300,
          firstName: 'Freddie',
          updatedAt: dayjs().subtract(4, 'd').format(),
        },
        {
          salary: 100,
          firstName: 'Alexander',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
      ];
      const expectedArr = [
        {
          salary: 100,
          firstName: 'Alexander',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
        {
          salary: 100,
          firstName: 'Alex',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 300,
          firstName: 'Freddie',
          updatedAt: dayjs().subtract(4, 'd').format(),
        },
        {
          salary: 400,
          firstName: 'Barney',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 500,
          firstName: 'Joe',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
        {
          salary: 600,
          firstName: 'Joseph',
          updatedAt: dayjs().subtract(3, 'd').format(),
        },
      ];

      expect(sortBy(users, ['salary', 'updatedAt'], [1, 1])).to.deep.eq(
        expectedArr
      );
    });

    it('should sort by number (asc) and date (desc)', () => {
      const users = [
        {
          salary: 400,
          firstName: 'Barney',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 500,
          firstName: 'Joe',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
        {
          salary: 100,
          firstName: 'Alex',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 600,
          firstName: 'Joseph',
          updatedAt: dayjs().subtract(3, 'd').format(),
        },
        {
          salary: 300,
          firstName: 'Freddie',
          updatedAt: dayjs().subtract(4, 'd').format(),
        },
        {
          salary: 100,
          firstName: 'Alexander',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
      ];
      const expectedArr = [
        {
          salary: 100,
          firstName: 'Alex',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 100,
          firstName: 'Alexander',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
        {
          salary: 300,
          firstName: 'Freddie',
          updatedAt: dayjs().subtract(4, 'd').format(),
        },
        {
          salary: 400,
          firstName: 'Barney',
          updatedAt: dayjs().subtract(1, 'd').format(),
        },
        {
          salary: 500,
          firstName: 'Joe',
          updatedAt: dayjs().subtract(2, 'd').format(),
        },
        {
          salary: 600,
          firstName: 'Joseph',
          updatedAt: dayjs().subtract(3, 'd').format(),
        },
      ];

      expect(sortBy(users, ['salary', 'updatedAt'], [1, -1])).to.deep.eq(
        expectedArr
      );
    });
  });
});
