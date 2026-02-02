using System;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class AttachmentMeta
    {
        public long AttachmentId { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentType { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string DocumentDescription { get; set; }
        public string Confidentiality { get; set; }
        public bool? Convert { get; set; }
        public bool? UsePriorityQueue { get; set; }
        public string IntegrationMeta { get; set; }
        public string OrderIds { get; set; }
        public long? PatientId { get; set; }

        public virtual Attachment Attachment { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
