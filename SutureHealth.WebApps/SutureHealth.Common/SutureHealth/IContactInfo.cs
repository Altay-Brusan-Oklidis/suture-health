using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SutureHealth
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [DataContract]
    public enum ContactType
    {
        [EnumMember(Value = "phone")] Phone = 0,
        [EnumMember(Value = "fax")] Fax = 1,
        [EnumMember(Value = "mobile")] Mobile = 2,
        [EnumMember(Value = "email")] Email = 3,
        [EnumMember(Value = "url")] Url = 4,
        [EnumMember(Value = "officephone")] OfficePhone = 5,
        [EnumMember(Value = "officephoneext")] OfficePhoneExt = 6,
        [EnumMember(Value = "homephone")] HomePhone = 7,
        [EnumMember(Value = "workphone")] WorkPhone = 8,
        [EnumMember(Value = "otherphone")] OtherPhone = 9,
    }

    public interface IContactInfo
    {
        ContactType Type { get; set; }
        string Value { get; set; }
    }

    public static class ContactInfoExtensions
    {
        public static T Clone<T>(this IContactInfo contact) where T : IContactInfo, new()
        {
            var value = contact.Value;
            switch (contact.Type)
            {
                case ContactType.Phone:
                case ContactType.Fax:
                case ContactType.Mobile:
                case ContactType.OfficePhone:
                    value = value.ToFormattedPhoneNumber();
                    break;
                case ContactType.Email:
                case ContactType.Url:
                case ContactType.OfficePhoneExt:
                default:
                    break;
            }

            var clone = new T
            {
                Type = contact.Type,
                Value = value
            };
            return clone;
        }
    }
}
