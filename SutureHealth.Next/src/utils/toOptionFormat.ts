export type Option = {
  label: string;
  value: string;
  documentCount?: number;
};

export default function toOptionFormat<T>(
  arr: T[],
  valueKey: keyof T | ((i: T) => string | number | undefined),
  labelKey: keyof T | ((i: T) => string | number | undefined)
): Option[] {
  return arr
    .filter((i) => {
      if (
        i &&
        ((typeof valueKey === 'function' && valueKey(i)) ||
          (valueKey && typeof valueKey !== 'function' && i[valueKey]))
      ) {
        return true;
      }

      return false;
    })
    .map((i) => {
      const option: Option = {
        value:
          typeof valueKey === 'function'
            ? valueKey(i)?.toString() || ''
            : `${i[valueKey]}`,
        label:
          typeof labelKey === 'function'
            ? labelKey(i)?.toString() || ''
            : `${i[labelKey]}`,
      };

      if (i && typeof i === 'object' && 'documentCount' in i) {
        option.documentCount = i.documentCount as number;
      }

      return option;
    });
}
