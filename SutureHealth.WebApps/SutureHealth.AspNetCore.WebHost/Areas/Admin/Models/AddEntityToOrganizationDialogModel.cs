namespace SutureHealth.AspNetCore.Areas.Admin.Models
{
    public class AddEntityToOrganizationDialogModel
    {
        public string DialogName { get; set; }
        public string DialogTitle { get; set; }
        public string ConfirmFunctionName { get; set; }
        public string ConfirmButtonLabel { get; set; }
        public string OrganizationFieldName { get; set; }
        public string OrganizationDataSourceUrl { get; set; }
    }
}
