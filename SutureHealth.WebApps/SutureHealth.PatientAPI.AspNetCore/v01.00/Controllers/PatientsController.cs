using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Asp.Versioning;
using SutureHealth.Patients.Services;
using SutureHealth.Application.Services;
using SutureHealth.Diagnostics;
using SutureHealth.Patients.v0100.Models.Patients;
using SutureHealth.Patients.v0100.Models;
using Domain = SutureHealth.Patients;
using System.Text.RegularExpressions;
using System;
using SutureHealth.Requests.Services;
using Amazon.Auth.AccessControlPolicy;

namespace SutureHealth.Patients.v0100.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("patients")]
[ControllerName("ApiPatients")]
public class PatientsController : SutureHealth.AspNetCore.Mvc.ControllerBase
{
    /// <summary>
    /// Configuration.
    /// </summary>
    public IConfiguration Configuration { get; }
    /// <summary>
    /// IPatientServices.
    /// </summary>
    public IPatientServicesProvider PatientServices { get; }
    /// <summary>
    /// Logger.
    /// </summary>
    public ILogger<PatientsController> Logger { get; }
    /// <summary>
    /// Mapper.
    /// </summary>
    public IMapper Mapper { get; }
    /// <summary>
    /// Tracer.
    /// </summary>
    public ITracingService Tracer { get; }
    /// <summary>
    /// Initialized the variable of <see cref="PatientsController"/> class.
    /// </summary>
    /// <param name="configuration">configuration</param>
    /// <param name="patientServices">patientServices</param>
    /// <param name="mapper">mapper</param>
    /// <param name="tracer">tracer</param>
    /// <param name="logger">logger</param>
    public PatientsController
    (
        IConfiguration configuration,
        IPatientServicesProvider patientServices,
        IMapper mapper,
        ITracingService tracer,
        ILogger<PatientsController> logger
    )
    {
        Configuration = configuration;
        PatientServices = patientServices;
        Logger = logger;
        Mapper = mapper;
        Tracer = tracer;
    }

    /// <summary>
    /// Api for getting the patient by patient id.
    /// </summary>
    /// <param name="patientId">The patient id</param>
    /// <returns></returns>
    [HttpGet("{patientId:int}", Name = "GetPatientById")]
    [Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Models.Patient))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetById(int patientId)
    {
        var patient = await PatientServices.GetByIdAsync(patientId);
        if (patient == null)
        {
            return Problem($"Can not find {patientId}", null, (int)HttpStatusCode.NotFound);
        }

        return Ok(Mapper.Map<Models.Patient>(patient));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AuthorizedApiUser)]
    [Route("match", Name = "GetMatchingPatients")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Models.PatientMatchingResponse))]
    [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(Models.PatientMatchingResponse))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Match([FromBody] Models.PatientMatchingRequest request)
    {
        if (!ModelState.IsValid)
        {
            // TODO: Need to provide more details about the failed validations.

            return Problem("Please provide a valid request.", null, (int)HttpStatusCode.BadRequest);
        }

        var matchRequest = this.Mapper.Map<Domain.PatientMatchingRequest>(request);
        matchRequest.ManualReviewEnabled = true;
        var matches = await PatientServices.MatchAsync(matchRequest);
        var response = Mapper.Map<Models.PatientMatchingResponse>(matches);

        return Ok(response);
    }

    /// <summary>
    /// Search for patients in scope of the current user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("search")]
    [Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PatientListItem[]))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Search
    (
        [FromBody] SearchRequest request,
        [FromServices] IIdentityService identityService,
        [FromServices] IInboxServicesProvider inboxService
    )
    {
        if (request.OrganizationId.HasValue)
        {
            var allowedOrganizationIds = await identityService.GetOrganizationMembersByMemberId(CurrentUser.MemberId)
                                                              .Where(om => om.IsActive)
                                                              .Select(om => om.OrganizationId)
                                                              .ToArrayAsync();

            if (!allowedOrganizationIds.Contains(request.OrganizationId.Value))
            {
                return Problem("You are not authorized to search for patients at the organization with the specified Id.", statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        if ((request.Search ?? string.Empty).Length < 3 && !(bool)request.IsCpoModal)
        {
            return Ok(Array.Empty<PatientListItem>());
        }

        var query = (CurrentUser.IsUserSender(), CurrentUser.IsUserPhysician(), request.OrganizationId.HasValue) switch
        {
            (true, _, true) => PatientServices.QueryPatientsForSenderByOrganizationId(request.OrganizationId.Value),
            (true, _, false) => PatientServices.QueryPatientsForSenderByMemberId(CurrentUser.MemberId),
            (false, true, _) => PatientServices.SearchPatientsForSigner(CurrentUser.MemberId, request.Search, request.OrganizationId),
            (false, false, _) => PatientServices.SearchPatientsForAssistant(CurrentUser.MemberId, request.Search, request.OrganizationId)
        };

        if (CurrentUser.IsUserSender())
        {
            var words = (request.Search ?? string.Empty).Split(' ').Select(w => Regex.Replace(w, @"[^A-Za-z0-9]+", string.Empty));
            var patientOrganizationIdScope = await PatientServices.GetOrganizationIdsInPatientScopeForSenderAsync(CurrentUser.MemberId, request.OrganizationId);

            foreach (var word in words)
            {
                query = query.Where(p => EF.Functions.Like(p.LastName, $"%{word}%") || EF.Functions.Like(p.FirstName, $"%{word}%") || EF.Functions.Like(p.Suffix, $"%{word}%") ||
                                            p.OrganizationKeys.Any(pok => patientOrganizationIdScope.Any(oid => pok.OrganizationId == oid) && EF.Functions.Like(pok.MedicalRecordNumber, $"%{word}%")));
            }
        }

        var date = DateTime.Now;

        var patients =  query.Take(request.Count.GetValueOrDefault(10)).ToList();
        var cpoEntries = await inboxService.GetCpoEntries()
                            .Where(cpo => patients.Select(p => p.PatientId).Contains(cpo.PatientId))
                            .Where(cpo => cpo.EffectiveAt.Year == date.Year && cpo.EffectiveAt.Month == date.Month)
                            .ToArrayAsync();

        var patientList = Mapper.Map<Models.PatientListItem[]>(patients, opt =>
            opt.Items["CpoEntries"] = cpoEntries
        );

        foreach ( var patient in patientList )
        {
            var patientEntries = cpoEntries.Where(cpo => cpo.PatientId == patient.PatientId).ToList();
            patient.TotalCpoTimeThisMonth = patientEntries.Sum(cpo => cpo.Minutes);
        }

        if((bool)request.IsCpoModal)
        {
            var cpoModalPatients = (await query.ToArrayAsync());
            var mappedCpoModalPatients = Mapper.Map<Models.PatientListItem[]>(cpoModalPatients, opt =>
            opt.Items["CpoEntries"] = cpoEntries);

            foreach (var patient in mappedCpoModalPatients)
            {
                var patientEntries = cpoEntries.Where(cpo => cpo.PatientId == patient.PatientId).ToList();
                patient.TotalCpoTimeThisMonth = patientEntries.Sum(cpo => cpo.Minutes);
            }
            return Ok(mappedCpoModalPatients.OrderByDescending(p => p.TotalCpoTimeThisMonth).Take(request.Count.GetValueOrDefault(10)).ToArray());
        }
        return Ok(patientList);
    }
}
