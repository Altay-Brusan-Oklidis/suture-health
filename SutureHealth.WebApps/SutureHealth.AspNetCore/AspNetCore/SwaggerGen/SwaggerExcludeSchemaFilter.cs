using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using SutureHealth.AspNetCore.SwaggerGen.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SutureHealth.AspNetCore.SwaggerGen
{
   public class SwaggerExcludeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (model?.Properties == null || context == null)
                return;

            var excludedProperties = context.Type.GetProperties()
                        .Where(t =>t.GetCustomAttribute<SwaggerExcludeAttribute>()
                                != null);

            foreach (var excludedProperty in excludedProperties)
            {
                if (model.Properties.ContainsKey(excludedProperty.Name))
                {
                    model.Properties.Remove(excludedProperty.Name);
                }
            }
        }
    }
}
