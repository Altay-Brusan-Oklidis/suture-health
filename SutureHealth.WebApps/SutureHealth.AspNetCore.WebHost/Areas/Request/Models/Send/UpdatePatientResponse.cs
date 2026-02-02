namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class UpdatePatientResponse
    {
        public IEnumerable<string> Errors { get; set; }
        public bool Success => Errors == null || !Errors.Any();
    }
}
