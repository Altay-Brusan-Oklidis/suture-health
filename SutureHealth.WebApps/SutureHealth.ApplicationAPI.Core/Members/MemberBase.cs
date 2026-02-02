using System;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace SutureHealth.Application
{
    public abstract class MemberBase
    {
        public MemberBase()
        {
        }

        public int MemberId { get; set; }
        public string NPI { get; set; }
        public int MemberTypeId { get; set; }
        public string SigningName { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string MaidenName { get; set; }
        public string Suffix { get; set; }
        public string ProfessionalSuffix { get; set; }

        [ProtectedPersonalData]
        public virtual string Email { get; set; }
        [ProtectedPersonalData]
        public virtual string MobileNumber { get; set; }

        public bool CanSign { get; set; } = false;
        public bool IsActive { get; set; }
        public bool IsCollaborator { get; set; }
        public bool IsPayingClient { get; set; }

        public DateTimeOffset? LastLoggedInAt { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public override string ToString() => $"Member[{MemberId}]: {string.Join(",", LastName, FirstName)}";

        [NotMapped]
        public MemberType MemberType => Enum.GetValues(typeof(MemberType)).Cast<MemberType>().FirstOrDefault(mt => (int)mt == MemberTypeId);
        public string LongName { get => new StringBuilder().Append(ShortName).AppendFormat(" (NPI: {0})", !string.IsNullOrWhiteSpace(NPI) ? NPI : "unavailable").ToString(); }
        public string ShortName { get => Person.GetFullName(LastName, FirstName, Suffix, ProfessionalSuffix); }

        public bool IsApplicationAdministrator() => MemberTypeId == 2016;
        public bool IsInvitationsEligible() => !IsPayingClient && IsUserSigningMember();
        [Obsolete("Please user IsUserPhysician() instead.")]
        public bool IsUserSigner() => IsUserPhysician();
        public bool IsUserPhysician() => MemberTypeId == 2000;
        public bool IsUserSigningMember() => (new int[] { 2000, 2001, 2002, 2008, 2012, 2014, 2015 }).Contains(MemberTypeId);
        public bool IsUserSender() => MemberTypeId == 2003;
        public bool IsUserCollaborator() => (new int[] { 2001, 2008 }).Contains(MemberTypeId);
    }
}