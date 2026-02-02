import { useState } from 'react';
import { Box, Input, InputGroup, InputRightElement } from '@chakra-ui/react';
import { CloseIcon } from 'suture-theme';
import { SearchIcon } from '@chakra-ui/icons';

interface Props {
  onChange: (value: string) => void;
}

export default function SearchInput({ onChange }: Props) {
  const [searchValue, setSearchValue] = useState<string>('');

  return (
    <InputGroup mt="20px">
      <Input
        _placeholder={{
          color: 'gray.400',
          fontSize: '14px',
        }}
        placeholder="Search Filters (ex. name, status, type, etc)"
        onChange={(e) => {
          setSearchValue(e.target.value);
          onChange(e.target.value);
        }}
        value={searchValue}
      />
      <InputRightElement>
        {searchValue ? (
          <Box
            display="flex"
            cursor="pointer"
            onClick={() => {
              setSearchValue('');
              onChange('');
            }}
          >
            <CloseIcon />
          </Box>
        ) : (
          <SearchIcon color="gray.500" />
        )}
      </InputRightElement>
    </InputGroup>
  );
}
