import { useMemo, useState, useRef } from 'react';
import {
  Flex,
  Text,
  Popover,
  PopoverTrigger,
  PopoverContent,
  Portal,
} from '@chakra-ui/react';
import {
  Badge,
  Button,
  TruncatedText,
  ChevronDownIconFA,
  ChevronUpIconFA,
} from 'suture-theme';
import { Virtuoso } from 'react-virtuoso';
import { useGetOrganizationsQuery } from '@containers/Inbox/apiReducer';
import UploadAvatar from '@components/UploadAvatar';

const ORG_HEIGHT = '42px';
const ORG_MT = '16px';

export default function OrganizationsList() {
  const [isMoreClicked, setIsMoreClicked] = useState<boolean>(false);
  const { data: orgs } = useGetOrganizationsQuery();
  const containerRef = useRef<HTMLDivElement>(null);

  const showMore = useMemo(
    () => orgs?.organizations.length && orgs?.organizations.length > 3,
    [orgs?.organizations.length]
  );

  if (!orgs?.organizations.length) {
    return null;
  }

  return (
    <Flex
      flexDirection="column"
      bg="white"
      pl="24px"
      pr="0px"
      py="12px"
      borderRadius="16px"
      mt="24px"
      ref={containerRef}
    >
      <Flex alignItems="center">
        <Text
          textTransform="uppercase"
          color="gray.500"
          fontWeight={700}
          fontSize="12px"
        >
          organizations
        </Text>
        <Badge
          count={orgs.organizations.length}
          ml="4px"
          isEllipse
          inactiveColors={{ bg: 'gray.500', color: 'white' }}
          height="13px"
          px="4px"
          fontSize="7px"
        />
      </Flex>
      <Virtuoso
        style={{
          height: '100%',
          flex: 1,
          minHeight: `calc((${ORG_HEIGHT} + ${ORG_MT}) * ${
            isMoreClicked ? 4 : showMore ? 3 : orgs.organizations.length
          })`,
          scrollbarGutter: 'stable',
        }}
        data={orgs.organizations.slice(0, isMoreClicked ? undefined : 3)}
        itemContent={(_, i) => (
          <Flex h={ORG_HEIGHT} mt={ORG_MT} pr="8px">
            <UploadAvatar
              name={i.name}
              badgeSize="12px"
              badgePosition={{ bottom: '-4px', right: '-4px' }}
              size="sm"
              borderRadius="8px"
              topSegmentStyle={{
                fontSize: '6px',
                height: '9px',
              }}
              addTooltipLabel="Add Logo"
              editTooltipLabel="Edit Logo"
            />
            <Flex ml="6px" flexDirection="column" w="100%">
              <TruncatedText
                text={i.name}
                fontSize="16px"
                color="gray.900"
                maxW="246px"
                openDelay={1000}
              />
              <Flex>
                <Text color="gray.500" fontSize="12px">
                  Primary
                </Text>
                <Popover trigger="hover" isLazy lazyBehavior="unmount">
                  {({ onClose }) => (
                    <>
                      <PopoverTrigger>
                        <Button
                          ml="auto"
                          color="blue.500"
                          variant="link"
                          fontSize="12px"
                          fontWeight={400}
                        >
                          Subscriptions
                        </Button>
                      </PopoverTrigger>
                      <Portal containerRef={containerRef}>
                        <PopoverContent
                          width="220px"
                          p="16px"
                          bg="gray.50"
                          borderColor="gray.500"
                          borderRadius="8px"
                        >
                          <Flex fontSize="16px" color="gray.900">
                            <Text fontWeight={700}>Account Type</Text>
                            <Text fontWeight={500} ml="auto">
                              Enterpise
                            </Text>
                          </Flex>
                          <Text
                            color="blue.500"
                            fontSize="11px"
                            fontWeight={700}
                            mt="10px"
                            lineHeight="20px"
                          >
                            ADD ONS
                          </Text>
                          <Flex color="gray.900" mt="2px">
                            <Text fontSize="14px">Inbox Marketing</Text>
                            <Text fontWeight={700} ml="auto" fontSize="12px">
                              OFF
                            </Text>
                          </Flex>
                          <Button
                            variant="outline"
                            alignSelf="center"
                            size="xs"
                            mt="14px"
                            color="blue.500"
                            onClick={onClose}
                          >
                            Manage
                          </Button>
                        </PopoverContent>
                      </Portal>
                    </>
                  )}
                </Popover>
              </Flex>
            </Flex>
          </Flex>
        )}
      />
      {showMore && (
        <Button
          variant="ghost"
          colorScheme="teal"
          color="teal.500"
          ml="auto"
          mr="24px"
          rightIcon={
            isMoreClicked ? <ChevronUpIconFA /> : <ChevronDownIconFA />
          }
          mt="14px"
          size="xs"
          height="16px"
          fontSize="14px"
          fontWeight="normal"
          onClick={() => setIsMoreClicked((prev) => !prev)}
        >
          {isMoreClicked ? 'LESS' : 'MORE'}
        </Button>
      )}
    </Flex>
  );
}
