using System;

namespace SutureHealth.Documents
{
    public class RejectedPdfAttributes
    {
        public DateTimeOffset DateProcessed { get; set; }
        public string ProcessedBy { get; set; }
        public string ProcessingOffice { get; set; }
        public string ProcessingOfficePhone { get; set; }
        public string RejectionReason { get; set; }
        public string RequestId { get; set; }
    }
}
