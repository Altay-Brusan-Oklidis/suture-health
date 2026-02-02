using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Requests;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Api
{
    public class SearchModel : BaseViewModel
    {
        public IEnumerable<SelectListItem> Statuses => new SelectListItem[] { new SelectListItem(string.Empty, string.Empty) }
                                                            .Union(Enum.GetValues<WorkflowStatus>()
                                                                       .Select(ws => new SelectListItem(ws.ToString(), ws.ToString(), ws == WorkflowStatus.Submitted)));
    }
}
