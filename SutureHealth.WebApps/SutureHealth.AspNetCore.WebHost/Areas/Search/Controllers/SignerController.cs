using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Search.Models.Signer;
using SutureHealth.Application.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Search.Controllers;

[Area("Search")]
[Route("Search/Signer")]
public class SignerController : Controller
{
    protected IApplicationService SecurityService { get; set; }

    public SignerController
    (
        IApplicationService securityService
    )
    {
        SecurityService = securityService;
    }

    [HttpPost]
    [Route("", Name = "SearchSigner")]
    [Route("Member", Name = "SearchSigningMembers")]
    public async Task<IActionResult> SigningMembers([FromBody] SearchRequest request)
        => Json(new SearchJsonModel<SigningMember>()
        {
            Signers = (await SecurityService.GetSigningOrganizationMembersAsync(request.Search, request.OrganizationStateOrProvinceFilter, request.Count ?? 10))
                                            .Select(om => om.Member)
                                            .Select(m => new SigningMember
                                            {
                                                MemberId = m.MemberId,
                                                Summary = $"{m.LastName}, {m.FirstName}" +
                                                                (!string.IsNullOrWhiteSpace(m.Suffix) ? $" {m.Suffix}" : string.Empty) +
                                                                (!string.IsNullOrWhiteSpace(m.ProfessionalSuffix) ? $" {m.ProfessionalSuffix}" : string.Empty) +
                                                                (!string.IsNullOrWhiteSpace(m.NPI) ? $" (NPI: {m.NPI})" : string.Empty)
                                            })
        });

    [HttpPost]
    [Route("OrganizationMember", Name = "SearchSigningOrganizationMembers")]
    public async Task<IActionResult> SigningOrganizationMembers([FromBody] SearchRequest request)
        => Json(new SearchJsonModel<SigningOrganizationMember>()
        {
            Signers = (await SecurityService.GetSigningOrganizationMembersAsync(request.Search, request.OrganizationStateOrProvinceFilter, request.Count ?? 10)).OrderByDescending(o => o.IsPrimary)
                                            .Select(om => new SigningOrganizationMember
                                            {
                                                OrganizationMemberId = om.OrganizationMemberId,
                                                OrganizationId = om.OrganizationId,
                                                MemberId = om.MemberId,
                                                Summary = $"{om.Member.LastName}, {om.Member.FirstName}" +
                                                            (!string.IsNullOrWhiteSpace(om.Member.Suffix) ? $" {om.Member.Suffix}" : string.Empty) +
                                                            (!string.IsNullOrWhiteSpace(om.Member.ProfessionalSuffix) ? $" {om.Member.ProfessionalSuffix}" : string.Empty) +
                                                            (!string.IsNullOrWhiteSpace(om.Member.NPI) ? $" (NPI: {om.Member.NPI})" : string.Empty) +
                                                            $" {om.Organization.Name} ({om.Organization.City}, {om.Organization.StateOrProvince})"
                                            })
        });
}
