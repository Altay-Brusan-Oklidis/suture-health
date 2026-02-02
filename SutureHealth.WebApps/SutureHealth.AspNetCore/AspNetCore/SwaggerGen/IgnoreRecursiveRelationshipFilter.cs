using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace SutureHealth.AspNetCore.SwaggerGen
{
   public class IgnoreRecursiveRelationshipFilter : ISchemaFilter
    {
        void ISchemaFilter.Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (model.Properties == null)
                return;

            var excludeList = from prop in context.Type.GetProperties()
                              where prop.PropertyType == context.Type || (prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Any(t => context.Type.IsAssignableFrom(t)))
                              select ToCamelCase(prop.Name);
            foreach (var prop in excludeList)
            {
                if (model.Properties.ContainsKey(prop) && model.Properties[prop]?.Items != null)
                {
                    model.Properties[prop].Items.Reference = null;
                    model.Properties[prop].Items.Type = "object";
                }
            }
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // if the next character is a space, which is not considered uppercase 
                    // (otherwise we wouldn't be here...)
                    // we want to ensure that the following:
                    // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                    // The code was written in such a way that the first word in uppercase
                    // ends when if finds an uppercase letter followed by a lowercase letter.
                    // now a ' ' (space, (char)32) is considered not upper
                    // but in that case we still want our current character to become lowercase
                    if (char.IsSeparator(chars[i + 1]))
                    {
                        chars[i] = ToLower(chars[i]);
                    }

                    break;
                }

                chars[i] = ToLower(chars[i]);
            }

            return new string(chars);
        }

        private static char ToLower(char c)
        {
#if HAVE_CHAR_TO_STRING_WITH_CULTURE
            c = char.ToLower(c, CultureInfo.InvariantCulture);
#else
            c = char.ToLowerInvariant(c);
#endif
            return c;
        }

    }
}
