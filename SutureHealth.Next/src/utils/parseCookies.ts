export default function parseCookies(cookies: string) {
  return Object.fromEntries(
    cookies.split('; ').map((i) => i.split(/=(.*)/s).map(decodeURIComponent))
  );
}
