using Microsoft.AspNetCore.Mvc;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Models
{
    public class CheckForVideoVisitResponse
    {
        public SutureHealth.Visits.Core.Visit Visit { get; set; }
        public IActionResult ErrorAction { get; set; }
        public bool VisitFound => Visit != null;
        public bool HasError => ErrorAction != null;
    }
}
