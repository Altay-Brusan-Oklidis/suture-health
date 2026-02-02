using System;
using System.Collections.Generic;
using System.Linq;

namespace SutureHealth.Documents
{
    public class Template
    {
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public int OrganizationId { get; set; }
        public int? TemplateTypeId { get; set; }
        public int? ParentTemplateId { get; set; }
        public bool IsActive { get; set; }
        public string StorageKey { get; set; }
        public int? StoredByMemberId { get; set; }
        public DateTime? DateStored { get; set; }

        public TemplateType TemplateType { get; set; }

        public ICollection<TemplateAnnotation> Annotations { get; set; }

        public override int GetHashCode()
        {
            var hashes = new int[]
            {
                TemplateId.GetHashCode(),
                TemplateTypeId.GetHashCode()
            };

            return Enumerable.Aggregate(hashes, 17, (aggr, hash) => aggr * 31 + hash);
        }
    }
}
