using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Admin.Pages
{
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class DashboardModel : BasePageModel
    {
        private readonly IAuthorizationService _authorizationService;

        public IEnumerable<Tile> Tiles { get; set; }

        public DashboardModel
        (
            IAuthorizationService authorizationService
        )
        {
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> OnGet()
        {
            var showPatientsInfoAuthzResult = await _authorizationService.AuthorizeAsync(User, AuthorizationPolicies.ShowPatientsInfo);
            var showPatientsInfo = showPatientsInfoAuthzResult.Succeeded;
            Tiles = new Tile[]
            {
                new Tile
                {
                    Name = "Organizations",
                    Description = "Current Organization in the system",
                    Url = Url.RouteUrl("AdminOrganizationIndex")
                },
                new Tile
                {
                    Name = "Users",
                    Description = "Current Users in the system",
                    Url = Url.RouteUrl("AdminMemberSearch")
                },
                new Tile
                {
                    Name = "Patients",
                    Description = "Current Patients in the system",
                    Url = Url.RouteUrl("AdminPatientSearch")
                },
                new Tile
                {
                    Name = "System",
                    Description = "Manage system settings",
                    Url = Url.RouteUrl("AdminSystemIndex")
                },
                new Tile
                {
                    Name = "Notifications",
                    Description = "Audit system notifications",
                    Url = Url.Page("/Notifications", new { area = "Admin" })
                },
                new Tile
                {
                    Name = "Support Review",
                    Description = "Tasks for Support Team to resolve",
                    Url = Url.RouteUrl("AdminReviewIndex"),
                    //Count = await RequestService.GetTransmittedRequestsCountAsync(SutureHealth.Requests.WorkflowStatus.PatientUnderReview)
                },
                new Tile
                {
                    Name = "API",
                    Description = "API Request management",
                    Url = Url.RouteUrl("AdminApiSearch")
                }
            }
            .Where(x => x.Name != "Patients" || showPatientsInfo);

            return Page();
        }

        public class Tile
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public int? Count { get; set; }
        }
    }
}
