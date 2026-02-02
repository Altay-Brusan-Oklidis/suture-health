import { useEffect, useMemo } from 'react';
import { useToast, UseToastOptions } from '@chakra-ui/react';
import { Toast } from 'suture-theme';
import { useSelector } from '@redux/hooks';

interface Props {
  isLoaded: boolean;
}

export default function ResentToast({ isLoaded }: Props) {
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const toast = useToast({ render: (props) => <Toast inline {...props} /> });
  const documentScroll = useSelector((state) => state.inbox.documentScroll);
  const isFilterMenuOpen = useSelector((state) => state.inbox.isFilterMenuOpen);
  const isFilterSidebarOpen = useSelector(
    (state) => state.inbox.isFilterSidebarOpen
  );
  const toastOptions: UseToastOptions = useMemo(
    () => ({
      id: `resent-${viewedDocument?.sutureSignRequestId}`,
      title: 'This document has been resent',
      isClosable: true,
      status: 'info',
      duration: 5000,
      position: 'top',
      containerStyle: {
        marginTop: `calc(var(--navbar-height) + var(--document-information-header-height) + ${
          documentScroll ? 'var(--document-revenue-header-height)' : '0px'
        })`,
        marginLeft: `calc(${
          isFilterMenuOpen ? 'var(--filter-menu-width)' : '0px'
        } + ${
          isFilterSidebarOpen
            ? 'var(--filter-sidebar-open-width)'
            : 'var(--filter-sidebar-close-width)'
        } + var(--document-list-width))`,
      },
    }),
    [
      viewedDocument?.sutureSignRequestId,
      documentScroll,
      isFilterMenuOpen,
      isFilterSidebarOpen,
    ]
  );

  useEffect(() => {
    if (toast.isActive(`resent-${viewedDocument?.sutureSignRequestId}`)) {
      toast.update(
        `resent-${viewedDocument?.sutureSignRequestId}`,
        toastOptions
      );
    }
  }, [toastOptions]);

  useEffect(() => {
    if (isLoaded && viewedDocument?.isResent) {
      // setTimeout helps to avoid toast jumping while scrolling between documents
      setTimeout(() => toast(toastOptions), 250);
    }

    return () => {
      if (toast.isActive(`resent-${viewedDocument?.sutureSignRequestId}`)) {
        toast.close(`resent-${viewedDocument?.sutureSignRequestId}`);
      }
    };
  }, [viewedDocument?.sutureSignRequestId, isLoaded]);

  return null;
}
