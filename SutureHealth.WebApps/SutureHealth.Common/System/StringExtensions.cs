using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        public static StringComparison DefaultIgnoreCaseComparison = StringComparison.OrdinalIgnoreCase;

        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static bool EqualsIgnoreCase(this string value, string toCompare) => string.Equals(value, toCompare, DefaultIgnoreCaseComparison);

        public static bool EqualsAnyIgnoreCase(this string value, IEnumerable<string> toCompare) => toCompare.Any(x => x.EqualsIgnoreCase(value));

        public static string GetLast(this string source, int numberOfChars)
        {
            if (source.IsNullOrEmpty())
                return null;
            if (numberOfChars >= source.Length)
                return source;
            return source.Substring(source.Length - numberOfChars);
        }

        public static string Interpolate<T>(this string source, T context)
        {
            if (source.IsNullOrEmpty() || context == null)
                return null;
            else
                return Regex.Replace(source, @"{(?<exp>[^}]+)}", match =>
                {
                    var parameter = Expression.Parameter(typeof(T), typeof(T).Name);
                    var expression = match.Groups["exp"].Value;
                    var properties = expression.Split('.')[1..];
                    Expression parameterExpression = parameter;
                    foreach (var member in properties)
                    {
                        parameterExpression = Expression.PropertyOrField(parameterExpression, member);
                    }

                    var lambda = Expression.Lambda<Func<T, string>>(Expression.Call(parameterExpression, "ToString", Type.EmptyTypes), new[] { parameter });
                    var compilation = lambda.Compile();
                    return compilation(context);
                });
        }
    }
}
