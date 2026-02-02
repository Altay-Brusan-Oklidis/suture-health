import responseParser from './responseParser';

export default function post(
  resource: string,
  body?: Object,
  params?: RequestInit
) {
  return fetch(resource, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: body ? JSON.stringify(body) : undefined,
    ...params,
  }).then(responseParser);
}
