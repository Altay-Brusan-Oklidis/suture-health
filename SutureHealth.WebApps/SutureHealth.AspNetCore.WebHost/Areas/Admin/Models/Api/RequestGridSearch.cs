using SutureHealth.Requests;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Api
{
    public class RequestGridSearch
    {
        public int? OrganizationId { get; set; }
        public WorkflowStatus? Status { get; set; }
    }
}
