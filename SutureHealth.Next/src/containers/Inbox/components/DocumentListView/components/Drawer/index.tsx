import { ReactNode, useState, useRef, useEffect } from 'react';
import {
  VStack,
  Flex,
  useOutsideClick,
  IconButton,
  Box,
} from '@chakra-ui/react';
import { ChevronRightIconFA, Badge } from 'suture-theme';
import useDeviceHandler from '@hooks/useDeviceHandler';
import useViewedDocIndex from '@hooks/useViewedDocIndex';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';

interface Props {
  children: ReactNode;
}

export default function Drawer({ children }: Props) {
  const [translateY, setTranslateY] = useState(0);
  const drawerRef = useRef<HTMLDivElement>(null);
  const { isTablet } = useDeviceHandler();
  const documents = useSelector((state) => state.inbox.documents);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const viewedDocIds = useSelector((state) => state.inbox.viewedDocIds);
  const isTabletDrawerOpen = useSelector(
    (state) => state.inbox.isTabletDrawerOpen
  );
  const viewedDocIndex = useViewedDocIndex();
  const { setIsTabletDrawerOpen } = useInboxActions();

  useEffect(() => {
    setTranslateY(viewedDocIndex * 19); // rectangle height + margin (15px + 4px)
  }, [viewedDocIndex]);

  useOutsideClick({
    ref: drawerRef,
    handler: () => {
      if (isTablet) {
        setIsTabletDrawerOpen(false);
      }
    },
  });

  return (
    <>
      <Flex
        h="var(--doc-height)"
        w="var(--document-list-width)"
        display={{ sm: 'none', md: 'flex', xl: 'none' }}
        py="14px"
        alignItems="center"
        direction="column"
      >
        <IconButton
          aria-label="open list"
          icon={<ChevronRightIconFA />}
          onClick={() => setIsTabletDrawerOpen(true)}
          colorScheme="gray"
          size="xs"
          mb="8px"
        />
        <Badge isActive count={documents?.length} max={99} isEllipse ml={0} />
        <Box overflow="hidden" mt="8px">
          <Box
            transform={`translateY(-${translateY}px)`}
            transition="transform 300ms"
          >
            {documents?.map(({ sutureSignRequestId }, index) => (
              <Box
                mt={index === 0 ? 0 : '4px'}
                key={sutureSignRequestId}
                width="6px"
                height="15px"
                backgroundColor={
                  sutureSignRequestId === viewedDocument?.sutureSignRequestId
                    ? 'blue.500'
                    : viewedDocIds.includes(sutureSignRequestId)
                    ? 'gray.400'
                    : 'blue.100'
                }
                borderRadius="10px"
                transitionProperty="background-color"
                transitionDuration="normal"
              />
            ))}
          </Box>
        </Box>
      </Flex>
      <VStack
        bg="white"
        w={{ sm: '100vw', md: '380px', xl: 'var(--document-list-width)' }}
        height={{ sm: 'var(--doc-height)', xl: 'var(--inbox-content-height)' }}
        overflowY="hidden"
        spacing={0}
        position={{ sm: 'static', md: 'fixed', xl: 'static' }}
        left={isTabletDrawerOpen ? 0 : '-380px'}
        top={0}
        zIndex={{ sm: 'base', md: 'sticky', xl: 'base' }}
        transitionProperty="left"
        transitionDuration="normal"
        ref={isTablet ? drawerRef : undefined}
        boxShadow={{
          md: isTabletDrawerOpen
            ? '0px 4px 5px rgba(0, 0, 0, 0.14), 0px 1px 10px rgba(0, 0, 0, 0.12), 0px 2px 4px rgba(0, 0, 0, 0.2)'
            : 'none',
          xl: 'none',
        }}
      >
        {children}
      </VStack>
    </>
  );
}
