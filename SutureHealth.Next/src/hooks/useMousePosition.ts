import { RefObject, useEffect, useState } from 'react';

export default function useMousePosition(ref: RefObject<HTMLElement>) {
  const [mouse, setMouse] = useState<MouseEvent>();

  useEffect(() => {
    if (ref.current) {
      ref.current.addEventListener('mousedown', setMouse);
    }

    return () => {
      if (ref.current) {
        ref.current.removeEventListener('mousedown', setMouse);
      }
    };
  }, [ref.current]);

  return mouse;
}
