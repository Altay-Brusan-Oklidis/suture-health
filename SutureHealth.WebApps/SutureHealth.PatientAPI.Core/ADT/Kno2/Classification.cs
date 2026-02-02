using System.Collections.Generic;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Classification
    {
        public Classification()
        {
            Messages = new HashSet<Message>();
        }

        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Scheme { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}
