using System.Runtime.Serialization;

namespace SutureHealth.DataScraping
{
    public class ContactHistory : PatientRelationalBase
    {
        public string? ContactText { get; set; }
        public ContactType ContactType { get; set; }
        public int PreferenceOrder { get; set; }

        public ContactHistory()
        {

        }

        public ContactHistory(Guid patientId, string contactText, ContactType contactType, int preferenceOrder)
        {
            PatientId = patientId;
            ContactText = contactText;
            ContactType = contactType;
            PreferenceOrder = preferenceOrder;
        }
    }

    public enum ContactType
    {
        [EnumMember(Value = "HomePhone")] HomePhone =0,
        [EnumMember(Value = "MobilePhone")] MobilePhone =1,
        [EnumMember(Value = "WorkPhone")] WorkPhone =2,
        [EnumMember(Value = "Email")] Email =3
    }
}