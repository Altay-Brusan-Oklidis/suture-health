using System;
using System.ComponentModel.DataAnnotations;

namespace SutureHealth.Application
{
    public class MemberAuditEvent
    {
        public long MemberAuditEventId { get; set; }
        public int MemberId { get; set; }
        public DateTime AuditDate { get; set; }
        public AuditEvents AuditEventId { get; set; }
        [MaxLength(255)]
        public string AuditEventName { get; set; }
        [MaxLength(50)]
        public string IpAddress { get; set; }
        public bool Succeeded { get; set; }

        public virtual MemberIdentity Member { get; set; }
    }

    public enum AuditEvents : int
    {
        Unknown = 0,
        SignIn,
        SignOut,
        Registration,
        ForgotPassword,
        ChangePassword,
        VerifyPassword,
        LockedOut,
        Unlocked,
        EmailConfirmed,
        PhoneConfirmed,
        PublicIdentityUsed,
        VerifyPublicIdentity,
        CreatePublicIdentity
    }
}
