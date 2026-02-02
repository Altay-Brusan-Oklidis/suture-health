using SutureHealth.Application;
using System.Text.RegularExpressions;

namespace SutureHealth.Requests
{
    public static class RequestExtensions
    {
        public static string GetRequestFileName(this ServiceableRequest request, MemberIdentity sutureUser)
        {
            var fileName = $"{request.Patient.LastName}_{request.Patient.FirstName}_DOB_{request.Patient.Birthdate:MM-dd-yyyy}__{(sutureUser.IsUserSender() ? request.Template.Name : request.Template.TemplateType.ShortName)}_{request.EffectiveDate.GetValueOrDefault(DateTime.UtcNow):MM-dd-yyyy}_{request.SutureSignRequestId}";
            var illegalChars = @"[\\\/:\*\?""'<>&| ]";
            return Regex.Replace(fileName, illegalChars, string.Empty);
        }
        public static string GetRequestShortFileName(this ServiceableRequest request, MemberIdentity sutureUser)
        {
            string fileName = string.Empty ;
            if (request.Template.TemplateType != null) 
            {             
                fileName = $"{request.Template.TemplateType.ShortName}_{request.EffectiveDate.GetValueOrDefault(DateTime.UtcNow):MM-dd-yyyy}_{request.SutureSignRequestId}";
            }            
            else 
            {
                fileName = $"Unknown_template_{request.EffectiveDate.GetValueOrDefault(DateTime.UtcNow):MM-dd-yyyy}_{request.SutureSignRequestId}";
            }
            var illegalChars = @"[\\\/:\*\?""'<>&| ]";
            return Regex.Replace(fileName, illegalChars, string.Empty);
        }
    }
}
