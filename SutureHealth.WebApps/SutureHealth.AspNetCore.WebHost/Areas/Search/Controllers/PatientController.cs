using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Search.Models.Patient;
using SutureHealth.Patients;
using SutureHealth.Patients.Services;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Search.Controllers
{
    [Area("Search")]
    [Route("Search/Patient")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PatientController : Controller
    {
        protected IPatientServicesProvider PatientService { get; }
        protected IIdentityService IdentityService { get; }
        protected IMemberService MemberService { get; }

        public PatientController
        (
            IPatientServicesProvider patientService,
            IIdentityService identityService,
            IMemberService memberService
        )
        {
            PatientService = patientService;
            IdentityService = identityService;
            MemberService = memberService;
        }

        [HttpPost]
        [Route("", Name = "SearchPatient")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            var patientOrganizationIdScope = Array.Empty<int>() as IEnumerable<int>;

            if (request.OrganizationId.HasValue)
            {
                var allowedOrganizationIds = await IdentityService.GetOrganizationMembersByMemberId(CurrentUser.MemberId)
                                                                  .Where(om => om.IsActive)
                                                                  .Select(om => om.OrganizationId)
                                                                  .ToArrayAsync();

                if (!(allowedOrganizationIds.Contains(request.OrganizationId.Value) || await MemberService.IsMemberSurrogateSenderAsync(CurrentUser)))
                {
                    return Unauthorized();
                }
            }

            if ((request.Search ?? string.Empty).Length < 2)
            {
                return Ok(new SearchJsonModel() { Patients = Array.Empty<SearchJsonModel.Patient>() });
            }

            var query = (CurrentUser.IsUserSender(), CurrentUser.IsUserPhysician(), request.OrganizationId.HasValue) switch
            {
                (true, _, true) => PatientService.QueryPatientsForSenderByOrganizationId(request.OrganizationId.Value).Include(p => p.OrganizationKeys),
                (true, _, false) => PatientService.QueryPatientsForSenderByMemberId(CurrentUser.MemberId).Include(p => p.OrganizationKeys),
                (false, true, _) => PatientService.SearchPatientsForSigner(CurrentUser.MemberId, request.Search, request.OrganizationId),
                (false, false, _) => PatientService.SearchPatientsForAssistant(CurrentUser.MemberId, request.Search, request.OrganizationId)
            };

            if (CurrentUser.IsUserSender())
            {
                var words = (request.Search ?? string.Empty).Split(' ').Select(w => Regex.Replace(w, @"[^A-Za-z0-9]+", string.Empty));

                patientOrganizationIdScope = await PatientService.GetOrganizationIdsInPatientScopeForSenderAsync(CurrentUser.MemberId, request.OrganizationId);

                foreach (var word in words)
                {
                    query = query.Where(p => EF.Functions.Like(p.LastName, $"%{word}%") || EF.Functions.Like(p.FirstName, $"%{word}%") || EF.Functions.Like(p.Suffix, $"%{word}%") ||
                                                p.OrganizationKeys.Any(pok => patientOrganizationIdScope.Any(oid => pok.OrganizationId == oid) && EF.Functions.Like(pok.MedicalRecordNumber, $"%{word}%")));
                }
            }

            query = query.OrderBy(p => p.LastName)
                         .ThenBy(p => p.FirstName)
                         .ThenBy(p => p.Suffix)
                         .Take(request.Count.GetValueOrDefault(10));

            return Ok(new SearchJsonModel()
            {
                Patients = CurrentUser.IsUserSender() ?
                                (await query.ToArrayAsync())
                                .SelectMany(p => p.OrganizationKeys.Join(patientOrganizationIdScope, pok => pok.OrganizationId, oid => oid, (pok, oid) => pok)
                                                                   .Where(pok => !string.IsNullOrWhiteSpace(pok.MedicalRecordNumber))
                                                                   .GroupBy(pok => pok.MedicalRecordNumber, (key, grp) => grp.First())
                                                                   .DefaultIfEmpty(p.OrganizationKeys.FirstOrDefault()), (p, pok) => new SearchJsonModel.Patient()
                                                                   {
                                                                       PatientId = p.PatientId,
                                                                       Summary = p.GetSearchSummary(pok?.OrganizationId)
                                                                   }) :
                                (await query.ToArrayAsync())
                                .Select(p => new SearchJsonModel.Patient()
                                {
                                    PatientId = p.PatientId,
                                    Summary = p.GetSearchSummary()
                                })
            });
        }
    }
}
