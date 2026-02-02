// credits:
// https://github.com/Kno2/Kno2.ApiTestClient/blob/de2cc748e43691bef44b80747128b9b722d3b071/src/Kno2.ApiTestClient.Core/Helpers/MediaType.cs

using System.ComponentModel;

namespace SutureHealth.Patients.Helpers
{
    public enum MediaType
    {
        unknown,
        [Description("application/json")]
        json,
        [Description("application/xml")]
        xml
    }
}
