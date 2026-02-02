export default function isErrorWithMessage(
  error: unknown
): error is { data: { [key: string]: string[] } } {
  if (error && typeof error === 'object' && 'data' in error) {
    return true;
  }

  return false;
}
