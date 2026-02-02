using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SutureHealth.Application
{
    public class MemberIdentity : MemberBase
    {
        public MemberIdentity()
        {
        }

        public int Id { get => MemberId; }

        string _UserInitials = null;
        [NotMapped]
        public string UserInitials
        {
            get
            {
                if (_UserInitials == null)
                {
                    _UserInitials = String.Join("", FirstName?[..1], LastName?[..1]);
                }

                return _UserInitials;
            }
        }

        [NotMapped]
        public string NormalizedUserName { get => UserName.ToUpperInvariant(); }
        [NotMapped]
        public virtual string NormalizedEmail { get => Email.ToUpperInvariant(); }
        public virtual bool EmailConfirmed { get; set; }
        public virtual bool MobileNumberConfirmed { get; set; }
        [ProtectedPersonalData]
        public virtual string OfficeNumber { get; set; }
        [ProtectedPersonalData]
        public virtual string OfficeExtension { get; set; }
        public virtual bool TwoFactorEnabled { get; set; }

        public int? AccessFailedCount { get; set; }
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public DateTime? EffectiveAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public bool LockoutEnabled { get; set; } = false;
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public bool MustReadEula { get; set; } = true;
        public bool MustRegisterAccount { get; set; } = true;
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public DateTime? SuspendedAt { get; set; }

        public bool CanEditOrganizationProfile { get; set; } = false;
        public int? CurrentOrganizationId { get; set; }
        public int PrimaryOrganizationId { get; set; }
    }
}
