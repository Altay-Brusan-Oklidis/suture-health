import { useMemo } from 'react';
import {
  Center,
  Flex,
  Skeleton,
  Text,
  HStack,
  StackDivider,
} from '@chakra-ui/react';
import {
  Button,
  EnvelopeRegularIconFA,
  GraduationCapIconFA,
} from 'suture-theme';
import { useRouter } from 'next/router';
import useMemberRole from '@hooks/useMemberRole';
import { UserType } from '@utils/zodModels';
import { useMainLayoutActions } from '@layouts/MainLayout/reducer';
import Link from '@components/Link';
import UploadAvatar from '@components/UploadAvatar';
import { NewUrls, OldUrls } from '@lib/constants';
import OrganizationsList from './components/OrganizationsList';

type MenuProps = {
  member: UserType;
  onClose: () => void;
};

const UserProfileMenu = ({ member, onClose: onMenuClose }: MenuProps) => {
  const { isSender } = useMemberRole();
  const selectedOrg = sessionStorage.getItem('selectedOrganizationId');
  const { setIsTourOpen } = useMainLayoutActions();
  const router = useRouter();
  const footerLinks = useMemo(
    () => [
      { label: 'Change Password', href: `/member/${member.memberId}/account` },
      { label: 'Support', href: 'mailto:support@suturehealth.com' },
      {
        label: 'Privacy Policy',
        href: 'https://drive.google.com/file/d/1LZr-dNzNyZNN5AvGQSnd-f7W8vw48k5b/view',
        isTargetBlank: true,
      },
      {
        label: 'Terms of Service',
        href: 'https://drive.google.com/file/d/1TPxg3oIDwpepJiWFJSLTaeEyJJjzhmw3/view',
        isTargetBlank: true,
      },
    ],
    [member.memberId]
  );

  const logout = () => {
    localStorage.removeItem('allPatientOptions');
    if (selectedOrg) sessionStorage.removeItem('selectedOrganizationId');
  };

  if (!member) return <Skeleton />;

  return (
    <Flex direction="column" data-cy="profile-menu">
      <Flex direction="column" alignContent="center">
        <Center flexDirection="column">
          <UploadAvatar
            name={`${member.firstName} ${member.lastName}`}
            badgeSize="24px"
            addTooltipLabel="Add Profile Photo"
            editTooltipLabel="Edit Profile Photo"
          />
          <Text mt="8px" color="gray.900" fontSize="31px">
            {`Hi, ${member.firstName}!`}
          </Text>
          <Text color="gray.500" fontSize="16px">
            {member.email}
          </Text>
          <Button
            as="a"
            href={`/member/${member.memberId}/account`}
            mt="8px"
            size="sm"
            fontWeight={400}
          >
            Manage Your Account
          </Button>
        </Center>
      </Flex>
      <OrganizationsList />
      <Flex alignItems="center" direction="column" mt="32px">
        <Text fontSize="20px">What&apos;s New</Text>
        {!isSender && router.pathname === NewUrls.INBOX && (
          <Button
            variant="link"
            colorScheme="gray"
            leftIcon={<EnvelopeRegularIconFA fontSize="24px" />}
            color="gray.500"
            fontWeight={400}
            mt="12px"
            onClick={() => {
              onMenuClose();
              setIsTourOpen(true);
            }}
          >
            Inbox Tour
          </Button>
        )}
        {Object.values<string>(OldUrls).includes(router.pathname) && (
          <Button
            variant="link"
            colorScheme="gray"
            leftIcon={<EnvelopeRegularIconFA fontSize="24px" />}
            color="gray.500"
            fontWeight={400}
            mt="12px"
            as="a"
            href="https://www.youtube.com/"
            target="_blank"
          >
            New Inbox for Signers
          </Button>
        )}
      </Flex>
      <Flex alignItems="center" direction="column" mt="12px">
        <Text fontSize="20px">Get Help</Text>
        <Button
          variant="link"
          colorScheme="gray"
          leftIcon={<GraduationCapIconFA fontSize="24px" />}
          color="gray.500"
          fontWeight={400}
          mt="12px"
          as="a"
          target="_blank"
          href={
            isSender
              ? 'https://sites.google.com/suturehealth.com/sutureu/home/sender-side'
              : 'https://sites.google.com/suturehealth.com/sutureu/home/signer-side'
          }
        >
          Suture U
        </Button>
      </Flex>
      <Button
        as="a"
        href="/identity/account/login"
        onClick={() => logout()}
        mt="32px"
        data-cy="logout-btn"
        alignSelf="center"
        variant="outline"
        color="blue.500"
      >
        Logout
      </Button>
      <HStack
        divider={
          <StackDivider borderColor="gray.500" h="10px" alignSelf="center" />
        }
        mt="32px"
      >
        {footerLinks.map((i, index) => (
          <Link
            href={i.href}
            key={index}
            color="gray.500"
            fontSize="10px"
            target={i.isTargetBlank ? '_blank' : undefined}
          >
            {i.label}
          </Link>
        ))}
      </HStack>
    </Flex>
  );
};

export default UserProfileMenu;
