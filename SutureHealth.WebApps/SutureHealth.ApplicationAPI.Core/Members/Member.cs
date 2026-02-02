using System.Collections.Generic;

namespace SutureHealth.Application
{
    public class Member : MemberBase
    {
        public Member()
        {
            Contacts = new List<MemberContact>();
        }

        public virtual ICollection<MemberContact> Contacts { get; set; }
    }

    public class MemberContact : ContactInfo<int, Member, int>
    {
        public int? CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsPrimary { get; set; }
    }
}