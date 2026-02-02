import Lottie from 'lottie-react';
import { Box } from '@chakra-ui/react';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';
import cross from '@public/animations/cross.json';
import checkmark from '@public/animations/checkmark.json';
import { useEffect } from 'react';

export default function LottieAnimations() {
  const animation = useSelector((state) => state.inbox.animation);
  const { setAnimation } = useInboxActions();

  useEffect(() => {
    if (animation) {
      setTimeout(() => {
        setAnimation(undefined);
      }, 1500);
    }
  }, [animation, setAnimation]);

  const renderLottie = () => {
    if (animation) {
      return (
        <Lottie
          animationData={animation?.type === 'success' ? checkmark : cross}
          style={{ width: '40%' }}
          loop={false}
          autoplay
          data-cy={`${animation?.type}-animation`}
        />
      );
    }

    return null;
  };

  return (
    <Box
      position="fixed"
      width="calc(100vw - var(--document-list-width))"
      height="100vh"
      zIndex={1000}
      right={0}
      display="flex"
      alignItems="center"
      justifyContent="center"
      pointerEvents="none"
    >
      {renderLottie()}
    </Box>
  );
}
