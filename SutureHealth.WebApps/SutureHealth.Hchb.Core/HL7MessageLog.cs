using Newtonsoft.Json;
using SutureHealth.Hchb.JsonConverters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SutureHealth.Hchb
{
#nullable enable
    [JsonConverter(typeof(HL7MessageLogConverter))]
    public class HL7MessageLog
    {
        public int Id { get; set; }
        public string? MessageControlId { get; set; }
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public string? Message { get; set; }
        public string? RawMessageFile { get; set; }
        public string? JsonMessageFile { get; set; }
        public bool? IsProcessed { get; set; }
        public string? HchbPatientId { get; set; }
        public string? EpisodeId { get; set; }
        public string? Status { get; set; }
        public string? ICDCode { get; set; }
        public string? Reason { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        
        public HL7MessageLog() 
        { }
        public HL7MessageLog(string message, string type)
        {

            var messagelog = JsonConvert.DeserializeObject<HL7MessageLog>(message, new HL7MessageLogConverter());
            if(messagelog != null) 
            {
                this.Type = type;
                this.SubType = messagelog.SubType;
                this.Message = message.Substring(0, 1000);
                this.ReceivedDate = DateTime.Now;
                this.IsProcessed = false;
                this.MessageControlId = messagelog.MessageControlId;
                this.RawMessageFile = messagelog.RawMessageFile;
                this.JsonMessageFile = messagelog.JsonMessageFile;

                if (Enum.Parse<Hl7>(type.ToUpper()) == Hl7.ADT)
                {
                    Adt adt = new (message);
                    this.HchbPatientId = adt.HchbPatient.HchbPatientId;
                    this.EpisodeId = adt.HchbPatient.EpisodeId;
                    this.Status = adt.HchbPatient.Status;
                    this.ICDCode = adt.HchbPatient.IcdCode;
                }
                else
                {
                    Mdm mdm = new (message);
                    this.HchbPatientId = mdm.Transaction.HchbPatientId;
                    this.EpisodeId = mdm.Transaction.EpisodeId;
                    this.Status = null;
                    this.ICDCode = null;
                }

            }
        }
    }
}
