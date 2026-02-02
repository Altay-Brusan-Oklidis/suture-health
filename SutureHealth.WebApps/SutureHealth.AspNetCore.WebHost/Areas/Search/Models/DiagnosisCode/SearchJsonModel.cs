namespace SutureHealth.AspNetCore.Areas.Search.Models.DiagnosisCode
{
    public class SearchJsonModel
    {
        public IEnumerable<DiagnosisCode> DiagnosisCodes { get; set; }

        public class DiagnosisCode
        {
            public int DiagnosisCodeId { get; set; }
            public string Summary { get; set; }
        }
    }
}
