
namespace SutureHealth.DataScraping.Services.Lambda
{
    public partial class Function
    {
        public async Task RemoveDuplicatePatients(Amazon.Lambda.CloudWatchEvents.ScheduledEvents.ScheduledEvent scheduledEvent)
        {
            await DataScrapingService.RemoveDuplicatePatients();
            await DataScrapingService.RemoveDuplicatePatientDetails();
        }
    }
 
}
