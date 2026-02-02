// credits:
// https://github.com/Kno2/Kno2.ApiTestClient/blob/de2cc748e43691bef44b80747128b9b722d3b071/src/Kno2.ApiTestClient.Core/Helpers/EnumExtensions.cs

using System;
using System.ComponentModel;

namespace SutureHealth.Patients.Helpers
{
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var descriptionAttributes = (DescriptionAttribute[])
                (value.GetType().GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false));
            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : value.ToString();
        }
    }
}
