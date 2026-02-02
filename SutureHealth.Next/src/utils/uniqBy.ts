export default function uniqBy<T>(
  arr: T[],
  key: keyof T | ((i: T) => string | number | undefined)
): T[] {
  const mapResult = new Map(
    arr.map((item) => [
      typeof key === 'function' ? key(item) : `${item[key]}`,
      item,
    ])
  );

  // @ts-ignore
  mapResult.delete(null);
  mapResult.delete(undefined);

  return [...mapResult.values()];
}
