using System;

namespace SutureHealth.Documents
{
    public class SignedPdfAttributes
    {
        public string Signature { get; set; }
        public string SignatureId { get; set; }
        public DateTimeOffset DateSigned { get; set; }
        public string RequestId { get; set; }
        public string Pid { get; set; }
    }
}
