namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class SaveResponse
    {
        public IEnumerable<string> Errors { get; set; }
        public bool Success => Errors != null && !Errors.Any();
    }
}
