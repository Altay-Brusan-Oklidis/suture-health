import { useState } from 'react';
import { AddIcon } from '@chakra-ui/icons';
import { Button, Flex, HStack } from '@chakra-ui/react';
import useDeviceHandler from '@hooks/useDeviceHandler';
import SwitchToOldInboxBtn from '@components/SwitchToOldInboxBtn';
import UserProfileBadge from './components/UserProfileBadge';
import OrgDropdown from './components/OrgDropdown';
import Logo from './components/Logo';
import Navigation from './components/Navigation';
import AddCPOModal from './components/AddCPOModal';
import styles from './styles';

export default function Navbar() {
  const [isOpenModal, setIsOpenModal] = useState<boolean>(false);
  const { isDesktop } = useDeviceHandler();

  return (
    <Flex
      h="var(--navbar-height)"
      bg="#F8F9FA"
      boxShadow="inset 0px -1px 0px #DEE2E6"
      display={{ sm: 'none', xl: 'flex' }}
    >
      {styles}
      <HStack alignItems="stretch">
        <Logo />
        <Navigation />
      </HStack>
      <Flex mr={5} ml="auto" alignItems="center">
        {isDesktop ? <SwitchToOldInboxBtn /> : null}
        <Button
          variant="outline"
          size="xs"
          leftIcon={<AddIcon />}
          mr={2}
          onClick={() => setIsOpenModal(true)}
          data-cy="add-cpo"
        >
          Add CPO
        </Button>
        <OrgDropdown />
        <UserProfileBadge />
        <AddCPOModal
          isOpen={isOpenModal}
          onClose={() => setIsOpenModal(false)}
        />
      </Flex>
    </Flex>
  );
}
