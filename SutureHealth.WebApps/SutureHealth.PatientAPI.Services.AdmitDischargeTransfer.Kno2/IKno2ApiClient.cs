using System;
using System.Threading.Tasks;

namespace SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2
{
    public interface IKno2ApiClient
    {
        Task<string> RequestMessageAsync(Uri messageUri);
    }
}
