using System.Threading.Tasks;

namespace SutureHealth.Application.Services;

public interface IFhirUserConflationService
{
    Task<FhirUserConflation?> GetConflatedUser(string fhirId);
    Task ConflateUser(FhirUserConflation conflation);
}