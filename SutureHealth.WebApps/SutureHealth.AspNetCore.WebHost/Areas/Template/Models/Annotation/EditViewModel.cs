using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class EditViewModel : BaseViewModel
    {
        public int OrganizationId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public bool IsParentTemplate { get; set; }
        public EditorViewModel Editor { get; set; }
        public string SaveReturnUrl { get; set; }
        public string CancelReturnUrl { get; set; }
    }
}
