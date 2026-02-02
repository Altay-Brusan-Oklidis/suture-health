import Head from 'next/head';
import MainLayout from '@layouts/MainLayout';
import Iframe from 'components/Iframe';
import { IframeUrls } from '@lib/constants';

export default function VideoPage() {
  return (
    <>
      <Head>
        <title>SutureHealth - Video</title>
      </Head>
      <MainLayout>
        <Iframe pathname={IframeUrls.VIDEO} />
      </MainLayout>
    </>
  );
}
