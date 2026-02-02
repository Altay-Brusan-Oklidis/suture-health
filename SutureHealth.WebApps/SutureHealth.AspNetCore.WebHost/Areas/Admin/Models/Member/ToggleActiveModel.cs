namespace SutureHealth.AspNetCore.Areas.Admin.Models.Member
{
    public class ToggleActiveModel
    {
        public const string EVENT_ACTIVATION_CHANGED = "ToggleActiveModal:ActivationChanged";

        public string UserName { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public bool HasOrganizationRelationship { get; set; }
        public bool HasPendingRequests { get; set; }
        public bool IsSoleAdministrator { get; set; }
        public string ToggleActiveActionUrl { get; set; }
        public bool CanPerformAction => (!IsCurrentlyActive && HasOrganizationRelationship) || (IsCurrentlyActive && !HasPendingRequests && !IsSoleAdministrator);
    }
}
