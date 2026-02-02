using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using SutureHealth.DataScraping;
using SutureHealth.DataScraping.Services;
using SutureHealth.DataScraping.Scrapers;
using SutureHealth.Patients.Services;
using SutureHealth.Patients;
using AutoMapper;
using SutureHealth.Storage;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SutureHealth.DataScraping.Services
{
    public class DataScrapingServicesProvider : IDataScrapingServicesProvider
    {
        protected DataScrapingDbContext DataScrapingContext { get; set; }
        protected IConfiguration Configuration { get; }
        protected ILogger<IDataScrapingServicesProvider> Logger { get; } 
        protected IPatientServicesProvider PatientServicesProvider { get; }
        protected IMapper Mapper { get; }
        public IStorageService StorageService { get; }
        protected IQueueService QueueService { get; }        
        protected readonly string DataScrapingQueueUrl;
        protected readonly string ScrapedHtmlS3Bucket;



        public DataScrapingServicesProvider
        (
           DataScrapingDbContext dataScrapingContext,
           IConfiguration configuration,
           ILogger<IDataScrapingServicesProvider> logger,           
           IPatientServicesProvider patientServicesProvider,
           IMapper mapper,
           IStorageService storageService,
           IQueueService queueService
        )
        {
            Logger = logger;
            Configuration = configuration;
            DataScrapingContext = dataScrapingContext;
            PatientServicesProvider = patientServicesProvider;
            Mapper = mapper;
            StorageService = storageService;
            QueueService = queueService;

            var section = configuration.GetSection("SutureHealth:DataScrapingServicesProvider");
            DataScrapingQueueUrl = section.GetValue("DataScrapingQueue", " ");
            ScrapedHtmlS3Bucket = section.GetValue("ScrapedHtmlBucket", " ");
        }

        public async Task<List<PatientMatchingResponse>> ScrapPatientHtmlAsync(ScrapPatientHtmlRequest scrapPatientHtmlRequest)
        {                   
            List<PatientMatchingResponse> patientMatchingReponses = new List<PatientMatchingResponse>();

            HtmlParser parser = new HtmlParser();
            var htmlDoc = parser.ParseDocument(scrapPatientHtmlRequest.Html);
            OpenEmrPatientDetailScraper openEmrDetailScraper = new OpenEmrPatientDetailScraper(htmlDoc);
            OpenEmrPatientListScraper openEmrPatientListScraper = new OpenEmrPatientListScraper(htmlDoc);          

            var scrapedPatientDetail = openEmrDetailScraper.Scrape(); // -->Works fine.
            var scrapedPatientList = openEmrPatientListScraper.Scrape(); // --> Works fine too.           

            if (scrapedPatientList.ScrapedPatients.Count > 0)
            {                
                foreach (var scrapedPatient in scrapedPatientList.ScrapedPatients)
                {
                    scrapedPatient.URL = scrapPatientHtmlRequest.PageUrl;
                    scrapedPatient.CreatedAt = scrapPatientHtmlRequest.ScrapedAt;

                    //Patient matching request
                    PatientMatchingRequest patientMatchingRequest = new PatientMatchingRequest();
                    Identifier identifier = new Identifier();
                    identifier.Type = KnownTypes.UniqueExternalIdentifier;
                    identifier.Value = scrapedPatient.ExternalId;

                    patientMatchingRequest.Ids = new List<IIdentifier>() { identifier };
                    patientMatchingRequest.FirstName = scrapedPatient.FirstName;
                    patientMatchingRequest.MiddleName = scrapedPatient.MiddleName;                     
                    patientMatchingRequest.LastName = scrapedPatient.LastName;
                    patientMatchingRequest.Birthdate = (DateTime)scrapedPatient.DateOfBirth;
                    patientMatchingRequest.ManualReviewEnabled = true;

                    patientMatchingReponses.Add(await PatientServicesProvider.MatchAsync(patientMatchingRequest));

                    //AWS Queue processes
                    QueueMessageRequest scrapedPatientRequest = new QueueMessageRequest();
                    Dictionary<string, QueueMessageAttributeValue> requestTypeDictionary = new Dictionary<string, QueueMessageAttributeValue>()
                    {
                        { "RequestType", new QueueMessageAttributeValue() { StringValue = "SimplePatient", DataType="String"} }
                    };
                    scrapedPatientRequest.QueueUrl = DataScrapingQueueUrl;
                    scrapedPatientRequest.MessageBody = JsonSerializer.Serialize(scrapedPatient);
                    scrapedPatientRequest.MessageAttributes = requestTypeDictionary;
                    await QueueService.QueueMessageAsync(scrapedPatientRequest);

                    //DB processes                                        
                    await DataScrapingContext.CreateScrapedPatientHistoryAsync(scrapedPatient);
                }
            }            

            if(!string.IsNullOrEmpty(scrapedPatientDetail.FirstName) && !string.IsNullOrEmpty(scrapedPatientDetail.LastName))
            {
                scrapedPatientDetail.URL = scrapPatientHtmlRequest.PageUrl;
                scrapedPatientDetail.CreatedAt = scrapPatientHtmlRequest.ScrapedAt;

                //Patient matching request
                PatientMatchingRequest patientMatchingRequest = new PatientMatchingRequest();
                Identifier identifier = new Identifier();
                identifier.Type = KnownTypes.UniqueExternalIdentifier;
                identifier.Value = scrapedPatientDetail.ExternalId;

                patientMatchingRequest.Ids = new List<IIdentifier>() { identifier };
                patientMatchingRequest.FirstName = scrapedPatientDetail.FirstName;
                patientMatchingRequest.MiddleName = scrapedPatientDetail.MiddleName;
                patientMatchingRequest.LastName = scrapedPatientDetail.LastName;
                patientMatchingRequest.Gender = await GenderIdentifierAsync(scrapedPatientDetail.Gender);
                patientMatchingRequest.Birthdate = (DateTime)scrapedPatientDetail.DateOfBirth;
                patientMatchingRequest.ManualReviewEnabled = false;
                patientMatchingReponses.Add(await PatientServicesProvider.MatchAsync(patientMatchingRequest));

                //AWS Queue processes
                QueueMessageRequest scrapedPatientDetailRequest = new QueueMessageRequest();
                Dictionary<string, QueueMessageAttributeValue> requestTypeDictionary = new Dictionary<string, QueueMessageAttributeValue>()
                    {
                        { "RequestType", new QueueMessageAttributeValue() { StringValue = "DetailedPatient", DataType="String"} }
                    };
                scrapedPatientDetailRequest.QueueUrl = DataScrapingQueueUrl;
                scrapedPatientDetailRequest.MessageBody = JsonSerializer.Serialize(scrapedPatientDetail);
                scrapedPatientDetailRequest.MessageAttributes = requestTypeDictionary;
                await QueueService.QueueMessageAsync(scrapedPatientDetailRequest);

                //DB Processes
                await DataScrapingContext.CreateScrapedPatientDetailHistoryAsync(scrapedPatientDetail);                
            }

            //S3 bucket processes
            StorageServicePutRequest htmlPutRequest = new StorageServicePutRequest()
            {
                ContentBody = scrapPatientHtmlRequest.Html,
                ContentType = "text/html",
                Container = ScrapedHtmlS3Bucket,
                Key = scrapPatientHtmlRequest.ScrapedAt.ToString()
            };
            await StorageService.PutObjectAsync(htmlPutRequest);

            return patientMatchingReponses;
        }

        public async Task<ScrapedPatientDetail?> GetScrapedPatientDetailById(string externalId)
        {
            return await DataScrapingContext.GetScrapedPatientDetailById(externalId);
        }

        public async Task<ScrapedPatient?> GetScrapedPatientById(string externalId)
        {
            return await DataScrapingContext.GetScrapedPatientById(externalId);
        }

        public async Task CreateScrapedPatientDetail(ScrapedPatientDetail scrapedPatientDetail)
        {
            await DataScrapingContext.CreateScrapedPatientDetailAsync(scrapedPatientDetail);
        }

        public async Task CreateScrapedPatient(ScrapedPatient scrapedPatient)
        {
            await DataScrapingContext.CreateScrapedPatientAsync(scrapedPatient);
        }

        public async Task UpdateScrapedPatientDetail(ScrapedPatientDetail scrapedPatientDetail)
        {
            await DataScrapingContext.UpdateScrapedPatientDetail(scrapedPatientDetail);
        }

        public async Task UpdateScrapedPatient(ScrapedPatient scrapedPatient)
        {
            await DataScrapingContext.UpdateScrapedPatient(scrapedPatient);
        }

        public async Task RemoveDuplicatePatients()
        {
            await DataScrapingContext.RemoveDuplicatePatients();
        }

        public async Task RemoveDuplicatePatientDetails()
        {
            await DataScrapingContext.RemoveDuplicatePatientDetails();

        }
        public async Task<Gender> GenderIdentifierAsync(string genderText)
        {
            if (genderText.IsNullOrEmpty())
            {
                return Gender.Unknown;
            }
            else if (genderText.ToUpper().StartsWith("M"))
            {
                return Gender.Male;
            }
            else if (genderText.ToUpper().StartsWith("F"))
            {
                return Gender.Female;
            }
            else if (genderText.ToUpper().StartsWith("A"))
            {
                return Gender.Ambiguous;
            }
            else
            {
                return Gender.Unknown;
            }
        }
    }
}
