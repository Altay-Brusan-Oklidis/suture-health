export type SortOrder = 1 | -1;

type SortValue = string | number | boolean | undefined | null;

const sortFn = (first: SortValue, second: SortValue) => {
  if (typeof first === 'string' && typeof second === 'string') {
    const firstDate = Date.parse(first);
    const secondDate = Date.parse(second);

    if (Number.isNaN(firstDate) && Number.isNaN(secondDate)) {
      return first.localeCompare(second);
    }

    return firstDate - secondDate;
  }

  if (!first && !second) return 0;
  if (!first && first !== 0) return 1;
  if (!second && second !== 0) return -1;

  return (first as number) - (second as number);
};

export default function sortBy<T>(
  arr: T[],
  keys: (keyof T | ((i: T) => SortValue))[],
  orders: SortOrder[]
) {
  return [...arr].sort((a, b) =>
    keys
      .map((i) => {
        if (typeof i === 'function') {
          const first = i(a);
          const second = i(b);

          return sortFn(first, second);
        }

        const first = a[i] as SortValue;
        const second = b[i] as SortValue;

        return sortFn(first, second);
      })
      .map((i, index) => i * (orders?.[index] || 1))
      .reduce((acc, cur) => acc || cur, 0)
  );
}
