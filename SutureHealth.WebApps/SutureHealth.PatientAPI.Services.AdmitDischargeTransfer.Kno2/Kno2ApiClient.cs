using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2
{
    public class Kno2ApiClient : IKno2ApiClient
    {
        public Kno2ApiClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        HttpClient HttpClient { get; }

        public async Task<string> RequestMessageAsync(Uri messageUri)
        {
            HttpResponseMessage result = await HttpClient.GetAsync(messageUri);
            string responseJson = await result.Content.ReadAsStringAsync();
            return responseJson;
        }
    }
}
