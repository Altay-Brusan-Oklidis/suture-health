using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Organization
{
    public class UsersViewModel : BaseViewModel
    {
        public int OrganizationId { get; set; }
        public bool IsAdministrator { get; set; }
        public bool OrganizationHasAdministrator { get; set; }
        public AlertViewModel NewUserAddedAlert { get; set; }
        public bool HasNewUserAddedAlert => NewUserAddedAlert != null;
    }
}
