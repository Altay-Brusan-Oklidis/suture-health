using System.Collections.Generic;

namespace SutureHealth.Application
{
    public static class MemberTypeExtensions
    {
        public static IEnumerable<MemberType> SignerTypes =>
            new MemberType[]
            {
                MemberType.Physician,
                MemberType.NursePractitioner,
                MemberType.Nurse,
                MemberType.PhysicianAssistant,
                MemberType.ClericalSupportStaff
            };

        public static IEnumerable<MemberType> SenderTypes =>
            new MemberType[]
            {
                MemberType.Staff
            };
    }
}
