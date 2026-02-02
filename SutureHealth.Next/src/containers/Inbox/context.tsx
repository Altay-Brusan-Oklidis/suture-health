import { useState, ReactNode } from 'react';
import { VirtuosoHandle } from 'react-virtuoso';
import { createContext, useContextSelector } from 'use-context-selector';

const useStore = () => {
  const [documentsRef, setDocumentsRef] = useState<
    VirtuosoHandle | undefined | null
  >(null);
  const [docViewRef, setDocViewRef] = useState<
    VirtuosoHandle | undefined | null
  >(null);

  return {
    documentsRef,
    setDocumentsRef,
    docViewRef,
    setDocViewRef,
  };
};

type StoreType = ReturnType<typeof useStore>;

const StoreContext = createContext<StoreType | null>(null);

export const StoreContextProvider = ({ children }: { children: ReactNode }) => (
  <StoreContext.Provider value={useStore()}>{children}</StoreContext.Provider>
);

export const useDocumentsRef = () =>
  useContextSelector(StoreContext, (store) => store?.documentsRef);
export const useSetDocumentsRef = () =>
  useContextSelector(StoreContext, (store) => store?.setDocumentsRef);

export const useDocViewRef = () =>
  useContextSelector(StoreContext, (store) => store?.docViewRef);
export const useSetDocViewRef = () =>
  useContextSelector(StoreContext, (store) => store?.setDocViewRef);
