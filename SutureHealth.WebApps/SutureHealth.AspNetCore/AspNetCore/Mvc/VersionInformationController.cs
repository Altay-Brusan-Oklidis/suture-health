using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SutureHealth.AspNetCore.Mvc
{
   [Produces("application/json")]
   public class VersionInformationController : ControllerBase
   {
      public const string CLOUD_ENVIRONMENT_KEY = "CloudEnvironment";
      protected virtual Assembly EntryAssembly { get; }

      public VersionInformationController()
      {
      }

      [HttpGet]
      public virtual IActionResult Get([FromServices] IConfiguration configuration)
      {
         try
         {
            var entryAssembly = EntryAssembly ?? this.RouteData.Values["EntryAssembly"] as Assembly;

            return Ok(new
            {
               Environment = configuration.GetValue<string>(CLOUD_ENVIRONMENT_KEY),
               AssemblyName = entryAssembly.GetName().Name,
               Version = new
               {
                  Informational = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
                  File = entryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version
               }
            });
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Error Getting Version Information");
         }
      }
   }
}
