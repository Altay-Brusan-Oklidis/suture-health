import Head from 'next/head';
import MainLayout from '@layouts/MainLayout';
import Iframe from 'components/Iframe';
import { IframeUrls } from '@lib/constants';

export default function RevenuePage() {
  return (
    <>
      <Head>
        <title>SutureHealth - Revenue</title>
      </Head>
      <MainLayout>
        <Iframe pathname={IframeUrls.REVENUE} />
      </MainLayout>
    </>
  );
}
