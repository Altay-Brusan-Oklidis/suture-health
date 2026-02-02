using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.Application.Services;
using SutureHealth.Services;

namespace SutureHealth.Application.v0100.Controllers;

[Route("api/v{version:apiVersion}/conflation")]
public class FhirUserConflationController : SutureHealth.AspNetCore.Mvc.ControllerBase
{
    [HttpPost("user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Member))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    public async Task<IActionResult> GetConflatedUser([FromServices] IApplicationService applicationService,
        [FromBody] string fhirId)
    {
        if (fhirId.IsNullOrWhiteSpace() || fhirId.Length > 50)
        {
            return BadRequest(new Error("fhirId is invalid."));
        }
        try
        {
            var conflation = await applicationService.GetConflatedUser(fhirId);
            if (conflation is null)
            {
                return NotFound("A fhir user conflation does not exist for this user.");
            }
            var member = await applicationService.GetMemberByIdAsync(conflation.UserId);
            return Ok(member);
        }
        catch (Exception e)
        {
            return Problem($"An unexpected error has occurred. {e.Message}", statusCode: 500);
        }
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    public async Task<IActionResult> CreateConflation([FromServices] IApplicationService applicationService,
        [FromBody] FhirUserConflation conflation)
    {
        if (!conflation.IsValid())
        {
            return BadRequest("Cannot create conflation with invalid input.");
        }
        try
        {
            await applicationService.ConflateUser(conflation);
        }
        catch (Exception e)
        {
            return Problem($"An unexpected error has occurred. {e.Message}", statusCode: 500);
        }
        return Ok();
    }
}