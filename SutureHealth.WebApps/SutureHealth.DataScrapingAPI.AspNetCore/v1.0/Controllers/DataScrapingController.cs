using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SutureHealth.DataScraping.Services;
using SutureHealth.Diagnostics;
using System.Net;

namespace SutureHealth.DataScraping.v0100.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("DataScraping")]
    [Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
    public class DataScrapingController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        /// <summary>
        /// IPatientServices.
        /// </summary>
        public IDataScrapingServicesProvider DataScrapingServices { get; }
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger<DataScrapingController> Logger { get; }
        /// <summary>
        /// Mapper.
        /// </summary>
        public IMapper Mapper { get; }
        /// <summary>
        /// Tracer.
        /// </summary>
        public ITracingService Tracer { get; }

        public DataScrapingController
        (
           IConfiguration configuration,
           IDataScrapingServicesProvider dataScrapingServices,
           IMapper mapper,
           ITracingService tracer,
           ILogger<DataScrapingController> logger
        )
        {
            Configuration = configuration;
            DataScrapingServices = dataScrapingServices;
            Logger = logger;
            Mapper = mapper;
            Tracer = tracer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("scrapeHtml", Name = "ScrapePatientData")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Models.ScrapPatientHtmlResponse))]  //TODO: change the response
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(Models.ScrapPatientHtmlRequest))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> ScrapePatientData([FromBody] Models.ScrapPatientHtmlRequest request)
        {
            //if (!ModelState.IsValid)
            //{
            //    // TODO: Need to provide more details about the failed validations.

            //    return Problem("Please provide a valid request.", null, (int)HttpStatusCode.BadRequest);
            //}

            var scpraHtmlRequest = this.Mapper.Map<ScrapPatientHtmlRequest>(request);
            var scrapedHtml = await DataScrapingServices.ScrapPatientHtmlAsync(scpraHtmlRequest);
            //var response = Mapper.Map<PatientMatchingResponse>(scrapedHtml);

            return Ok();
        }
    }
}
