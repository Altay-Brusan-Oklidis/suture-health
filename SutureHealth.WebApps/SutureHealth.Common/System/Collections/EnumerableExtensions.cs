using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            return source.GetType().GetProperties().ToDictionary(property => property.Name, property => (T) Convert.ChangeType(property.GetValue(source), typeof(T)));
        }
    }
}
