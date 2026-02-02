using SutureHealth.Reporting;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Reporting
{
    public class ReportViewModel : SutureHealth.AspNetCore.Models.BaseViewModel
    {
        public string CallbackUrl { get; set; }
        public IEnumerable<IReportViewModel> Reports { get; set; }
        public Type ComponentType { get; internal set; }
    }
}
