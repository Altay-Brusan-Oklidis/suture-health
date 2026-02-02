import Head from 'next/head';
import MainLayout from '@layouts/MainLayout';
import Iframe from 'components/Iframe';
import { IframeUrls } from '@lib/constants';

export default function ArchivePage() {
  return (
    <>
      <Head>
        <title>SutureHealth - Archive</title>
      </Head>
      <MainLayout>
        <Iframe pathname={IframeUrls.HISTORY} />
      </MainLayout>
    </>
  );
}
