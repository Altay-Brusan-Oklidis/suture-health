using Newtonsoft.Json;
using Slack.Webhooks;
using Slack_Integration.Slack;
using SutureHealth.Hchb.JsonConverters;
using SutureHealth.Patients;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Text.Json;

namespace SutureHealth.Hchb
{
    [JsonConverter(typeof(MdmConverter))]
    public class Mdm
    {
        [JsonConverter(typeof(PatientMatchingRequestConverter))]
        [JsonProperty("patient")]
        public PatientMatchingRequest Patient { get; set; }
        
        [JsonConverter(typeof(TransactionConverter))]
        public HchbTransaction Transaction { get; set; }
        
        [JsonConverter(typeof(SignerConverter))]
        public Person Signer { get; set; }

        [JsonConverter(typeof(SenderConverter))]
        public Person Sender { get; set; }    
        
        public string MessageControlId { get; set; }        
        public string Rawfilename { get; set; }        
        public string Jsonfilename { get; set; }        
        public string Status { get; set; }
        public string ClientApplicationId { get; set; }
        public string ClientFacilityId { get; set; }

        public Mdm() { }

        public Mdm(string message)
        {
            JsonConverter[] converters = new JsonConverter[] 
            { 
                new MdmConverter(),
                new PatientMatchingRequestConverter(), 
                new TransactionConverter(), 
                new SenderConverter(), 
                new SignerConverter() 
            };

            Mdm messageInfo = JsonConvert.DeserializeObject<Mdm>(message,converters);

            this.Patient = messageInfo.Patient;
            this.Transaction = messageInfo.Transaction;
            this.Signer = messageInfo.Signer;
            this.Sender = messageInfo.Sender;
            this.MessageControlId = messageInfo.MessageControlId;
            this.Rawfilename = messageInfo.Rawfilename;
            this.Jsonfilename = messageInfo.Jsonfilename;
            this.Status = messageInfo.Status;
            this.ClientApplicationId = messageInfo?.ClientApplicationId;
            this.ClientFacilityId = messageInfo?.ClientFacilityId;
        }               
    }
    public static class MdmExtentions
    {
        public static SlackMessage ToNotification(this Mdm mdm)
        {
            SlackMessageBuilder slackMessageBuilder = new SlackMessageBuilder();
            slackMessageBuilder.Add(":warning:*Document issue (_MDM_)*");
            slackMessageBuilder.AddDivider();
            slackMessageBuilder.Add($"*MessageControlId*: {mdm.MessageControlId} ");
            slackMessageBuilder.Add($"*Raw Message File*: {mdm.Rawfilename} ");
            slackMessageBuilder.Add($"*Processed Message File*: {mdm.Jsonfilename} ");
            slackMessageBuilder.AddDivider();
            slackMessageBuilder.Add($"*Document*: {mdm.Transaction.FileName} ");
            slackMessageBuilder.AddDivider();
            slackMessageBuilder.Add($"*Admission Type*: {mdm.Transaction.AdmissionType} ");
            slackMessageBuilder.Add($"*Order Type*: {mdm.Transaction.ObservationId}^{mdm.Transaction.ObservationText} ");
            slackMessageBuilder.Add($"*Patient Type*: {mdm.Transaction.PatientType} ");
            slackMessageBuilder.Add($"*Order Number: {mdm.Transaction.OrderNumber} ");
            return slackMessageBuilder.BuildSlackMessage();
        }
    }
}
