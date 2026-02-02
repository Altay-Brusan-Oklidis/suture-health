using System.Collections.Generic;
using System.Linq;

namespace SutureHealth.Application
{
    public static class MemberExtensions
    {
        public static string RoleDescription(this MemberBase member)
        {
            if (member.MemberTypeId == 2000 || member.CanSign)
            {
                return "Signer";
            }
            if (member.IsCollaborator)
            {
                return "Collaborator";
            }
            if (member.MemberTypeId == 2003)
            {
                return "Sender";
            }

            return "Assistant";
        }


        public static TContactInfo GetPrimaryContact<TContactInfo>(this IEnumerable<TContactInfo> contacts, ContactType contactType) where TContactInfo : MemberContact, IContactInfo
            => (contacts ?? new TContactInfo[] { }).Where(c => c.IsActive && c.IsPrimary && c.Type == contactType && !string.IsNullOrWhiteSpace(c.Value))
                                                   .FirstOrDefault();
        public static TContactInfo GetPrimaryEmailAddress<TContactInfo>(this IEnumerable<TContactInfo> contacts) where TContactInfo : MemberContact, IContactInfo
            => GetPrimaryContact(contacts, ContactType.Email);
        public static TContactInfo GetPrimaryMobileNumber<TContactInfo>(this IEnumerable<TContactInfo> contacts) where TContactInfo : MemberContact, IContactInfo
            => GetPrimaryContact(contacts, ContactType.Mobile);
    }
}
