import { Text, Flex, Icon, Tooltip } from '@chakra-ui/react';
import dayjs from 'dayjs';
import { RolodexCardIcon, TruncatedText } from 'suture-theme';
import { PatientType } from '@utils/zodModels';

interface Props {
  patient: PatientType;
}

export default function PatientRow({ patient }: Props) {
  return (
    <Flex direction="row" alignItems="center" mb="6px">
      <TruncatedText
        fontSize="1em"
        fontWeight={700}
        lineHeight="120%"
        color="gray.600"
        text={`${patient.firstName}, ${patient.lastName}`}
      />
      <Text ml={1} fontSize="14px" fontWeight={400} color="gray.600">
        {dayjs(patient.birthdate).format('(MM/DD/YYYY)')}
      </Text>
      <Flex direction="row" alignContent="center" ml={2} mr={1}>
        <Icon w="16px" color="gray.600" as={RolodexCardIcon} />
      </Flex>
      <Flex whiteSpace="break-spaces">
        {patient.payerMix.map((mix, index) => (
          <Tooltip
            key={index}
            label={`${mix.name} - ${mix.value}`}
            isDisabled={!mix.value}
          >
            <Text color="gray.600" fontSize="14px">
              {`${mix.name}${
                patient.payerMix.length > 0 &&
                index !== patient.payerMix.length - 1
                  ? ', '
                  : ''
              }`}
            </Text>
          </Tooltip>
        ))}
      </Flex>
    </Flex>
  );
}
