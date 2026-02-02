import {
  Popover,
  PopoverContent,
  PopoverBody,
  Avatar,
  PopoverCloseButton,
  PopoverTrigger,
  Button,
} from '@chakra-ui/react';
import { useGetMemberIdentityQuery } from '@containers/Inbox/apiReducer';
import UserProfileMenu from './components/UserProfileMenu';

export default function UserProfileBadge() {
  const { data: member } = useGetMemberIdentityQuery();

  if (!member) return null;

  return (
    <Popover placement="bottom-end">
      {({ onClose }) => (
        <>
          <PopoverTrigger>
            <Button variant="unstyled" size="sm">
              <Avatar
                name={`${member.firstName} ${member.lastName}`}
                size="sm"
                bg="gray.500"
                color="white"
                cursor="pointer"
                data-cy="user-badge"
              />
            </Button>
          </PopoverTrigger>
          <PopoverContent
            bg="gray.200"
            borderRadius="16px"
            px="24px"
            py="32px"
            width="sm"
          >
            <PopoverCloseButton
              color="gray.500"
              top="32px"
              right="24px"
              width="28px"
              height="28px"
              fontSize="16px"
            />
            <PopoverBody p={0}>
              <UserProfileMenu member={member} onClose={onClose} />
            </PopoverBody>
          </PopoverContent>
        </>
      )}
    </Popover>
  );
}
