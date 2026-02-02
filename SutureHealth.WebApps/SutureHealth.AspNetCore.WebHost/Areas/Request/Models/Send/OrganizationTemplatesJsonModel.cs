namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class OrganizationTemplatesJsonModel
    {
        public string StandardTemplateGroupName { get; set; }
        public string OrganizationTemplateGroupName { get; set; }
        public IEnumerable<Template> StandardTemplates { get; set; }
        public IEnumerable<Template> OrganizationTemplates { get; set; }
        public bool HasStandardTemplates => StandardTemplates != null && StandardTemplates.Any();
        public bool HasOrganizationTemplates => OrganizationTemplates != null && OrganizationTemplates.Any();

        public class Template
        {
            public int TemplateId { get; set; }
            public string Summary { get; set; }
            public string ClinicalDateLabel { get; set; }
            public bool DiagnosisCodeAllowed { get; set; }
            public bool DiagnosisCodeRequired { get; set; }
            public bool PdfRequired { get; set; }
        }
    }
}
