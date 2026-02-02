import { Fragment, useEffect, useMemo, useState } from 'react';
import { Tabs, TabList, Tab, Badge } from 'suture-theme';
import { useRouter } from 'next/router';
import NextLink from 'next/link';
import { ApplicationUrls, UrlValues, UserRoleValues } from '@lib/constants';
import { skipToken } from '@reduxjs/toolkit/dist/query';
import { useGetDocumentsCountQuery } from '@containers/Inbox/apiReducer';
import useOrgIds from '@hooks/useOrgIds';
import SaveModal from '@containers/Inbox/components/SaveModal';
import useAnnotationValidation from '@hooks/useAnnotationValidation';
import { Box, Skeleton } from '@chakra-ui/react';
import pathConfig from '@lib/userRoleConfig';
import parseCookies from '@utils/parseCookies';

export default function Navigation() {
  const [modalOpen, setModalOpen] = useState(false);
  const [url, setUrl] = useState<UrlValues | undefined>();
  const [userRole, setUserRole] = useState<UserRoleValues | undefined>();
  const router = useRouter();
  const orgIds = useOrgIds();
  const { data: documentsCount } = useGetDocumentsCountQuery(
    orgIds || skipToken
  );
  const { isMovedTo } = useAnnotationValidation();

  const links = useMemo(() => {
    const config = pathConfig(userRole);

    if (!config) {
      return [];
    }

    return Array.from(config.navigation).map(([key, value]) => {
      const count =
        key === ApplicationUrls.INBOX ? documentsCount?.all || 0 : undefined;

      return { href: key, title: value, count };
    });
  }, [documentsCount, userRole]);

  useEffect(() => {
    if (document.cookie) {
      const userRoleCookie = parseCookies(document.cookie)[
        'SutureHealth.UserRoleCookie'
      ];

      setUserRole(userRoleCookie);
    }
  }, []);

  const currentIndex = useMemo(() => {
    if (typeof window !== 'undefined') {
      return links.findIndex(
        (i) =>
          new URL(i.href, window.location.origin).pathname === router.pathname
      );
    }

    return 0;
  }, [links, router]);

  const handleClick = (link: UrlValues) => {
    if (isMovedTo !== null) {
      setModalOpen(!modalOpen);
      setUrl(link);
    } else {
      router.push(link || ApplicationUrls.INBOX);
    }
  };

  return (
    <>
      <SaveModal
        onClose={() => {
          setModalOpen(false);
          setUrl(undefined);
        }}
        location={url!}
        isOpen={modalOpen}
        isLeaveAction
      />
      <Tabs variant="top-line" fontFamily="Open Sans" index={currentIndex}>
        <TabList mt="3px" h="100%">
          {userRole
            ? links.map(({ count, href, title }, index) => (
                <Fragment key={index}>
                  <Tab
                    fontSize="sm"
                    onClick={(e) => {
                      e.preventDefault();
                      handleClick(href);
                    }}
                    pt="0"
                    _hover={{ bg: '#E9ECEF' }}
                    height="100%"
                    fontWeight="semibold"
                    as={NextLink}
                    href={href}
                  >
                    {title}
                    {count && count > 0 ? (
                      <Badge
                        count={count}
                        max={999}
                        isEllipse
                        activeColors={{ color: 'white', bg: 'red.500' }}
                        isActive={currentIndex === index}
                      />
                    ) : null}
                  </Tab>
                </Fragment>
              ))
            : Array(4)
                .fill(null)
                .map((_, index) => (
                  <Fragment key={index}>
                    <Tab>
                      <Skeleton
                        p="0"
                        height="60%"
                        width="100%"
                        isLoaded={false}
                      >
                        <Box width="80px" />
                      </Skeleton>
                    </Tab>
                  </Fragment>
                ))}
        </TabList>
      </Tabs>
    </>
  );
}
