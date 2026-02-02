using System;
using System.Collections.Generic;
using System.Text;

namespace SutureHealth
{
    // https://www.cs.columbia.edu/sip/draft/rfc2806bis/draft-ietf-iptel-rfc2806bis-03.html#:~:text=The%20%22tel%22%20URI%20describes%20resources%20identified%20by%20telephone,to%20route%20the%20call%20to%20this%20termination%20point.
    // https://github.com/google/libphonenumber/blob/master/java/libphonenumber/src/com/google/i18n/phonenumbers/Phonenumber.java
    public class TelephoneNumber
    {
        public int? CountryCode { get; private set; }
        public string RegionalNumber { get; private set; }
        public string Extension { get; private set; }

        public TelephoneNumber()
        { }

        public TelephoneNumber(string regionalNumber)
            : this(regionalNumber, 1)
        { }

        public TelephoneNumber(string regionalNumber, int countryCode)
        {
            CountryCode = countryCode;
            RegionalNumber = regionalNumber;
        }

        public bool HasCountryCode => CountryCode.HasValue;
        public bool HasExtension => !string.IsNullOrWhiteSpace(Extension);

        public override string ToString() => $"tel:+{CountryCode}.{RegionalNumber}";
    }
}
