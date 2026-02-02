using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SutureHealth.AspNetCore.Areas.Network.Models.Listing
{
    public class EntityListItem
    {
        public SutureCustomerDetails SutureCustomer { get; set; }
        public bool IsSutureCustomer => SutureCustomer != null;
        public long ProviderId { get; set; }
        public long Npi { get; set; }
        public string Name { get; set; }
        public ProviderType ProviderType { get; set; }
        public IEnumerable<Relationship> Relationships { get; set; }
        public IEnumerable<string> ServiceTypes { get; set; }
        public string FullAddress { get; set; }
        public string Phone { get; set; }
        public double Distance { get; set; }
        public IEnumerable<ActionButton> Buttons { get; set; }
        public bool HasInvitationAction => this.Buttons != null && this.Buttons.Any(b => b == ActionButton.Invite);
        public long ClaimsWithUser { get; set; }
        public bool HasClaimsWithUser => this.ClaimsWithUser > 0;
        public bool HasBeenInvitedByUser { get; set; }
        public bool IsPreselected { get; set; }

        public bool HasTenDigitPhone => !string.IsNullOrWhiteSpace(this.Phone) && this.Phone.Length == 10;
        public bool HasMultipleButtons => this.Buttons != null && this.Buttons.Count() > 1;
        public bool HasSingleButton => this.Buttons != null && this.Buttons.Count() == 1;

        public class Relationship
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string FullAddress { get; set; }

            public bool HasTenDigitPhone => !string.IsNullOrWhiteSpace(this.Phone) && this.Phone.Length == 10;
        }

        public class SutureCustomerDetails
        {
            public long? SignerUserId { get; set; }
            public DateTimeOffset DateCreated { get; set; }
            public long DateCreatedTicks { get; set; }
            public string AccountType { get; set; }
            public bool IsInUsersNetwork { get; set; }
            public long EstimatedClaims { get; set; }
            public bool HasEstimatedClaims => this.EstimatedClaims > 0;
            public bool IsInNetwork { get; set; }
            public string ParentFacilityName { get; set; }
            public bool HasParentFacilityName => !string.IsNullOrWhiteSpace(this.ParentFacilityName);
        }

        public enum ActionButton
        {
            GetSignature,
            Invite,
            Message,
            Refer,
            Video,
            UpgradeMembership
        }
    }

    public enum ProviderType
    {
        Undefined = 0,
        Individual = 1,
        Organization = 2
    }

    public static class EntityListItemActionButtonExtensions
    {
        public static async Task<IHtmlContent> PartialAsync(this IHtmlHelper helper, EntityListItem.ActionButton button, object model = null)
        {
            switch (button)
            {
                case EntityListItem.ActionButton.GetSignature:
                    return await helper.PartialAsync("Buttons/_GetSignature", model);
                case EntityListItem.ActionButton.Invite:
                    return await helper.PartialAsync("Buttons/_Invite", model);
                case EntityListItem.ActionButton.Message:
                    return await helper.PartialAsync("Buttons/_Message", model);
                case EntityListItem.ActionButton.Refer:
                    return await helper.PartialAsync("Buttons/_Refer", model);
                case EntityListItem.ActionButton.Video:
                    return await helper.PartialAsync("Buttons/_Video", model);
                case EntityListItem.ActionButton.UpgradeMembership:
                    return await helper.PartialAsync("Buttons/_UpgradeMembership", model);
                default:
                    throw new NotImplementedException();
            }
        }

        public static EntityListItem.ActionButton[] RowButtonOrder { get; } = new EntityListItem.ActionButton[]
        {
            EntityListItem.ActionButton.UpgradeMembership,
            EntityListItem.ActionButton.Video,
            EntityListItem.ActionButton.Message,
            EntityListItem.ActionButton.Refer,
            EntityListItem.ActionButton.Invite,
            EntityListItem.ActionButton.GetSignature
        };

        public static EntityListItem.ActionButton[] GridButtonOrder { get; } = new EntityListItem.ActionButton[]
        {
            EntityListItem.ActionButton.UpgradeMembership,
            EntityListItem.ActionButton.GetSignature,
            EntityListItem.ActionButton.Video,
            EntityListItem.ActionButton.Message,
            EntityListItem.ActionButton.Refer,
            EntityListItem.ActionButton.Invite
        };

        public static HtmlString GetInvitationStateCssClass(this IHtmlHelper helper, EntityListItem entity)
        {
            if (entity.HasInvitationAction)
            {
                if (entity.HasBeenInvitedByUser)
                {
                    return new HtmlString("invite-pending");
                }
                else
                {
                    return new HtmlString("invite-loading");
                }
            }
            else
                return HtmlString.Empty;
        }
    }
}
