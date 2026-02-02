using System.Runtime.Serialization;


namespace SutureHealth.DataScraping
{
    public class Contact : PatientRelationalBase
    {
        public string? ContactText { get; set; }
        public ContactType ContactType { get; set; }
        public int PreferenceOrder { get; set; }

        public Contact()
        {

        }
    }
}
