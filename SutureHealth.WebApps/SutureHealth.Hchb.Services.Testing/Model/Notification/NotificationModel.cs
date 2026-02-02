using SutureHealth.Hchb.Services.Testing.Model.Visit;
using NHapi.Model.V23.Datatype;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Security;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.PatientAPI.Services.Testing.Model.Notification
{

    public class NotificationModel
    {
        public string? SetId { get; set; }
        /// <summary>
        /// HCHB default balue is PDF; however, HL7 standard 
        /// suggests to use one of the items from table 0270.
        /// </summary>
        public DocumentType? DocumentType { get; set; }        
        public string? ActivityDateTime { get; set; }
        public string? DocumentContentPresentation { get; set; }
        public string? OriginationDateTime { get; set; }
        public AuthenticatorType? AssignedDocumentAuthenticator { get; set; }
        /// <summary>
        /// HCHB Branch Code
        /// </summary>
        [MaxLength(80)]
        public string? TranscriptionistCodeName { get; set; }
        /// <summary>
        /// This field contains a unique document identification number 
        /// assigned by the sending system. This document number is used 
        /// to assist the receiving system in matching future updates to 
        /// the document, as well as to identify the document in a query. 
        /// When the vendor does not provide a unique document ID number, 
        /// some Type of document identifier should be entered here, 
        /// or the Unique Document File name should be utilized. 
        /// Where the system does not customarily have a document filler number, 
        /// this number could serve as that value, as well.
        /// 
        /// <example>BranchCode_ordertypeid_orderID</example>
        /// </summary>
        [MaxLength(60)]
        public string? UniqueDocumentNumber   { get; set; }
        [MaxLength()]
        public string? FillerOrderNumber { get; set; }
        /// <summary>
        ///  HCHB defined the default value for Document Completion Status is equal to "UNAUTH"
        ///  However, HL7 standard defined the valid value table in  0271 - Document completion Status
        ///  
        /// </summary>
        [MaxLength(60)]        
        public CompletionStatus? DocumentCompletionStatus { get; set; }

    }
}
