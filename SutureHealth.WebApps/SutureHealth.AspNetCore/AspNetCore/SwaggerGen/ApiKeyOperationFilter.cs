using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using SutureHealth.AspNetCore.SwaggerGen.Attributes;

namespace SutureHealth.AspNetCore.SwaggerGen
{
   public class ApiKeyOperationFilter : IOperationFilter
   {
      public const string X_API_KEY = "x-api-key";

      void IOperationFilter.Apply(OpenApiOperation operation, OperationFilterContext context)
      {
         if (context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                                               .Union(context.MethodInfo.GetCustomAttributes(true))
                                               .OfType<ApiKeyRequiredAttribute>()
                                               .Any())
         {
            operation.Extensions.Add("security", new OpenApiObject
            {
               ["api_key"] = new OpenApiObject()
            });

            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            var apikey = operation.Parameters.FirstOrDefault(p => p.Name == X_API_KEY && p.In.Value.GetDisplayName() == "header");
            if (apikey != null)
            {
               operation.Parameters.Remove(apikey);
            }
         }
      }
   }
}
