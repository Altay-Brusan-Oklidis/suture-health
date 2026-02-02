import isErrorWithMessage from '@utils/isErrorWithMessage';

export default function isProblemsWithPassword(error: unknown) {
  if (isErrorWithMessage(error)) {
    const errorMessage = error.data.signerPassword?.join('');

    if (
      errorMessage?.includes('must reset their password') ||
      errorMessage?.includes('must register their account')
    ) {
      return true;
    }
  }

  return false;
}
