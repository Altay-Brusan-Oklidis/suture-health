using MessageSenderAgent.Model.Notification;
using MessageSenderAgent.Model.Visit;
using MessageSenderAgent.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MessageSenderAgent.Builder
{
    public class TXASegmentBuilder
    {
        NotificationModel notificationModel;        
        
        public TXASegmentBuilder(ReferencedDataType dataType, 
                                 DocumentType? documentType = null, 
                                 string? setId = null,
                                 AuthenticatorType? authenticator=null,
                                 CompletionStatus? completionStatus= CompletionStatus.UNAUTH) 
        {
            notificationModel = new NotificationModel();
            
            string? name = Enum.GetName(typeof(ReferencedDataType), dataType);
            notificationModel.DocumentContentPresentation = name;

            if (documentType.HasValue)
            {
                notificationModel.DocumentType = documentType;
                //var dtName = Enum.GetRandomNameOrFamilyName(typeof(DocumentType), documentType.Value);
                //if (!string.IsNullOrEmpty(dtName))
                //{
                //    notificationModel.DocumentType = dtName;
                //}
            }
            if (authenticator != null) 
            {
                notificationModel.AssignedDocumentAuthenticator = authenticator;
            }
            else 
            {
                AuthenticatorType authen = new AuthenticatorType()
                {
                    FirstName = Utilities.GetRandomNameOrFamilyName("FirstName"),
                    LastName = Utilities.GetRandomNameOrFamilyName("LastName"),
                    NPI = Utilities.GetRandomalphabeticString(5),
                };
                notificationModel.AssignedDocumentAuthenticator = authen;
            }
            notificationModel.DocumentCompletionStatus = completionStatus;
            notificationModel.SetId = setId;         
        }

        public NotificationModel Build() 
        {

            notificationModel.ActivityDateTime= DateTime.Now.UpToMinuteString();
            notificationModel.OriginationDateTime = DateTime.Now.UpToMinuteString();
            
            return notificationModel;
        }
    }
}
