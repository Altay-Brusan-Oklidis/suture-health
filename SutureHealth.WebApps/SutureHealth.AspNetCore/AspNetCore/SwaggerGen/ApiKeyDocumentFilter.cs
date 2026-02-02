using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SutureHealth.AspNetCore.SwaggerGen
{
   public class ApiKeyDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions.Add("securityDefinitions", new OpenApiObject
            {
                ["api_key"] = new OpenApiObject
                {
                    ["type"] = new OpenApiString("apiKey"),
                    ["name"] = new OpenApiString(ApiKeyOperationFilter.X_API_KEY),
                    ["in"] = new OpenApiString("header")
                }
            });
        }
    }
}
