using Slack.Webhooks;
using Slack_Integration.Slack;
using SutureHealth.Hchb.JsonConverters;
using System.Text.Json.Serialization;

namespace SutureHealth.Hchb
{
    [JsonConverter(typeof(OruConverter))]
    public class Oru
    {
        public int RequestId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string PatientId { get; set; }
        public string EpisodeId { get; set; }
        public string OrderNumber { get; set; }
        public string ObservationId { get; set; }
        public string ObservationText { get; set; }
        public string RequestedDateTime { get; set; }
        public string RejectReason { get; set; }
        public string ResultDate { get; set; }
        public string ResultStatus { get; set; }
    }

    public static class OruExtentions
    {
        public static SlackMessage ToNotification(this Oru oru)
        {
            SlackMessageBuilder slackMessageBuilder = new SlackMessageBuilder();
            slackMessageBuilder.Add(":warning:*Document issue (_ORU_)*");
            slackMessageBuilder.AddDivider();
            slackMessageBuilder.Add($"*Order Number*: {oru.OrderNumber} ");
            slackMessageBuilder.Add($"*Result Status*: {oru.ResultStatus} ");
            slackMessageBuilder.Add($"*Reject Reason*: {oru.RejectReason} ");
            return slackMessageBuilder.BuildSlackMessage();
        }
    }
}