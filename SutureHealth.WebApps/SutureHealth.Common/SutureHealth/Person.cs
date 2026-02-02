using System;
using System.Collections.Generic;
using System.Text;

namespace SutureHealth
{
    public abstract class Person
    {
        public static string GetFullName(string lastName = null, string firstName = null, string suffix = null, string professionalSuffix = null)
        {
            var sb = new StringBuilder().Append(string.Join(", ", lastName, firstName));
            if (!string.IsNullOrWhiteSpace(suffix))
                sb.AppendFormat(" {0}", suffix);
            if (!string.IsNullOrWhiteSpace(professionalSuffix))
                sb.AppendFormat(" {0}", professionalSuffix);

            return sb.ToString();
        }
    }
}
