import { useEffect, useState } from 'react';
import { Box, useConst } from '@chakra-ui/react';
import TourContent from '@components/Tour/components/TourContent';
import {
  useGetMiscSettingsQuery,
  useGetTourSettingsQuery,
  useDoneTourSettingsMutation,
} from '@containers/Inbox/apiReducer';
import Joyride, {
  CallBackProps,
  Step,
  STATUS,
  ACTIONS,
  EVENTS,
} from 'react-joyride';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { useMainLayoutActions } from '@layouts/MainLayout/reducer';
import { useSelector } from '@redux/hooks';
import { Logo } from '@icons';
import scroll from '@public/gifs/scroll.gif';
import bulkAction from '@public/gifs/bulkAction.gif';
import mouseClick from '@public/gifs/mouseClick.gif';
import filters from '@public/gifs/filters.gif';
import reassign from '@public/gifs/reassign.gif';
import Tooltip from './components/Tooltip';

export default function Tour() {
  const [step, setStep] = useState<number>(0);
  const { toggleIsFilterSidebarOpen } = useInboxActions();
  const { setIsTourOpen } = useMainLayoutActions();
  const isFilterSidebarOpen = useSelector(
    (state) => state.inbox.isFilterSidebarOpen
  );
  const isTourOpen = useSelector((state) => state.mainLayout.isTourOpen);
  const scrollHintVisible = useSelector(
    (state) => state.inbox.scrollHintVisible
  );
  const withoutArrow = useConst<Partial<Step>>({
    placement: 'left-start',
    offset: 0,
    spotlightPadding: 0,
    styles: {
      options: { arrowColor: 'transparent' },
    },
  });
  const [steps, setSteps] = useState<Step[]>([]);
  const { data: miscSettings } = useGetMiscSettingsQuery();
  const { data: tourSettings } = useGetTourSettingsQuery();
  const [doneTourSettings] = useDoneTourSettingsMutation();

  useEffect(() => {
    if (
      miscSettings &&
      tourSettings?.isFirstInboxTourDone === false &&
      steps.length &&
      !isTourOpen
    ) {
      setIsTourOpen(true);
      doneTourSettings();
    }
  }, [
    doneTourSettings,
    miscSettings,
    setIsTourOpen,
    tourSettings?.isFirstInboxTourDone,
    steps,
  ]);

  useEffect(() => {
    if (miscSettings) {
      setSteps([
        {
          target: '.tour-step',
          disableBeacon: true,
          ...withoutArrow,
          content: (
            <TourContent
              title="Welcome to Your New, Blazing Fast Inbox!"
              description={`Simply scroll through all documents while reviewing each one for at least ${
                miscSettings.documentViewDuration / 1000
              } seconds.`}
              icon={<Logo />}
              // eslint-disable-next-line @next/next/no-img-element
              extraContent={<img src={scroll.src} alt="scroll" />}
            />
          ),
        },
        {
          target: '.tour-step',
          disableBeacon: true,
          ...withoutArrow,
          content: (
            <TourContent
              title="Process Multiple Documents with One Click"
              description={`After reviewing each one for ${
                miscSettings.documentViewDuration / 1000
              } seconds, process all the ones you have viewed!`}
              // eslint-disable-next-line @next/next/no-img-element
              extraContent={<img src={bulkAction.src} alt="bulk" />}
            />
          ),
        },
        {
          target: '.quick-navigation',
          disableBeacon: true,
          placement: 'top',
          content: (
            <TourContent
              title="Quick Navigation"
              description="Use the Previous and Next buttons to go to the top of the next document."
              extraContent={
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={mouseClick.src}
                  alt="mouse-click"
                  style={{ minHeight: '299px' }}
                />
              }
            />
          ),
        },
        {
          target: '.save-for-later-btn',
          disableBeacon: true,
          content: (
            <TourContent
              title="Need to skip something temporarily?"
              description="Use the Save for Later feature to temporarily snooze a document in your inbox."
            />
          ),
        },
        {
          target: '.documents-filters',
          placement: 'right',
          disableBeacon: true,
          content: (
            <TourContent
              title="Search & Filter"
              description="You can even search or fine tune your filters with more advanced filters."
              // eslint-disable-next-line @next/next/no-img-element
              extraContent={<img src={filters.src} alt="filters" />}
            />
          ),
        },
        {
          target: '.filter-sidebar',
          placement: 'right-start',
          disableBeacon: true,
          content: (
            <TourContent
              title="Organize By Task"
              description="Now you can see All documents in one view, or as before see only those that Need Signature or Filling Out."
            />
          ),
        },
        {
          target: '.reassign-icon',
          disableBeacon: true,
          content: (
            <TourContent
              title="Reassigning & Forwarding"
              description="Reassign users, add collaborators, or forward documents here."
              // eslint-disable-next-line @next/next/no-img-element
              extraContent={<img src={reassign.src} alt="reassign" />}
            />
          ),
        },
        {
          target: scrollHintVisible ? '.scroll-hint' : '.scroll-hint-plug',
          disableBeacon: true,
          content: (
            <TourContent
              title="You are ready to start using Inbox!"
              description="Go ahead and start scrolling to get started."
              icon={<Logo />}
            />
          ),
          ...(scrollHintVisible ? {} : { spotlightPadding: 0 }),
        },
      ]);
    }
  }, [miscSettings, scrollHintVisible]);

  const handleJoyrideCallback = (data: CallBackProps) => {
    const { status, action, index, type } = data;
    const finishedStatuses: string[] = [STATUS.FINISHED, STATUS.SKIPPED];

    if (action === ACTIONS.CLOSE || finishedStatuses.includes(status)) {
      /* istanbul ignore next */
      if (isFilterSidebarOpen) {
        toggleIsFilterSidebarOpen();
      }

      setIsTourOpen(false);
      setStep(0);
    } else if (
      ([EVENTS.STEP_AFTER, EVENTS.TARGET_NOT_FOUND] as string[]).includes(type)
    ) {
      const nextIndex = index + (action === ACTIONS.PREV ? -1 : 1);

      if (action === ACTIONS.NEXT) {
        if (
          (index === 4 && !isFilterSidebarOpen) ||
          (index === 5 && isFilterSidebarOpen)
        ) {
          toggleIsFilterSidebarOpen();
          setTimeout(() => setStep(nextIndex), 200);
        } else {
          setStep(nextIndex);
        }
      } else if (action === ACTIONS.PREV) {
        if (
          (index === 6 && !isFilterSidebarOpen) ||
          (index === 5 && isFilterSidebarOpen)
        ) {
          toggleIsFilterSidebarOpen();
          setTimeout(() => setStep(nextIndex), 200);
        } else {
          /* istanbul ignore next */
          setStep(nextIndex);
        }
      }
    }
  };

  return (
    <>
      <Joyride
        tooltipComponent={Tooltip}
        stepIndex={step}
        continuous
        run={isTourOpen}
        steps={steps}
        callback={handleJoyrideCallback}
        disableScrolling
        disableOverlayClose
        styles={{
          beacon: {
            width: 0,
            height: 0,
          },
        }}
      />
      <Box className="tour-step" position="fixed" top="64px" right="34px" />
    </>
  );
}
