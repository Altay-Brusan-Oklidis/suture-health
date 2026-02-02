using System;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class IntegrationLog
    {
        public long MessageId { get; set; }
        public DateTime? Date { get; set; }
        public long? ReferenceId { get; set; }
        public string Message { get; set; }

        public virtual Message MessageNavigation { get; set; }
    }
}
