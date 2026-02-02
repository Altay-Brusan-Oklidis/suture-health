using Moq;
using SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2;

namespace SutureHealth.PatientAPI.Services.Kno2.Lambda.Testing;

internal class Kno2ApiClientMock : Mock<IKno2ApiClient>
{
    public const string Id = "xtxgte7y6qzeemsc4p235t4bz47zgh6utvnb6gaa";
    public static readonly Uri Uri = new Uri($"https://api.kno2fy-integration.com/api/messages/{Id}");
    private const string kno2MessageJson = $@"{{
  ""threadId"": null,
  ""organizationId"": ""m7m6n6uyx5rnegch3pzehkvmb66ecs5utvnb6gaa"",
  ""originalObjectId"": null,
  ""attachments"": [],
  ""signers"": [],
  ""messageDate"": ""2022-07-29T12:47:47.19"",
  ""createdDate"": ""2022-07-29T12:47:47.19"",
  ""updatedDate"": null,
  ""fromAddress"": ""records@suturehealth.direct.kno2fy-integration.com"",
  ""toAddress"": ""records@suturehealth.direct.kno2fy-integration.com"",
  ""patientName"": ""Ivan Ivanov"",
  ""properties"": {{}},
  ""subject"": ""Miscellaneous"",
  ""body"": ""Miscellaneous from SutureHealth\n\nMiscellaneous for: \nPatient ID: 123123123\nPatient Name: Ivan Petrov Ivanov\nGender: M\nDOB: 08/24/1972\nAddress: Balkan str. 681\nPernik, PR 2300\nPhone: 00359761234567\n\nComments:\nfor testing"",
  ""patient"": {{
    ""patientId"": """",
    ""patientIdRoot"": """",
    ""patientIds"": [],
    ""firstName"": ""Ivan"",
    ""middleName"": ""Petrov"",
    ""lastName"": ""Ivanov"",
    ""suffix"": null,
    ""gender"": ""M"",
    ""visitId"": null,
    ""issuer"": null,
    ""birthDate"": ""1972-08-24"",
    ""visitDate"": null,
    ""fullName"": ""Ivan Ivanov"",
    ""zip"": null,
    ""streetAddress1"": ""Balkan str. 681"",
    ""streetAddress2"": null,
    ""city"": ""Pernik"",
    ""state"": ""PR"",
    ""postalCode"": ""2300"",
    ""country"": null,
    ""telephone"": null,
    ""telecom"": [],
    ""integrationMeta"": null
  }},
  ""reasonForDisclosure"": null,
  ""origin"": ""Kno2ToKno2"",
  ""isProcessed"": false,
  ""isUrgent"": false,
  ""isDraft"": false,
  ""status"": ""Received"",
  ""processedType"": ""None"",
  ""processTypes"": [],
  ""priority"": ""NotUrgent"",
  ""channelId"": ""882c1f11bf5b4baf8f099d7154dacb7e"",
  ""sourceType"": ""DirectMessage"",
  ""unprocessedNotificationSent"": null,
  ""attachments2Pdf"": false,
  ""attachments2Cda"": false,
  ""attachments2HL7"": false,
  ""attachmentSendType"": null,
  ""releaseTypeId"": null,
  ""isNew"": true,
  ""conversation"": null,
  ""integrationLogs"": [],
  ""type"": ""Intake"",
  ""messageType"": ""default"",
  ""classification"": {{
    ""code"": null,
    ""name"": null,
    ""scheme"": null
  }},
  ""hispMessageIds"": [],
  ""id"": ""{Id}""
}}
";

    //isNew = false
    private const string kno2MessageJsonUpdated = $@"{{
  ""threadId"": null,
  ""organizationId"": ""m7m6n6uyx5rnegch3pzehkvmb66ecs5utvnb6gaa"",
  ""originalObjectId"": null,
  ""attachments"": [],
  ""signers"": [],
  ""messageDate"": ""2022-07-29T12:47:47.19"",
  ""createdDate"": ""2022-07-29T12:47:47.19"",
  ""updatedDate"": null,
  ""fromAddress"": ""records@suturehealth.direct.kno2fy-integration.com"",
  ""toAddress"": ""records@suturehealth.direct.kno2fy-integration.com"",
  ""patientName"": ""Ivan Ivanov"",
  ""properties"": {{}},
  ""subject"": ""Miscellaneous"",
  ""body"": ""Miscellaneous from SutureHealth\n\nMiscellaneous for: \nPatient ID: 123123123\nPatient Name: Ivan Petrov Ivanov\nGender: M\nDOB: 08/24/1972\nAddress: Balkan str. 681\nPernik, PR 2300\nPhone: 00359761234567\n\nComments:\nfor testing"",
  ""patient"": {{
    ""patientId"": """",
    ""patientIdRoot"": """",
    ""patientIds"": [],
    ""firstName"": ""Ivan"",
    ""middleName"": ""Petrov"",
    ""lastName"": ""Ivanov"",
    ""suffix"": null,
    ""gender"": ""M"",
    ""visitId"": null,
    ""issuer"": null,
    ""birthDate"": ""1972-08-24"",
    ""visitDate"": null,
    ""fullName"": ""Ivan Ivanov"",
    ""zip"": null,
    ""streetAddress1"": ""Balkan str. 681"",
    ""streetAddress2"": null,
    ""city"": ""Pernik"",
    ""state"": ""PR"",
    ""postalCode"": ""2300"",
    ""country"": null,
    ""telephone"": null,
    ""telecom"": [],
    ""integrationMeta"": null
  }},
  ""reasonForDisclosure"": null,
  ""origin"": ""Kno2ToKno2"",
  ""isProcessed"": false,
  ""isUrgent"": false,
  ""isDraft"": false,
  ""status"": ""Received"",
  ""processedType"": ""None"",
  ""processTypes"": [],
  ""priority"": ""NotUrgent"",
  ""channelId"": ""882c1f11bf5b4baf8f099d7154dacb7e"",
  ""sourceType"": ""DirectMessage"",
  ""unprocessedNotificationSent"": null,
  ""attachments2Pdf"": false,
  ""attachments2Cda"": false,
  ""attachments2HL7"": false,
  ""attachmentSendType"": null,
  ""releaseTypeId"": null,
  ""isNew"": false,
  ""conversation"": null,
  ""integrationLogs"": [],
  ""type"": ""Intake"",
  ""messageType"": ""default"",
  ""classification"": {{
    ""code"": null,
    ""name"": null,
    ""scheme"": null
  }},
  ""hispMessageIds"": [],
  ""id"": ""{Id}""
}}
";

    public Kno2ApiClientMock()
    {
        SetupSequence(x => x.RequestMessageAsync(Uri))
            .ReturnsAsync(kno2MessageJson)
            .ReturnsAsync(kno2MessageJsonUpdated);
    }
}
