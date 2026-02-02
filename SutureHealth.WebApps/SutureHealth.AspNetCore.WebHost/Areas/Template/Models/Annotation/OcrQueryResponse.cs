namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class OcrQueryResponse
    {
        public bool ResultFound { get; set; }
        public OcrQueryResult BindingResult { get; set; }
        public IEnumerable<OcrQueryResult> OtherResults { get; set; }
    }
}
