import getConfig from 'next/config';
import { Box } from 'suture-theme';
import dynamic from 'next/dynamic';
import { useEffect } from 'react';
import { autoLogOut } from '@utils/authCookieUtils';
import useInactivityTimer from '@hooks/useInactivityTimer';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { useRouter } from 'next/router';
import SessionHandler from './components/SessionHandler';
import Navbar from './components/Navbar';

const Tour = dynamic(() => import('@components/Tour'), { ssr: false });

const { publicRuntimeConfig } = getConfig();

interface Props {
  children: React.ReactNode;
}

export default function MainLayout({ children }: Props) {
  const router = useRouter();
  const { syncAnnotations } = useUpdateAnnotations();

  if (
    process.env.NODE_ENV !== 'test' &&
    process.env.NODE_ENV !== 'development'
  ) {
    console.log(`Current app version is ${publicRuntimeConfig.version}.`);
    console.log(
      `Current theme version is ${publicRuntimeConfig.themeVersion}.`
    );
  }

  // auto logout after 20 minutes of inactivity
  useInactivityTimer(() => {
    localStorage.removeItem('allPatientOptions');
    syncAnnotations().then(() => router.push('/identity/account/login'));
  }, 20 * 60 * 1000);

  // auto logout if expired auth cookie
  useEffect(() => {
    const timer = setInterval(() => {
      autoLogOut();
    }, 20000);

    return () => {
      clearInterval(timer);
    };
  }, []);

  return (
    <>
      <Tour />
      <Box>
        <Navbar />
        <SessionHandler />
        {children}
      </Box>
    </>
  );
}
