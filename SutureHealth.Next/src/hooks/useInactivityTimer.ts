import { useEffect, useRef } from 'react';

function useInactivityTimer(action: () => void, delay: number) {
  const timer = useRef<NodeJS.Timeout>();

  const handleActivity = () => {
    if (timer.current) {
      clearTimeout(timer.current);
    }
    timer.current = setTimeout(() => {
      action();
    }, delay);
  };

  useEffect(() => {
    handleActivity(); // start the timer for the first time

    // set the event listeners
    window.addEventListener('mousemove', handleActivity);
    window.addEventListener('keypress', handleActivity);

    return () => {
      if (timer.current) {
        clearTimeout(timer.current);
      }
      // clean up the event listeners
      window.removeEventListener('mousemove', handleActivity);
      window.removeEventListener('keypress', handleActivity);
    };
  }, []);
}

export default useInactivityTimer;
