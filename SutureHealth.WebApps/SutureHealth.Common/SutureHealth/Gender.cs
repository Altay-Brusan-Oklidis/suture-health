using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SutureHealth
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Gender
    {
        [EnumMember(Value = "U")] Unknown = 0,
        [EnumMember(Value = "M")] Male = 1,
        [EnumMember(Value = "F")] Female = 2,
        [EnumMember(Value = "A")] Ambiguous = 3,
    }
}
