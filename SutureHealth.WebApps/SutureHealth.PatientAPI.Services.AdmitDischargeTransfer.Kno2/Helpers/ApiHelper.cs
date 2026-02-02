// credits:
// https://github.com/Kno2/Kno2.ApiTestClient/blob/de2cc748e43691bef44b80747128b9b722d3b071/src/Kno2.ApiTestClient.Core/Helpers/ApiHelper.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SutureHealth.Patients.Resources;

namespace SutureHealth.Patients.Helpers
{
    public static class ApiHelper
    {
        /// <summary>
        /// Creates a http client requeset using a simple c# anonymous object that is serialized into
        /// a string content object and sent to a API endpoint expecting a application/json media type
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="messageUri">The specific API endpoint for making draft id requests</param>
        /// <returns></returns>
        public static async Task<MessageResource> RequestMessageDraftAsync(HttpClient httpClient, Uri messageUri)
        {
            // Make a PUT request to the draft id endpoint.  It will return a draft id response as a bare string.
            HttpResponseMessage result = await httpClient.PutAsync(messageUri, null);
            string responseJson = await result.Content.ReadAsStringAsync();

            return Deserialize<MessageResource>(responseJson, httpClient.DefaultMediaType());
        }

        /// <summary>
        /// Creates a http request to get the available document types that the current users has access too
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="documentTypesUri">The specific API endpoint for making document types requests</param>
        /// <returns></returns>
        public static async Task<IEnumerable<string>> RequestDocumentTypesAsync(HttpClient httpClient, Uri documentTypesUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            HttpResponseMessage result = await httpClient.GetAsync(documentTypesUri);
            string responseJson = await result.Content.ReadAsStringAsync();

            var documentTypesResource = Deserialize<DocumentTypesResource>(responseJson, httpClient.DefaultMediaType());

            return documentTypesResource.DocumentTypes.Select(x => x.Name);
        }

        /// <summary>
        /// Creates a http request that will upload a file binary to an existing MessageResource draft.  The payload
        /// also includes information about the attachment or metadata
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="attachmentsUri"></param>
        /// <param name="fileName"></param>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public static async Task<AttachmentResource> UploadAttachmentAsync(HttpClient httpClient, Uri attachmentsUri, string fileName, AttachmentResource attachment)
        {
            string serializeObject = Serialize(attachment.AttachmentMeta, httpClient.DefaultMediaType());

            // This API requires a POST of both text based and binary data using MultipartContent
            //  https://msdn.microsoft.com/System.Net.Http.MultipartContent
            var multipartContent = new MultipartFormDataContent();

            // Using the StringContent (https://msdn.microsoft.com/System.Net.Http.StringContent) class to encode
            //  and setup the required mime type for this endpoint
            var contentString = new StringContent(serializeObject, Encoding.UTF8, httpClient.DefaultMediaType().Description());

            // Using the ByteArrayContent (https://msdn.microsoft.com/System.Net.Http.ByteArrayContent) class to encode
            //  and setup the required array buffer
            ByteArrayContent byteArrayContent = new ByteArrayContent(attachment.NativeFileBytes, 0, attachment.NativeFileBytes.Length);

            // Add the two content httpcontent based instances to the collection to be sent up to the API
            multipartContent.Add(contentString);
            multipartContent.Add(byteArrayContent, fileName, fileName);

            // Make a POST request to the draft id endpoint.  It will return a draft id response as a bare string.
            HttpResponseMessage result = await httpClient.PostAsync(attachmentsUri, multipartContent);
            string responseJson = await result.Content.ReadAsStringAsync();
            var attachmentResource = Deserialize<AttachmentResource>(responseJson, httpClient.DefaultMediaType());
            return attachmentResource;
        }

        /// <summary>
        /// Creates an http request that will request attachment metadata for a specific attachment.
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="attachmentsUri"></param>
        /// <returns>application/json</returns>
        public static async Task<AttachmentResource> RequestAttachmentMetadataAsync(HttpClient httpClient, Uri attachmentsUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            HttpResponseMessage result = await httpClient.GetAsync(attachmentsUri);
            var fileBytes = await result.Content.ReadAsStringAsync();
            var attachmentResource = Deserialize<AttachmentResource>(fileBytes, HttpClientExtensions.DefaultMediaType(httpClient));
            return attachmentResource;
        }

        /// <summary>
        /// Creates an http request that will request nateive attachment binary for a specific attachment.
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="attachmentsUri"></param>
        /// <param name="mediaType"></param>
        /// <returns>byte[]</returns>
        public static async Task<byte[]> RequestAttachmentAsync(HttpClient httpClient, Uri attachmentsUri, string mediaType)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, attachmentsUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            HttpResponseMessage result = await httpClient.SendAsync(request);
            var fileBytes = await result.Content.ReadAsByteArrayAsync();
            return fileBytes;
        }

        /// <summary>
        /// An example of a intake query request
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="documentsMessagesUri"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<MessageResource>> RequestUnprocessedIntakeMessagesAsync(HttpClient httpClient, Uri documentsMessagesUri)
        {
            IEnumerable<MessageResource> messageResources = Enumerable.Empty<MessageResource>();

            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            HttpResponseMessage result = await httpClient.GetAsync(documentsMessagesUri);
            string responseJson = await result.Content.ReadAsStringAsync();

            // parse the response.items for the messages and convert them to message resources
            JToken jToken = JObject.Parse(responseJson);
            if (string.IsNullOrWhiteSpace(responseJson))
                return messageResources;

            var messages = jToken.SelectToken("items").ToString();
            if (string.IsNullOrWhiteSpace(messages))
                return messageResources;

            messageResources = Deserialize<IEnumerable<MessageResource>>(messages, HttpClientExtensions.DefaultMediaType(httpClient));

            return messageResources;
        }

        /// <summary>
        /// Send draft performs the function of creating a MessageResource in a unsent state
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="messageUri"></param>
        /// <param name="messageResource"></param>
        public static void SendDraftAsync(HttpClient httpClient, Uri messageUri, MessageResource messageResource)
        {
            string serializeObject = Serialize(messageResource, HttpClientExtensions.DefaultMediaType(httpClient));
            httpClient.PutAsync(messageUri, new StringContent(serializeObject, Encoding.UTF8, HttpClientExtensions.DefaultMediaType(httpClient).Description()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="messageSendUri"></param>
        /// <param name="messageResource"></param>
        public static void SendReleaseAsync(HttpClient httpClient, Uri messageSendUri, MessageResource messageResource)
        {
            string serializeObject = Serialize(messageResource, HttpClientExtensions.DefaultMediaType(httpClient));
            httpClient.PostAsync(messageSendUri, new StringContent(serializeObject, Encoding.UTF8, HttpClientExtensions.DefaultMediaType(httpClient).Description()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="directoryValidateUri"></param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, bool>> ValidateAddressesAsync(HttpClient httpClient, Uri directoryValidateUri, params string[] addresses)
        {
            // Since this GET request takes a set of direct MessageResource addresses as the url parameters 
            //  we are using FormUrlEncodedContent to create the url query parameter
            var content = new FormUrlEncodedContent(addresses.Select(x => new KeyValuePair<string, string>("addresses", x)));
            string queryParameters = await content.ReadAsStringAsync();

            // Build up the address validation collection
            UriBuilder uriBuilder = new UriBuilder(httpClient.BaseAddress);
            uriBuilder.Path = directoryValidateUri.ToString();
            uriBuilder.Query = queryParameters;

            HttpResponseMessage result = await httpClient.GetAsync(uriBuilder.Uri);
            string responseJson = await result.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(responseJson);
            var addressValidationResults = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, JToken> validationResultItem in jObject)
            {
                bool isValid = Convert.ToBoolean(validationResultItem.Value);
                addressValidationResults.Add(validationResultItem.Key, isValid);
            }

            return addressValidationResults;
        }

        /// <summary>
        /// Downloads the the selected MessageResource body
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="messageUri"></param>
        /// <returns></returns>
        public static async Task<string> RequestMessageAsync(HttpClient httpClient, Uri messageUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            HttpResponseMessage result = await httpClient.GetAsync(messageUri);
            string responseJson = await result.Content.ReadAsStringAsync();
            return responseJson;
        }

        /// <summary>
        /// Send a MessageResource 'read' event to the server
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="messageReadEventUri"></param>
        public static void RequesetMessageReadEventAsync(HttpClient httpClient, Uri messageReadEventUri)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, messageReadEventUri);

            //create an anonymous object that will be serialized as json
            var request = new
            {
                isProcessed = true,
                processType = "emrexported"
            };

            // Using Json.Net (http://www.nuget.org/packages/Newtonsoft.Json/) we serialize the object into
            //  a json string
            string serializeObject = JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);

            var messageProcessedContent = new StringContent(serializeObject, Encoding.UTF8, "application/json");
            httpRequestMessage.Content = messageProcessedContent;

            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            httpClient.SendAsync(httpRequestMessage);
        }

        public static void RequestAttachmentReadEventAsync(HttpClient httpClient, Uri attachmentReadUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            httpClient.PutAsync(attachmentReadUri, null);
        }

        public static string Serialize<T>(T value, MediaType mediaType)
        {
            if (mediaType == MediaType.xml)
            {
                Encoding enc = Encoding.UTF8;

                using (var ms = new MemoryStream())
                {
                    var xmlWriterSettings = new XmlWriterSettings
                    {
                        CloseOutput = false,
                        Encoding = enc,
                        OmitXmlDeclaration = false,
                        Indent = true
                    };
                    using (XmlWriter xmlWriter = XmlWriter.Create(ms, xmlWriterSettings))
                    {
                        var s = new XmlSerializer(typeof(T));
                        s.Serialize(xmlWriter, value);
                    }

                    return enc.GetString(ms.ToArray());
                }
            }

            // Using Json.Net (http://www.nuget.org/packages/Newtonsoft.Json/) we serialize the object into
            //  a json string
            if (mediaType == MediaType.json)
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    Converters = new[] { new StringEnumConverter() },
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                return JsonConvert.SerializeObject(value, jsonSerializerSettings);
            }

            throw new SerializationException("no serializer for " + mediaType);
        }

        public static T Deserialize<T>(string rawValue, MediaType mediaType)
        {
            if (mediaType == MediaType.xml)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                StringReader reader = new StringReader(rawValue);

                T value = (T)xmlSerializer.Deserialize(reader);
                return value;
            }

            if (mediaType == MediaType.json)
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<T>(rawValue, jsonSerializerSettings);
            }

            throw new SerializationException("no serializer for " + mediaType);
        }
    }
}
