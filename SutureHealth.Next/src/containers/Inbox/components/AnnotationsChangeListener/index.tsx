import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { useEffect } from 'react';

export default function AnnotationsChangeListener() {
  const { isAnnotationsChanged, syncAnnotations } = useUpdateAnnotations();

  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isAnnotationsChanged) {
        e.preventDefault();
        syncAnnotations();
        e.returnValue = '';

        return e.returnValue;
      }

      return '';
    };

    window.addEventListener('beforeunload', handleBeforeUnload);

    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [isAnnotationsChanged]);

  return null;
}
