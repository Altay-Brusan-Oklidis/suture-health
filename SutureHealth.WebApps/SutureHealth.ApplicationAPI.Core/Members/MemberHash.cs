using System;
using System.Collections.Generic;
using System.Text;

namespace SutureHealth.Application
{
    public class MemberHash
    {
        public int MemberHashId { get; set; }
        public int MemberId { get; set; }
	    public string HashProvider { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public virtual Member Member { get; set; }
    }
}
