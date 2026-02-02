using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.Application;

namespace SutureHealth.AspNetCore.Mvc.Rendering
{
    public static class SelectListItems
    {
        public static IEnumerable<SelectListItem> AllMemberTypes =>
            Enum.GetValues<MemberType>().Where(mt => mt != MemberType.Unknown).Select(mt =>
            {
                var value = (int)mt;

                switch (mt)
                {
                    case MemberType.Physician:
                        return new SelectListItem("Physician (Signer)", value.ToString());
                    case MemberType.Nurse:
                    case MemberType.ClericalSupportStaff:
                        return new SelectListItem($"{mt.GetEnumDescription()} (Assistant)", value.ToString());
                    case MemberType.NursePractitioner:
                    case MemberType.PhysicianAssistant:
                        return new SelectListItem($"{mt.GetEnumDescription()} (Collaborator)", value.ToString());
                    case MemberType.Staff:
                        return new SelectListItem("Staff (Sender)", value.ToString());
                    default:
                        return new SelectListItem(mt.GetEnumDescription(), value.ToString());
                }
            });

        public static IEnumerable<SelectListItem> SignerMemberTypes =>
            AllMemberTypes.Join(MemberTypeExtensions.SignerTypes, sl => sl.Value, mt => ((int)mt).ToString(), (sl, mt) => sl);

        public static IEnumerable<SelectListItem> SenderMemberTypes =>
            AllMemberTypes.Join(MemberTypeExtensions.SenderTypes, sl => sl.Value, mt => ((int)mt).ToString(), (sl, mt) => sl);
    }
}
