import Head from 'next/head';
import MainLayout from '@layouts/MainLayout';
import Inbox from '@containers/Inbox';

export default function InboxPage() {
  return (
    <>
      <Head>
        <title>SutureHealth - Inbox</title>
      </Head>
      <MainLayout>
        <Inbox />
      </MainLayout>
    </>
  );
}
