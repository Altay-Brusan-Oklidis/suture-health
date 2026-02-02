using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SutureHealth.Application
{
    public class MemberRelationship
    {
        [Required]
        [ForeignKey(nameof(Supervisor))]
        public int SupervisorMemberId { get; set; }
        [Required]
        [ForeignKey(nameof(Subordinate))]
        public int SubordinateMemberId { get; set; }
        public bool? IsActive { get; set; }

        public virtual Member Supervisor { get; set; }
        public virtual Member Subordinate { get; set; }
    }
}
