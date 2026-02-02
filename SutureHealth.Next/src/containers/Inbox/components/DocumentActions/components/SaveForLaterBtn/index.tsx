import { useState, useMemo, useEffect, useRef } from 'react';
import {
  useToast,
  MenuButton,
  Menu,
  IconButton,
  ButtonGroup,
  Button,
  MenuList,
  MenuItem,
  Text,
  MenuDivider,
  Popover,
  PopoverTrigger,
  PopoverContent,
  PopoverBody,
  Box,
} from '@chakra-ui/react';
import {
  Toast,
  SortUpIconFA,
  HistoryIconFA,
  CalendarCheckIcon,
  DatePicker,
  ClockIconFA,
  RedoIconFA,
} from 'suture-theme';
import dayjs from 'dayjs';
import { useFormContext, useWatch } from 'react-hook-form';
import type { FilterData } from '@containers/Inbox';
import localizedFormat from 'dayjs/plugin/localizedFormat';
import { useSelector } from '@redux/hooks';
import useRefetchViewedDoc from 'hooks/useRefetchViewedDoc';
import {
  useSaveForLaterMutation,
  useMoveToInboxMutation,
  useGetMemberIdentityQuery,
  useGetMiscSettingsQuery,
  useUpdateMiscUserSettingsMutation,
} from '@containers/Inbox/apiReducer';
import useSetNextDoc from '@hooks/useSetNextDoc';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { FADE_OUT_ANIM_DURATION } from '@lib/constants';

dayjs.extend(localizedFormat);

interface Props {
  isDisabled: boolean;
}

interface SaveForLaterParams {
  isUndo?: boolean;
  expirationDate?: Date;
}

export default function SaveForLater({ isDisabled }: Props) {
  const [, rerender] = useState({});
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const isSavedUntilMenuOpen = useSelector(
    (state) => state.inbox.isSavedUntilMenuOpen
  );
  const [datePickerVisible, setDatePickerVisible] = useState<boolean>(false);
  const [selectedDate, setSelectedDate] = useState<Date>(
    dayjs().add(4, 'hours').startOf('minutes').toDate()
  );
  const refetchViewedDoc = useRefetchViewedDoc();
  const { setCurrentAction, setIsSavedUntilMenuOpen, setStackAnimation } =
    useInboxActions();
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { data: member } = useGetMemberIdentityQuery();
  const setNextDoc = useSetNextDoc();
  const toast = useToast({ render: Toast });
  const { control } = useFormContext<FilterData>();
  const documentProcessStatus = useWatch({
    control,
    name: 'documentProcessStatus',
  });
  const [saveForLaterMutation] = useSaveForLaterMutation();
  const [moveToInboxMutation] = useMoveToInboxMutation();
  const { data: settings } = useGetMiscSettingsQuery();
  const [updateMiscSettings] = useUpdateMiscUserSettingsMutation();
  const quickSelectionOptions = useMemo(
    () =>
      Array.from(Array(3).keys()).map((i) => {
        const date = dayjs(selectedDate)
          .set('hour', 10)
          .add(3 * i, 'hours')
          .startOf('hour');

        return { value: date.toDate(), label: date.format('ddd, LT') };
      }),
    [selectedDate]
  );
  const saveUntilOptions = [
    {
      label: '1 Hour',
      value: dayjs().add(1, 'hour').startOf('minutes').toDate(),
    },
    {
      label: 'End of Day',
      value: dayjs().set('hours', 16).startOf('hours').toDate(),
    },
    {
      label: 'Tomorrow',
      value: dayjs().add(1, 'day').set('hours', 8).startOf('hour').toDate(),
    },
    {
      label: dayjs().add(2, 'days').format('dddd'),
      value: dayjs().add(2, 'days').set('hours', 8).startOf('hour').toDate(),
    },
    {
      label: dayjs().add(3, 'days').format('dddd'),
      value: dayjs().add(3, 'days').set('hours', 8).startOf('hour').toDate(),
    },
  ].filter((i) => i.value.getTime() > new Date().getTime());
  const timerRef = useRef<NodeJS.Timeout | null>(null);
  const { syncAnnotations } = useUpdateAnnotations();

  const rerenderEveryMinute = () => {
    rerender({});
    timerRef.current = setTimeout(rerenderEveryMinute, 60_000);
  };

  useEffect(() => {
    timerRef.current = setTimeout(() => {
      rerenderEveryMinute();
    }, dayjs().endOf('minutes').valueOf() - dayjs().valueOf());

    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
        timerRef.current = null;
      }
    };
  }, []);

  async function moveToInbox(isUndo?: boolean) {
    setCurrentAction('saveForLater');
    await moveToInboxMutation({
      id: viewedDocument!.sutureSignRequestId,
      documentProcessStatus,
    }).unwrap();
    setCurrentAction(null);

    if (isUndo) {
      refetchViewedDoc(viewedDocument!.sutureSignRequestId);
    } else {
      refetchViewedDoc();
      setNextDoc();
    }

    toast.closeAll();
    toast({
      title: 'Document Returned to Inbox',
      description: dayjs(selectedDate).format('ddd, LT'),
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
              saveForLater({ isUndo: true });
            },
          }}
          buttonText="Undo"
        />
      ),
    });
  }

  async function saveForLater(params?: SaveForLaterParams) {
    const { isUndo, expirationDate } = params || {};
    const date = (expirationDate || selectedDate).toUTCString();

    setCurrentAction('saveForLater');

    updateMiscSettings({
      lastSavedForLaterDate: date,
    });

    await syncAnnotations(true);
    await saveForLaterMutation({
      expirationDate: date,
      memberId: member!.memberId,
      id: viewedDocument!.sutureSignRequestId!,
      documentProcessStatus,
    }).unwrap();
    setCurrentAction(null);

    if (isUndo) {
      refetchViewedDoc(viewedDocument!.sutureSignRequestId);
    } else {
      refetchViewedDoc();
      setStackAnimation('fadeOut');
      setTimeout(() => {
        setStackAnimation(undefined);
        setNextDoc();
      }, FADE_OUT_ANIM_DURATION);
    }

    toast.closeAll();

    // setTimeout fixes for the immediate toast hide issue
    setTimeout(() => {
      toast({
        title: 'Saved For Later',
        description: dayjs(date).format('ddd, LT'),
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
    });
  }

  const closeDatePicker = () => {
    setDatePickerVisible(false);
    setIsSavedUntilMenuOpen(false);
    setSelectedDate(dayjs().add(4, 'hours').startOf('minutes').toDate());
  };

  return (
    <ButtonGroup
      isAttached
      variant="outline"
      colorScheme="cyan"
      w={{ sm: '100%', md: 'auto' }}
      className="save-for-later-btn"
    >
      <Button
        w={{ sm: '100%', md: 'auto' }}
        leftIcon={<HistoryIconFA />}
        onClick={() =>
          saveForLater({
            expirationDate: dayjs().add(1, 'hour').startOf('minutes').toDate(),
          })
        }
        isDisabled={isDisabled}
        isLoading={currentAction === 'saveForLater'}
        data-cy="save-for-later-btn"
      >
        Save For Later
      </Button>
      <Menu
        placement="top-end"
        onClose={() => {
          setIsSavedUntilMenuOpen(false);
          closeDatePicker();
        }}
        isLazy
        isOpen={isSavedUntilMenuOpen}
        onOpen={() => setIsSavedUntilMenuOpen(true)}
      >
        <MenuButton
          as={IconButton}
          aria-label="Options"
          icon={<SortUpIconFA />}
          isDisabled={isDisabled}
        />
        <MenuList minW="230px" zIndex="dropdown">
          <Text
            color="gray.600"
            fontSize="14px"
            fontWeight={600}
            p="4px 12px 12px"
          >
            Save Until...
          </Text>
          {settings?.lastSavedForLaterDate &&
            new Date(settings?.lastSavedForLaterDate).getTime() >
              new Date().getTime() && (
              <MenuItem
                fontSize="12px"
                onClick={() =>
                  saveForLater({
                    expirationDate: new Date(settings.lastSavedForLaterDate),
                  })
                }
              >
                <RedoIconFA mr="8px" color="gray.600" fontSize="16px" />
                <Box display="flex" justifyContent="space-between" width="100%">
                  <Text color="black">Last</Text>
                  <Text color="blackAlpha.700">
                    {dayjs(settings.lastSavedForLaterDate).format(
                      'ddd, MMM D, LT'
                    )}
                  </Text>
                </Box>
              </MenuItem>
            )}
          {saveUntilOptions.map((i) => (
            <MenuItem
              key={i.value.getTime()}
              fontSize="12px"
              justifyContent="space-between"
              onClick={() => saveForLater({ expirationDate: i.value })}
            >
              <Text color="black">{i.label}</Text>
              <Text color="blackAlpha.700">
                {dayjs(i.value).format('ddd, LT')}
              </Text>
            </MenuItem>
          ))}
          <MenuDivider />
          <Popover
            isOpen={datePickerVisible}
            onClose={closeDatePicker}
            closeOnBlur={false}
            isLazy
          >
            <PopoverTrigger>
              <MenuItem
                fontSize="14px"
                justifyContent="center"
                closeOnSelect={false}
                onClick={() => setDatePickerVisible(true)}
              >
                <CalendarCheckIcon color="gray.600" mr="8px" fontSize="16px" />
                <Text color="black">Pick date & time</Text>
              </MenuItem>
            </PopoverTrigger>
            <PopoverContent width="auto">
              <PopoverBody p={0}>
                <DatePicker
                  inline
                  withTimePicker
                  withFooterButtons
                  minDate={new Date()}
                  onCancelClick={() => {
                    closeDatePicker();
                  }}
                  onSaveClick={() => saveForLater()}
                  selected={selectedDate}
                  onChange={(date) => setSelectedDate(date as Date)}
                  quickSelectionOptions={quickSelectionOptions}
                />
              </PopoverBody>
            </PopoverContent>
          </Popover>
        </MenuList>
      </Menu>
    </ButtonGroup>
  );
}
