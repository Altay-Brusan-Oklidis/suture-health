using System.Text.Json.Serialization;

namespace SutureHealth
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Hl7
    {
        ADT = 0,
        MDM = 1,
        ORU = 3,
        NOTDEFINED = 4
    }

    public enum ADT
    {
        A01 = 0,
        A03 = 1,
        A04 = 2,
        A08 = 3,
        A11 = 4,
    }
}
