export default function externalSubmit(formId: string) {
  document
    .getElementById(formId)
    ?.dispatchEvent(new Event('submit', { cancelable: true, bubbles: true }));
}
