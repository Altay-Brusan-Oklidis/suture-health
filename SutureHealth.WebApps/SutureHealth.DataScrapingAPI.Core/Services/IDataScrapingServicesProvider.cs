using SutureHealth.Patients;

namespace SutureHealth.DataScraping.Services
{
    public interface IDataScrapingServicesProvider
    {
        Task<List<PatientMatchingResponse>> ScrapPatientHtmlAsync(ScrapPatientHtmlRequest scrapPatientHtmlRequest);
        Task CreateScrapedPatientDetail(ScrapedPatientDetail scrapedPatientDetail);
        Task CreateScrapedPatient(ScrapedPatient scrapedPatient);
        Task<ScrapedPatientDetail?> GetScrapedPatientDetailById(string externalId);
        Task<ScrapedPatient?> GetScrapedPatientById(string externalId);
        Task UpdateScrapedPatientDetail(ScrapedPatientDetail scrapedPatientDetail);
        Task UpdateScrapedPatient(ScrapedPatient scrapedPatient);
        Task RemoveDuplicatePatientDetails();
        Task RemoveDuplicatePatients();
        Task<Gender> GenderIdentifierAsync(string genderText);
    }
}
