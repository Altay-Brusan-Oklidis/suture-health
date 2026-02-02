#if !NETSTANDARD1_1
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SutureHealth
{
   public static class EnumMemberExtensions
    {
        public static string GetEnumMemberValue<T>(this T value) where T : struct
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }

        public static T ToEnum<T>(this string value)
        {
            var enumType = typeof(T);
            foreach (var name in Enum.GetNames(enumType))
            {
                var enumMemberAttribute = enumType.GetField(name).GetCustomAttributes<EnumMemberAttribute>(true).SingleOrDefault();
                if ((enumMemberAttribute != null && enumMemberAttribute.Value == value) || string.Equals(name, value, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)Enum.Parse(enumType, name);
                }
            }
            
            return default(T);
        }

        public static string GetEnumDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}
#endif