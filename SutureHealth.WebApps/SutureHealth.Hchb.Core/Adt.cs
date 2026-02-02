using System;
using System.Globalization;
using Newtonsoft.Json;
using Slack.Webhooks;
using Slack_Integration.Slack;
using SutureHealth.Hchb.JsonConverters;
using SutureHealth.Patients;

namespace SutureHealth.Hchb
{

    [JsonConverter(typeof(AdtConverter))]
    public class Adt
    {
        [JsonConverter(typeof(PatientConverter))]
        public Patient Patient { get; set; }

        [JsonConverter(typeof(HchbPatientConverter))]
        [JsonProperty("patient_hchb")]
        public HchbPatientWeb HchbPatient { get; set; }
        
        public ADT Type { get; set; }
        public string BranchCode { get; set; }
        public string MessageControlId { get; set; }
        public string RawFileName { get; set; }
        public string JsonFileName { get; set; }
        public string ClientApplicationId { get; set; }
        public string ClientFacilityId {  get; set; }

        public Adt() { }

        public Adt(string message)
        {
            
            JsonConverter[] converters = new JsonConverter[]
            { 
                new AdtConverter(),
                new PatientConverter(),
                new HchbPatientConverter() 
            };
            Adt messageInfo = JsonConvert.DeserializeObject<Adt>(message, converters);            

            this.Patient = messageInfo?.Patient;

            this.HchbPatient = messageInfo?.HchbPatient;
            this.Type = messageInfo.Type;
            this.BranchCode = messageInfo?.BranchCode;
            this.MessageControlId = messageInfo?.MessageControlId;
            this.RawFileName = messageInfo?.RawFileName;
            this.JsonFileName = messageInfo?.JsonFileName;
            this.ClientApplicationId = messageInfo?.ClientApplicationId;
            this.ClientFacilityId = messageInfo?.ClientFacilityId;
        }
    }
    public static class AdtExtentions
    {
        public static SlackMessage ToNotification(this Adt adt)
        {
            SlackMessageBuilder slackMessageBuilder = new SlackMessageBuilder();
            slackMessageBuilder.Add(":warning:* Patient Admit, Discharge or Transfer issue (_ADT_)*");
            slackMessageBuilder.Add(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            slackMessageBuilder.AddDivider();
            slackMessageBuilder.Add($"*Type*: {adt.Type} ");
            slackMessageBuilder.Add($"*MessageControlId*: {adt.MessageControlId} ");
            slackMessageBuilder.Add($"*Raw Message File*: {adt.RawFileName} ");
            slackMessageBuilder.Add($"*Processed Message File*: {adt.JsonFileName} ");

            return slackMessageBuilder.BuildSlackMessage();
        }

        public static SlackMessage ToPatientMatchNotification(this Adt adt, string branchName)
        {
            SlackMessageBuilder slackMessageBuilder = new SlackMessageBuilder();
            slackMessageBuilder.AddDivider();
            slackMessageBuilder.Add($"*From*: {branchName}");
            slackMessageBuilder.Add($"*Submitted*: _{DateTime.UtcNow.TimeOfDay.ToString("hh\\:mm\\:ss")}_ On {DateTime.UtcNow.Date.ToString("MM/dd/yyyy")}");
            slackMessageBuilder.Add($"*Source*: HCHB");
            //slackMessageBuilder.Add(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            return slackMessageBuilder.BuildSlackMessage();
        }

    }
}