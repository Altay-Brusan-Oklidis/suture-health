using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Search.Models.DiagnosisCode;
using SutureHealth.Requests.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Search.Controllers;

[Area("Search")]
[Route("Search/DiagnosisCode")]
public class DiagnosisCodeController : Controller
{
    protected IRequestServicesProvider RequestService { get; }

    public DiagnosisCodeController
    (
        IRequestServicesProvider requestService
    )
    {
        RequestService = requestService;
    }

    [HttpPost]
    [Route("", Name = "SearchDiagnosisCode")]
    public async Task<JsonResult> Search([FromBody] SearchRequest request)
        => new JsonResult(new SearchJsonModel()
        {
            DiagnosisCodes = (await RequestService.SearchDiagnosisCodesAsync(request.Search, request.Count.GetValueOrDefault(10))).Select(dc => new SearchJsonModel.DiagnosisCode()
            {
                DiagnosisCodeId = dc.DiagnosisCodeId,
                Summary = $"{dc.Code} ({(dc.CodeType == 10 ? "ICD-10" : "ICD-9")}) {dc.Description}"
            })
        });
}

