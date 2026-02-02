namespace SutureHealth.Documents
{
    public class TemplateType
    {
        public int TemplateTypeId { get; set; }
        public string Category { get; set; }
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public ClinicalDate DateAssociation { get; set; }
        public bool ShowIcd9 { get; set; }
        public bool AssociatePatient { get; set; }
        public bool RequireDxCode { get; set; }
        public bool IsActive { get; set; }
        public bool SignerChangeAllowed { get; set; }
    }

    public enum ClinicalDate
    {
        Unknown = 0,
        StartOfCare = 1,
        EffectiveDate = 2
    }
}
