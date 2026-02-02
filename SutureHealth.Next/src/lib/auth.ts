import { NextRequest } from 'next/server';

export function verifyAuthentication(request: NextRequest): boolean {
  const authCookie =
    request.cookies.get(
      `SutureHealth.AuthenticationCookie.${process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT}`
    ) ||
    request.cookies.get(
      `SutureHealth.Saml2.${process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT}`
    ) ||
    process.env.NEXT_PUBLIC_SKIP_AUTH;

  return authCookie !== undefined;
}
