using Slack.Webhooks;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services
{
    public interface IHchbServiceProvider
    {
        Task<string> ProcessAdtMessage(string message, int logId = -1);
        Task<string> ProcessMdmMessage(string message, int logId = -1);
        Task<string> ProcessOruMessage(string message);
        int LogMessage(string type, string message);
        bool? IsProcessedMessage(int id);
        void LogReason(int id, string reason);
        Task<bool> SendNotificationAsync(string error,SlackMessage message);
        Task<bool> SendNotificationAsync(string error, string message);
        void CreateHchbPatient(int patientId, string messageControlId);
    }
}
