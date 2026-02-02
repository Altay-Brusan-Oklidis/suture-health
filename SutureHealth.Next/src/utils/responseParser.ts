import camelcaseKeys from 'camelcase-keys';

const responseParser = (res: Response): Promise<any> => {
  const contentType = res.headers.get('content-type');

  if (contentType && contentType.indexOf('application/json') !== -1) {
    return res.json().then((data) => camelcaseKeys(data, { deep: true }));
  }

  return res.text().then((text) => JSON.stringify(text));
};

export default responseParser;
