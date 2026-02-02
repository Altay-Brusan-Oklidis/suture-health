using System;

namespace SutureHealth.Documents
{
    public class OCRDocument
    {
        public int OCRDocumentId { get; set; }
        public string OCRResult { get; set; }
        public string StorageContainer { get; set; }
        public string StorageKey { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateCompleted { get; set; }
    }
}
