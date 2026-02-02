// credits:
// https://github.com/Kno2/Kno2.ApiTestClient/blob/de2cc748e43691bef44b80747128b9b722d3b071/src/Kno2.ApiTestClient.Core/Helpers/HttpClientExtensions.cs

using System.Net.Http;
using System.Net.Http.Headers;

namespace SutureHealth.Patients.Helpers
{
    public static class HttpClientExtensions
    {
        public static MediaType DefaultMediaType(this HttpClient source)
        {
            return source.DefaultRequestHeaders.DefaultMediaType();
        }

        public static MediaType DefaultMediaType(this HttpRequestHeaders source)
        {
            if (source.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
                return MediaType.json;
            if (source.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/xml")))
                return MediaType.xml;

            return MediaType.unknown;
        }
    }

}
