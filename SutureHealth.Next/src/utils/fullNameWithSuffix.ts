interface Args {
  firstName: string;
  lastName: string;
  professionalSuffix?: string;
  isLastNameFirst?: boolean;
  suffix?: string;
  withSuffix?: boolean;
}

export default function fullNameWithSuffix({
  firstName,
  lastName,
  professionalSuffix,
  isLastNameFirst,
  suffix,
  withSuffix,
}: Args) {
  const fullName = isLastNameFirst
    ? `${lastName}, ${firstName}`
    : `${firstName} ${lastName}`;
  const suffixPart = withSuffix && suffix ? ` ${suffix}` : '';
  const profSuffixPart = professionalSuffix
    ? isLastNameFirst
      ? `${suffixPart} ${professionalSuffix}`
      : `,${suffixPart} ${professionalSuffix}`
    : '';

  return `${fullName}${profSuffixPart}`;
}
