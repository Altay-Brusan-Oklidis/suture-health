import Head from 'next/head';
import MainLayout from '@layouts/MainLayout';
import Iframe from 'components/Iframe';
import { IframeUrls } from '@lib/constants';

export default function SendPage() {
  return (
    <>
      <Head>
        <title>SutureHealth - Send</title>
      </Head>
      <MainLayout>
        <Iframe pathname={IframeUrls.SEND} />
      </MainLayout>
    </>
  );
}
