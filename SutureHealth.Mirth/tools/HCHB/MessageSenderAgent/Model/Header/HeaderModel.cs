using Microsoft.VisualBasic;
using NHapi.Base.Model;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MessageSenderAgent.Model.Header
{
    public class HeaderModel
    {
        public readonly string ReceivingFacility = String.Empty;
        public string FieldSeparator { get; } = "|";
        public string EncodingCharacters { get; } = "^~\\&";
        /// <summary>
        /// Each HCHB database will have a unique identifier. The receiving application
        /// will need to utilize this identifier to route messages to the appropriate
        /// customer environment on the receiving side.This identifier must be used in
        /// conjunction with the HCHB database security key(see MSH 4).
        /// <para>
        /// Note that some HCHB customers may include multiple databases. This will
        /// still result in a unique identifier per database.The receiving application will
        /// need to be able to consolidate these to meet business requirements.
        /// </para>        
        /// </summary>
        public string SendingApplication { get; set; } = "AgencyID";
        /// <summary>
        /// Each HCHB database will have a unique security key which will be a GUID. The
        /// receiving application will need to correlate the security key with the HCHB
        /// database identifier, and only when these two values match should the
        /// message be accepted into the specified customer environment.
        /// It is the receiving application’s responsibility to reject messages whose MSH 3
        /// and MSH 4 values do not correlate to one another, based on a predetermined
        /// exchange of information with HCHB.These values serve as a
        /// “username/password” credential authentication for the message.
        /// </summary>
        public string SendingFacility { get; set; } = "TransactionID";
        public string ReceivingApplication { get; set; } = "VendorName";        
        public string DateTimeOfMessage { get; private set; } = DateTime.Now.ToString("yyyyMMddhhmm");
        [Required]
        public MessageType MessageType { get; set; } = new MessageType();
        public string MessageControlID { get; set; } = String.Empty;
        public ProcessingIdType ProcessingID { get; set; } = ProcessingIdType.P;
        public string VersionID { get; } = "2.5";
        public string? CharacterSet { get; set; }        
    }
}
