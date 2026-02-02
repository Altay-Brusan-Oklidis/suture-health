type Array = (string | number)[];

export default function compareSimpleArr(arr1: Array, arr2: Array) {
  return (
    arr1.length === arr2.length && arr1.every((i, index) => i === arr2[index])
  );
}
