namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Telecom
    {
        public long PatientId { get; set; }
        public string System { get; set; }
        public string Use { get; set; }
        public string Value { get; set; }

        public virtual Patient Patient { get; set; }
    }
}
