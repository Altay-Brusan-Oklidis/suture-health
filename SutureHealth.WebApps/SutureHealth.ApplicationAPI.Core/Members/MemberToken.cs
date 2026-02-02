using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SutureHealth.Application
{
    public class MemberToken
    {
        public int MemberTokenId { get; set; }
        public int MemberId { get; set; }
        [MaxLength(128)]
        public string TokenProvider { get; set; }
        [MaxLength(128)]
        public string Name { get; set; }
        public string Value { get; set; }

        public virtual Member Member { get; set; }
    }
}
