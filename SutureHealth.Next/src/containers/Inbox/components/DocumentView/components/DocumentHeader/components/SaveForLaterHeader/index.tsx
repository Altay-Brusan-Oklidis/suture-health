import { Flex, Text, useToast, IconButton } from '@chakra-ui/react';
import { useFormContext, useWatch } from 'react-hook-form';
import {
  Button,
  Toast,
  HistoryIconFA,
  ClockIconFA,
  EditSolidIconFA,
} from 'suture-theme';
import {
  useGetMemberIdentityQuery,
  useSaveForLaterMutation,
  useMoveToInboxMutation,
} from '@containers/Inbox/apiReducer';
import { useInboxActions } from '@containers/Inbox/localReducer';
import useSetNextDoc from '@hooks/useSetNextDoc';
import dayjs from 'dayjs';
import useRefetchViewedDoc from '@hooks/useRefetchViewedDoc';
import type { FilterData } from '@containers/Inbox';

type HeaderProps = {
  isSaved: boolean;
  date?: string | null; // can be the date time saved until or last saved date
  documentId: number;
};

const SaveForLaterHeader = ({ isSaved, date, documentId }: HeaderProps) => {
  const [saveForLaterMutation, { isLoading: isSaveForLaterLoading }] =
    useSaveForLaterMutation();
  const [moveToInboxMutation, { isLoading: isMoveToInboxLoading }] =
    useMoveToInboxMutation();
  const toast = useToast();
  const setNextDoc = useSetNextDoc();
  const formattedDate = dayjs(date).format('ddd, LT');
  const { data: member } = useGetMemberIdentityQuery();
  const refetchViewedDoc = useRefetchViewedDoc();
  const { control } = useFormContext<FilterData>();
  const documentProcessStatus = useWatch({
    control,
    name: 'documentProcessStatus',
  });
  const { setIsSavedUntilMenuOpen } = useInboxActions();

  async function moveToInbox(isUndo?: boolean) {
    await moveToInboxMutation({
      id: documentId,
      documentProcessStatus,
    }).unwrap();

    if (isUndo) {
      refetchViewedDoc(documentId);
    } else {
      refetchViewedDoc();
      setNextDoc();
    }

    toast.closeAll();
    toast({
      title: 'Document Returned to Inbox',
      description: formattedDate,
      containerStyle: {
        minWidth: '330px',
        marginBottom: '140px',
        marginLeft: '20px',
      },
      position: 'bottom-left',
      duration: 2000,
      isClosable: true,
      render: (props) => (
        <Toast
          {...props}
          buttonProps={{
            onClick: () => {
              toast.closeAll();
              // eslint-disable-next-line @typescript-eslint/no-use-before-define
              saveForLater(true);
            },
          }}
          buttonText="Undo"
        />
      ),
    });
  }

  async function saveForLater(isUndo?: boolean) {
    const defaultDate = dayjs().add(4, 'hours').toDate();

    await saveForLaterMutation({
      expirationDate: defaultDate.toUTCString(),
      memberId: member!.memberId,
      id: documentId,
      documentProcessStatus,
    }).unwrap();

    if (isUndo) {
      refetchViewedDoc(documentId);
    } else {
      refetchViewedDoc();
      setNextDoc();
    }

    toast.closeAll();
    toast({
      title: 'Saved For Later',
      description: dayjs(defaultDate).format('ddd, LT'),
      containerStyle: {
        minWidth: '330px',
        marginBottom: '140px',
        marginLeft: '20px',
      },
      position: 'bottom-left',
      duration: 2000,
      isClosable: true,
      render: (props) => (
        <Toast
          {...props}
          buttonProps={{
            onClick: () => {
              toast.closeAll();
              moveToInbox(true);
            },
          }}
          buttonText="Undo"
          icon={<ClockIconFA />}
        />
      ),
    });
  }

  if (date === null) return <></>;

  return (
    <Flex
      minW="500px"
      h="56px"
      bgColor={isSaved ? 'gray.700' : 'cyan.500'}
      px="12px"
      borderRadius="0px 0px 6px 6px"
      position="absolute"
      top="var(--document-information-header-height)"
      left="50%"
      transform="translateX(-50%)"
    >
      <Flex
        fontWeight={400}
        direction="row"
        p={2}
        align="center"
        justify="space-between"
        fontSize="16px"
        lineHeight="24px"
      >
        <HistoryIconFA color="whiteAlpha.900" fontSize="24px" />
        <Flex pl="13.4px" pr="40px" color="whiteAlpha.900" alignItems="center">
          <Text>{isSaved ? 'Saved until' : 'Returned on'}</Text>
          <Text ml={1}>{formattedDate}</Text>
          {isSaved && (
            <IconButton
              ml="4px"
              icon={<EditSolidIconFA color="white" />}
              aria-label="edit"
              variant="ghost"
              colorScheme="white"
              size="xs"
              onClick={() => setIsSavedUntilMenuOpen(true)}
            />
          )}
        </Flex>
        <Button
          leftIcon={<HistoryIconFA />}
          onClick={() =>
            isSaved ? moveToInbox() : setIsSavedUntilMenuOpen(true)
          }
          ml="25px"
          h="32px"
          fontSize="14px"
          lineHeight="20px"
          colorScheme="whiteAlpha"
          color="whiteAlpha.900"
          variant="outline"
          isLoading={isMoveToInboxLoading || isSaveForLaterLoading}
        >
          {isSaved ? 'Move to Inbox' : 'Save for Later'}
        </Button>
      </Flex>
    </Flex>
  );
};

export default SaveForLaterHeader;
