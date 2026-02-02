using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SutureHealth.Application;
using SutureHealth.AspNetCore.Mvc;

namespace SutureHealth.AspNetCore.Models
{
    public class StatusResult
    {
        public StatusResult()
        { }

        public bool Succeeded { get; set; }
        public string Message { get; set; }

        public static StatusResult Success() => new() { Succeeded = true };
        public static StatusResult Success(string message) => new() { Succeeded = true, Message = message };
        public static StatusResult Failure(string message) => new() { Succeeded = false, Message = message };
    }

    public interface IBaseViewModel  : IIdentityProvider
    {
        bool RequireClientHeader { get; }
        bool RequireKendo { get; }
        bool RequireUI { get; }
        bool RequiresLoading { get; }
        bool RequireValidation { get; }
    }

    public class BaseViewModel : IBaseViewModel
    {
        public bool RequireClientHeader { get; set; } = true;
        public bool RequireKendo { get; set; } = false;
        public bool RequiresLoading { get; set; } = false;
        public bool RequireUI { get; set; } = false;
        public bool RequireValidation { get; set; } = false;

        public MemberIdentity CurrentUser { get; set; }
    }

    public class BasePageModel : PageModel, IBaseViewModel
    {
        public bool RequireClientHeader { get; set; } = true;
        public bool RequireKendo { get; set; } = false;
        public bool RequiresLoading { get; set; } = false;
        public bool RequireUI { get; set; } = false;
        public bool RequireValidation { get; set; } = false;

        public MemberIdentity CurrentUser { get; set; }
    }
}
