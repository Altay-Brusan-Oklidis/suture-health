using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SutureHealth.Application;

namespace SutureHealth
{
    public class ApplicationUserException : ApplicationException
    {
        public ModelStateDictionary ModelState { get; set; }
        public MemberIdentity SutureUser { get; set; }

        public ApplicationUserException(MemberIdentity sutureUser, ModelStateDictionary modelState)
        {
            ModelState = modelState;
            SutureUser = sutureUser;
        }

        public ApplicationUserException(MemberIdentity sutureUser, string message, IdentityResult result)
            : base(message)
        {
            SutureUser = sutureUser;
            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }
        }
    }
}
