import { NextRequest, NextResponse } from 'next/server';
import { verifyAuthentication } from '@lib/auth';
import parseCookies from '@utils/parseCookies';
import pathConfig from '@lib/userRoleConfig';
import { IframeUrlValues, UrlValues } from '@lib/constants';

export default async function middleware(req: NextRequest) {
  if (process.env.APP_ENV === 'test') {
    return NextResponse.next();
  }

  const url = req.nextUrl.clone();
  const userRole = parseCookies(req.cookies.toString())[
    'SutureHealth.UserRoleCookie'
  ];
  const config = pathConfig(userRole);
  const shouldRedirect = (pathname: string) => {
    const equals = (value: UrlValues | IframeUrlValues) => value === pathname;

    return (
      config && ![...config.navigation.keys(), ...config.iframe].some(equals)
    );
  };

  if (shouldRedirect(url.pathname)) {
    url.pathname = config!.default;

    return NextResponse.redirect(url);
  }

  if (verifyAuthentication(req)) {
    const response = NextResponse.next();
    const cookieValue = req.cookies.get(
      `SutureHealth.AuthenticationCookie.${process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT}.SessionManager.Expiration`
    )?.value;

    response.cookies.set(
      `${process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT}.expiry`,
      cookieValue || ''
    );

    return response;
  }

  const returnUrl = new URL(url.href);

  if (returnUrl.hostname.includes('ec2.internal')) {
    returnUrl.hostname = String(process.env.NEXT_PUBLIC_WEBHOST);
    returnUrl.port = '3000';
  }

  if (process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT === 'Prod') {
    return NextResponse.redirect(
      `https://app.suturesign.com/identity/account/login?returnUrl=${encodeURIComponent(
        returnUrl.toString()
      )}`
    );
  }

  return NextResponse.redirect(
    `https://app.${
      process.env.NEXT_PUBLIC_WEBHOST
    }/identity/account/login?returnUrl=${encodeURIComponent(
      returnUrl.toString()
    )}`
  );
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|icons).*)'],
};
