import { fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  BaseQueryFn,
  FetchArgs,
  FetchBaseQueryError,
} from '@reduxjs/toolkit/query';
import camelcaseKeys from 'camelcase-keys';

const baseQuery = fetchBaseQuery({
  baseUrl: '/api',
});

const baseQueryWithCamelize: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  const result = await baseQuery(args, api, extraOptions);

  if (result.data) {
    return {
      ...result,
      data: camelcaseKeys(result.data as any, { deep: true }),
    };
  }

  if (result.error) {
    if (result.error.status === 401) {
      window.location.href = `${result.meta?.response?.headers.get(
        'location'
      )}`;
    } else {
      return { ...result, error: camelcaseKeys(result.error, { deep: true }) };
    }
  }

  return result;
};

export default baseQueryWithCamelize;
