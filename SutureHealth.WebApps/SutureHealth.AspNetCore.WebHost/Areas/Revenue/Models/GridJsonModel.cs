namespace SutureHealth.AspNetCore.Areas.Revenue.Models
{
    public class GridJsonModel
    {
        public IEnumerable<Request> Requests { get; set; }
        public int TotalRecords { get; set; }
        public string CurrentReportRevenue { get; set; }
        public string CurrentReportRVU { get; set; }

        public class Request
        {
            public int FormId { get; set; }
            public string Practice { get; set; }
            public string PracticeNPI { get; set; }
            public string ProviderFirstName { get; set; }
            public string ProviderLastName { get; set; }
            public string ProviderSuffix { get; set; }
            public string ProviderCredential { get; set; }
            public string ProviderNPI { get; set; }
            public string PatientLastName { get; set; }
            public string PatientFirstName { get; set; }
            public string PatientSuffix { get; set; }
            public string PatientDOB { get; set; }
            public string Last4SSN { get; set; }
            public string ReferringProvider { get; set; }
            public string ReferringProviderNPI { get; set; }
            public string ReferringProviderMedicare { get; set; }
            public string DocumentType { get; set; }
            public string PlaceOfService { get; set; }
            public DateTime? ServiceDate { get; set; }
            public DateTime? EffectiveDate { get; set; }
            public string DiagnosisCode { get; set; }
            public string BillingCode { get; set; }
            public decimal Revenue { get; set; }
            public int Total { get; set; }
            public int Medicaid { get; set; }
            public int Medicare { get; set; }
            public int MedicareAdvantage { get; set; }
            public int PrivateInsurance { get; set; }
            public int SelfPay { get; set; }
            public decimal RVU { get; set; }

            public string ProviderDisplayName { get; set; }
            public string PatientDisplayName { get; set; }
            public string Payer { get; set; }
            public string PdfUrl { get; set; }
        }
    }
}
