import Cookies from 'js-cookie';

type CookieOptions = {
  expires: Date;
  path?: string;
  domain?: string;
  secure?: boolean;
};

export const autoLogOut = () => {
  const cookieValue = Cookies.get(
    `${process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT}.expiry`
  );

  // if (!cookieValue) {
  //   window.location.reload();
  // }

  if (cookieValue) {
    const expirationTime = decodeURIComponent(cookieValue);
    const currentTime = new Date().toISOString();

    if (expirationTime < currentTime) {
      Cookies.remove(
        `${process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT}.expiry`
      );
      window.location.reload();
    }
  }
};

export const setCookieExpiration = () => {
  const expirationTime = new Date();

  expirationTime.setHours(expirationTime.getHours() + 1);

  const cookieOptions: CookieOptions = {
    expires: expirationTime,
    path: '/', // Adjust the path if necessary
  };

  Cookies.set(
    `SutureHealth.AuthenticationCookie.${process.env.NEXT_PUBLIC_ASPNETCORE_ENVIRONMENT}`,
    encodeURIComponent(expirationTime.toISOString()),
    cookieOptions
  );
};
