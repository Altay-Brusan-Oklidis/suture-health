using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace SutureHealth.AspNetCore.Mvc.ApplicationModels;

public class ControllerHidingAppConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        // We can also do it with an application model convention
        // Replace with any logic you want
        foreach (ControllerModel controller in application.Controllers.Where(c => c.ControllerType.IsAssignableTo(typeof(Microsoft.AspNetCore.Mvc.Controller))))
        {
            // This does not work
            controller.ApiExplorer.IsVisible = false;

            // We need to hide all the actions
            foreach (ActionModel action in controller.Actions)
            {
                action.ApiExplorer.IsVisible = false;
            }
        }
    }
}