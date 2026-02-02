import {
  Text,
  Flex,
  Menu,
  MenuButton,
  MenuList,
  MenuItem,
} from '@chakra-ui/react';
import dayjs from 'dayjs';
import {
  TruncatedText,
  PhoneIconFA,
  Button,
  SortDownIconFA,
} from 'suture-theme';
import { RequestDocumentType } from '@utils/zodModels';

interface Props {
  document: RequestDocumentType;
}

export default function EffectiveSection({
  document: {
    document: { effectiveDate, submitterOrganization },
  },
}: Props) {
  return (
    <Flex ml="24px" direction="column" flex={1}>
      <Flex direction="row" mb="6px" flex={1}>
        <Flex alignItems="center">
          <Text color="gray.600" fontSize="14px">
            Effective
          </Text>
          <TruncatedText
            fontWeight={700}
            fontSize="16px"
            lineHeight="120%"
            color="gray.600"
            ml={1}
            text={dayjs(effectiveDate).format('M/DD/YYYY')}
          />
        </Flex>
      </Flex>
      <Flex direction="row" align="center" height="30px">
        <Text color="gray.600" fontSize="14px" mr={1}>
          From
        </Text>
        <TruncatedText
          fontWeight={700}
          fontSize="16px"
          lineHeight="120%"
          color="gray.600"
          mr={1}
          text={submitterOrganization.name}
        />
        {submitterOrganization.phoneNumber && (
          <Menu placement="bottom">
            <MenuButton
              as={Button}
              aria-label="call"
              rightIcon={<SortDownIconFA ml="-6px" />}
              variant="outline"
              size="xs"
              color="blue.500"
              p="6px"
              borderRadius="full"
              ml={1}
            >
              <PhoneIconFA />
            </MenuButton>
            <MenuList minWidth="140px">
              <MenuItem
                color="gray.500"
                fontSize="16px"
                fontWeight={600}
                as="span"
                bg="transparent"
              >
                {submitterOrganization.phoneNumber}
              </MenuItem>
              <MenuItem
                onClick={() =>
                  navigator.clipboard.writeText(
                    submitterOrganization.phoneNumber!
                  )
                }
              >
                Copy number
              </MenuItem>
              <MenuItem
                as="a"
                href={`tel:${submitterOrganization.phoneNumber}`}
              >
                Call
              </MenuItem>
            </MenuList>
          </Menu>
        )}
      </Flex>
    </Flex>
  );
}
