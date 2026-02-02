using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Request.Models.Send;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.Documents.Services;
using SutureHealth.Patients.Services;
using SutureHealth.Requests.Services;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.Storage;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Request.Controllers
{
    [Area("Request")]
    [Route("Request/Send")]
    public class SendController : Controller
    {
        private readonly int[] F2F_TEMPLATE_IDS = new int[] { 1000, 1001 };

        protected IDocumentServicesProvider DocumentService { get; }
        protected IRequestServicesProvider RequestService { get; }
        protected IApplicationService SecurityService { get; }
        protected IPatientServicesProvider PatientService { get; }
        protected IBinaryStorageService StorageService { get; }
        protected IConfiguration Configuration { get;  }

        public SendController
        (
            IDocumentServicesProvider documentService,
            IApplicationService securityService,
            IRequestServicesProvider requestService,
            IPatientServicesProvider patientService,
            IBinaryStorageService storageService,
            IConfiguration configuration
        )
        {
            DocumentService = documentService;
            RequestService = requestService;
            SecurityService = securityService;
            PatientService = patientService;
            StorageService = storageService;
            Configuration = configuration;
        }

        [HttpGet]
        [Route("", Name = "SendIndex")]
        public async Task<IActionResult> Index([FromQuery] int? templateId, bool contentOnly = false)
        {
            return View(await InitializeSendModel(templateId: templateId, contentOnly: contentOnly));
        }

        [HttpPost]
        [Route("OrganizationsForSurrogateSender", Name = "OrganizationsForSurrogateSender")]
        public async Task<IActionResult> GetOrganizationsForSurrogateSender([FromBody] FromOfficesRequest request)
        {
            var defaultMaxUploadKilobytes = await SecurityService.GetMaxUploadKilobytesAsync();

            if (request.ExpandSearch)
            {
                var orgs = await SecurityService.GetOrganizations()
                    .Where(o => o.IsActive &&
                        !(o.OrganizationTypeId == 10003 || o.OrganizationTypeId == 10004) &&
                        (o.Name.Contains(request.Contains) || o.NPI.Contains(request.Contains)))
                    .GroupJoin(
                        SecurityService.GetOrganizationMembersByMemberId(CurrentUser.Id).Where(om => om.IsActive),
                        o => o.OrganizationId,
                        om => om.OrganizationId,
                        (o, oms) => new { o, oms })
                    .SelectMany(x => x.oms.DefaultIfEmpty(), (g, om) => new { g.o, om })
                    .GroupJoin(
                        SecurityService.GetOrganizationSettings()
                            .Where(s => s.Key == "DocumentSizeLimit" && s.IsActive == true && s.ItemInt != null),
                        x => x.o.OrganizationId,
                        s => s.ParentId,
                        (x, s) => new
                        {
                            Organization = x.o,
                            OrganizationMember = x.om,
                            MaxUploadBytes = (s.FirstOrDefault() != null ?
                                s.First().ItemInt.GetValueOrDefault(defaultMaxUploadKilobytes) :
                                defaultMaxUploadKilobytes) * 1024
                        })
                    .Select(x => new SendModel.FromOffice()
                    {
                        OrganizationId = x.Organization.OrganizationId,
                        Name = !string.IsNullOrWhiteSpace(x.Organization.OtherDesignation) ? x.Organization.OtherDesignation : x.Organization.Name,
                        IsPrimary = x.OrganizationMember != null && x.OrganizationMember.IsPrimary,
                        City = x.Organization.City,
                        NPI = x.Organization.NPI,
                        IsPayingClient = !x.Organization.IsFree,
                        StateOrProvince = x.Organization.StateOrProvince,
                        CanSendRequests = x.Organization.OrganizationTypeId != 10004,
                        HasIncompleteProfile = !x.Organization.IsValidForSendingRequests(),
                        MaxUploadBytes = x.MaxUploadBytes
                    })
                    .ToListAsync();

                return Json(orgs);
            }

            var states = (await SecurityService.GetSurrogateSendingOrganizationsAsync(CurrentUser.MemberId))
                .Select(x => x.OrganizationMember.Organization.StateOrProvince);

            var surrogateAreaOrgs = await SecurityService.GetOrganizations()
                .Where(o => o.IsActive &&
                    !(o.OrganizationTypeId == 10003 || o.OrganizationTypeId == 10004) &&
                    (o.Name.Contains(request.Contains) || o.NPI.Contains(request.Contains)) &&
                    states.Any(s => s == o.StateOrProvince))
                .ToListAsync();

            var userOrgs = await SecurityService.GetOrganizationMembersByMemberId(CurrentUser.Id)
                    .Where(om => om.IsActive &&
                        (om.Organization.Name.Contains(request.Contains) || om.Organization.NPI.Contains(request.Contains)))
                    .Select(om => om.Organization)
                    .ToListAsync();
            var result = surrogateAreaOrgs.Union(userOrgs)
                .GroupJoin(
                    SecurityService.GetOrganizationMembersByMemberId(CurrentUser.Id).Where(om => om.IsActive),
                    o => o.OrganizationId,
                    om => om.OrganizationId,
                    (o, oms) => new { o, oms })
                .SelectMany(x => x.oms.DefaultIfEmpty(), (g, om) => new { g.o, om })
                .GroupJoin(
                    SecurityService.GetOrganizationSettings()
                        .Where(s => s.Key == "DocumentSizeLimit" && s.IsActive == true && s.ItemInt != null),
                    x => x.o.OrganizationId,
                    s => s.ParentId,
                    (x, s) => new
                    {
                        Organization = x.o,
                        OrganizationMember = x.om,
                        MaxUploadBytes = (s.FirstOrDefault() != null ?
                            s.First().ItemInt.GetValueOrDefault(defaultMaxUploadKilobytes) :
                            defaultMaxUploadKilobytes) * 1024
                    })
                .Select(x => new SendModel.FromOffice()
                {
                    OrganizationId = x.Organization.OrganizationId,
                    Name = !string.IsNullOrWhiteSpace(x.Organization.OtherDesignation) ? x.Organization.OtherDesignation : x.Organization.Name,
                    IsPrimary = x.OrganizationMember != null && x.OrganizationMember.IsPrimary,
                    City = x.Organization.City,
                    NPI = x.Organization.NPI,
                    IsPayingClient = !x.Organization.IsFree,
                    StateOrProvince = x.Organization.StateOrProvince,
                    CanSendRequests = x.Organization.OrganizationTypeId != 10004,
                    HasIncompleteProfile = !x.Organization.IsValidForSendingRequests(),
                    MaxUploadBytes = x.MaxUploadBytes
                });

            return Json(result);
        }

        [RequireAuthorizedOrganization]
        [HttpGet]
        [Route("Organization/{organizationId:int}", Name = "SendNew")]
        public async Task<IActionResult> New(Organization organization)
        {
            var model = await InitializeSendModel(true, organization.OrganizationId);
            model.RequireClientHeader = SecurityService.ShowLegacyNavBar(CurrentUser.IsUserSender(), CurrentUser.MemberId);
            return View("Index", model);
        }

        [RequireAuthorizedOrganization]
        [HttpGet]
        [Route("Organization/{organizationId:int}/Patient/{patientId:int}", Name = "SendIndexWithPatient")]
        public async Task<IActionResult> IndexWithPatient(Organization organization, int patientId)
        {
            var patient = await PatientService.GetByIdAsync(patientId, organization.OrganizationId);
            if (patient == null)
            {
                return StatusCode(404);
            }

            var model = await InitializeSendModel();
            model.PatientId = patient.PatientId;
            var mrn = patient.OrganizationKeys.Where(ok => ok.OrganizationId == organization.OrganizationId && !ok.MedicalRecordNumber.IsNullOrWhiteSpace())
                                              .OrderByDescending(ok => ok.CreatedAt)
                                              .ThenByDescending(ok => ok.LastModifiedAt)
                                              .Select(ok => ok.MedicalRecordNumber)
                                              .FirstOrDefault();
            model.PatientAutoComplete = $"{patient.FullName} (DOB: {patient.Birthdate:M/d/yyyy}{(!string.IsNullOrWhiteSpace(mrn) ? $", MRN: {mrn}" : string.Empty)})";
            model.RequireClientHeader =
                SecurityService.ShowLegacyNavBar(CurrentUser.IsUserSender(), CurrentUser.MemberId);

            return View("Index", model);
        }

        [RequireAuthorizedOrganization]
        [HttpPost]
        [Route("Organization/{organizationId:int}/Submit", Name = "SendSubmit")]
        public async Task<IActionResult> Submit(Organization organization, [FromForm] SendModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", await InitializeSendModel(false, organization.OrganizationId, null, model));
            }

            var pdfBytes = Array.Empty<byte>();
            var signer = await SecurityService.GetMemberByIdAsync(model.SignerMemberId.Value);
            var signerOrganization = SecurityService.GetOrganizationMembersByMemberId(model.SignerMemberId.Value)
                                                    .FirstOrDefault(om => om.OrganizationId == model.SignerOrganizationId && om.IsActive)?.Organization;
            var signerSubordinates = model.CollaboratorMemberId.HasValue || model.AssistantMemberId.HasValue ?
                                        SecurityService.GetSubordinatesForMemberId(model.SignerMemberId.Value)
                                                       .Where(mr => (mr.IsActive ?? false) && mr.Subordinate.IsActive)
                                                       .Select(mr => mr.Subordinate)
                                                       .ToArray() : Array.Empty<Member>();
            var patient = await PatientService.GetByIdAsync(model.PatientId.Value, organization.OrganizationId);
            var requestTemplate = await DocumentService.GetTemplateByIdAsync(model.TemplateId.Value);
            var parentTemplate = (await DocumentService.GetActiveTemplatesByOrganizationIdAsync(organization.OrganizationId)).FirstOrDefault(t => t.TemplateId == model.TemplateId.Value) ??
                                    (requestTemplate?.ParentTemplateId > 1 ? await DocumentService.GetTemplateByIdAsync(requestTemplate.ParentTemplateId.Value) : null);
            var isF2F = F2F_TEMPLATE_IDS.Contains(parentTemplate.TemplateId);

            model.PreviewBeforeSending = !isF2F ? model.PreviewBeforeSending : false;
            if (requestTemplate != null && requestTemplate.ParentTemplateId.GetValueOrDefault() <= 1)
            {
                requestTemplate = null;
            }

            if (model.PdfContents != null)
            {
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    await model.PdfContents.CopyToAsync(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }
            }

            if (requestTemplate != null && requestTemplate.OrganizationId != organization.OrganizationId)
            {
                ModelState.AddModelError(string.Empty, "The template you have chosen could not be associated with your request.");
            }
            if (!organization.IsValidForSendingRequests())
            {
                ModelState.AddModelError(string.Empty, "You must complete your company profile before sending.");
            }
            if (signer == null || !signer.CanSign || !signer.IsActive)
            {
                ModelState.AddModelError("SignerMemberId", "The signer you have chosen is not valid.");
            }
            if (organization.IsFree && signer != null && !signer.IsPayingClient)
            {
                ModelState.AddModelError(string.Empty, "The signer you are attempting to send this document to is not a paid subscriber.");
            }
            if (signerOrganization == null || !signerOrganization.IsActive)
            {
                ModelState.AddModelError("SignerOrganizationId", "The location you have chosen is not valid.");
            }
            if (model.CollaboratorMemberId.HasValue && signerSubordinates.FirstOrDefault(m => m.MemberId == model.CollaboratorMemberId.Value) == null)
            {
                ModelState.AddModelError(string.Empty, "The collaborator you have chosen is not valid.");
            }
            if (model.AssistantMemberId.HasValue && signerSubordinates.FirstOrDefault(m => m.MemberId == model.AssistantMemberId.Value) == null)
            {
                ModelState.AddModelError(string.Empty, "The assistant you have chosen is not valid.");
            }

            if (patient == null)
            {
                ModelState.AddModelError("PatientId", "The patient you have chosen is not valid.");
            }

            if (parentTemplate == null)
            {
                ModelState.AddModelError("TemplateId", "The template you have chosen is not valid.");
            }

            if (requestTemplate == null && !F2F_TEMPLATE_IDS.Contains(model.TemplateId.Value))
            {
                if (!pdfBytes.Any())
                {
                    ModelState.AddModelError("PdfContents", "A file upload is required for this template type.");
                }
                if (pdfBytes.Length > ((await SecurityService.GetMaxUploadKilobytesAsync(organization.OrganizationId)) * 1024))
                {
                    ModelState.AddModelError("PdfContents", "The file you have uploaded is too large.");
                }
                if (pdfBytes.Any() && !DocumentService.ValidatePdf(pdfBytes))
                {
                    ModelState.AddModelError(string.Empty, "The file you have uploaded could not be read.");
                }
            }

            if (parentTemplate.TemplateType.RequireDxCode && !model.DiagnosisCodeId.HasValue)
            {
                ModelState.AddModelError("DiagnosisCodeId", "Diagnosis Code is required");
            }

            if (requestTemplate == null && model.CanCheckForDuplicateRequests() &&
                (await RequestService.GetMatchingSubmittedRequestsAsync(organization.OrganizationId, model.PatientId.Value, model.SignerMemberId.Value, model.TemplateId.Value, model.ClinicalDate.Value)) == 2)
            {
                ModelState.AddModelError(string.Empty, "A duplicate document has already been signed or is pending. Resubmitting duplicate documents is not allowed.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", await InitializeSendModel(false, organization.OrganizationId, null, model));
            }

            var requestTemplateId = requestTemplate != null ? requestTemplate.TemplateId : (isF2F ? (int?)parentTemplate.TemplateId : null);
            if (!requestTemplateId.HasValue)
            {
                var storageKey = await StorageService.SaveToBinaryStorageAsync(BinaryStorageType.Templates, pdfBytes);
                requestTemplateId = await DocumentService.CreateRequestTemplateAsync(parentTemplate.TemplateId, organization.OrganizationId, CurrentUser.Id, storageKey);
            }

            if (requestTemplate != null || !model.PreviewBeforeSending)
            {
                await RequestService.CreateRequestAsync(CurrentUser.Id,
                                                        organization.OrganizationId,
                                                        patient.PatientId,
                                                        requestTemplateId.Value,
                                                        signer.MemberId,
                                                        signerOrganization.OrganizationId,
                                                        model.CollaboratorMemberId,
                                                        model.AssistantMemberId,
                                                        model.DiagnosisCodeId,
                                                        parentTemplate.TemplateType.DateAssociation == Documents.ClinicalDate.EffectiveDate ? model.ClinicalDate : null,
                                                        parentTemplate.TemplateType.DateAssociation == Documents.ClinicalDate.StartOfCare ? model.ClinicalDate : null,
                                                        model.TemplateId == requestTemplateId.Value);
            }
            else
            {
                return RedirectToRoute("TemplateAnnotationEdit", new { templateId = requestTemplateId });
            }

            return RedirectToRoute("SendNew", new { organizationId = organization.OrganizationId });
        }

        protected async Task<SendModel> InitializeSendModel(bool overrideClientModel = false, int? organizationId = null, int? templateId = null, SendModel model = null, bool contentOnly = false)
        {
            var isSurrogate = await SecurityService.IsMemberSurrogateSenderAsync(CurrentUser);
            IEnumerable<(OrganizationMember OrganizationMember, int MaxUploadBytes)> organizations = null;

            int defaultMaxUploadKilobytes;
            if (!isSurrogate)
            {
                defaultMaxUploadKilobytes = await SecurityService.GetMaxUploadKilobytesAsync();
                organizations = (await SecurityService.GetOrganizationMembersByMemberId(CurrentUser.Id).Where(om => om.IsActive).ToArrayAsync())
                            .GroupJoin(await SecurityService.GetOrganizationSettings().Where(s => s.Key == "DocumentSizeLimit" && s.IsActive == true && s.ItemInt != null)
                                            .Join(SecurityService.GetOrganizationMembersByMemberId(CurrentUser.Id).Where(om => om.IsActive), s => s.ParentId, om => om.OrganizationId, (s, om) => s)
                                            .ToArrayAsync(),
                                            om => om.OrganizationId,
                                            s => s.ParentId,
                                            (om, s) => (om, (s.FirstOrDefault()?.ItemInt).GetValueOrDefault(defaultMaxUploadKilobytes) * 1024));
            }
            //surrogate senders see autocomplete instead of dropdown, hence there are no initial orgs for them
            //unless we are on a Organization/{organizationId:int} route.
            else if (organizationId.HasValue)
            {
                defaultMaxUploadKilobytes = await SecurityService.GetMaxUploadKilobytesAsync();
                organizations = (await SecurityService.GetOrganizationMembersByOrganizationId(organizationId.Value).Where(om => om.IsActive).ToArrayAsync())
                    .GroupJoin(await SecurityService.GetOrganizationSettings().Where(s => s.Key == "DocumentSizeLimit" && s.IsActive == true && s.ItemInt != null)
                    .ToArrayAsync(),
                    om => om.OrganizationId,
                    s => s.ParentId,
                    (om, s) => (om, (s.FirstOrDefault()?.ItemInt).GetValueOrDefault(defaultMaxUploadKilobytes) * 1024));
            }

            var organization = organizationId.HasValue
                ? organizations?.Where(x => x.OrganizationMember?.OrganizationId == organizationId.Value).Select(x => x.OrganizationMember?.Organization).FirstOrDefault()
                : organizations?.OrderByDescending(x => x.OrganizationMember?.IsPrimary).FirstOrDefault().OrganizationMember?.Organization;

            var template = templateId.HasValue ? await DocumentService.GetTemplateByIdAsync(templateId.Value) : null;

            model ??= new SendModel();

            model.IsSurrogateSender = isSurrogate;
            model.OrganizationId = organization == null ? 0 : organization.OrganizationId;
            model.FromOfficeAutoComplete = organization == null ? "" : $"{organization.Name} ({organization.City}, {organization.StateOrProvince} - NPI: {organization.NPI})";
            model.TemplateId = template?.TemplateId ?? model.TemplateId;
            model.HasRequestTemplate = (template?.ParentTemplateId).GetValueOrDefault() > 1;
            model.CurrentUser = CurrentUser;
            model.OverrideClientModel = overrideClientModel;
            model.PreviewBeforeSending = true;
            model.RequireClientHeader = !contentOnly;
            model.Offices = organizations?.Select(o => new SendModel.Office()
            {
                OrganizationId = o.OrganizationMember.Organization.OrganizationId,
                IsPrimary = o.OrganizationMember.IsPrimary,
                IsPayingClient = !o.OrganizationMember.Organization.IsFree,
                Name = !string.IsNullOrWhiteSpace(o.OrganizationMember.Organization.OtherDesignation) ? o.OrganizationMember.Organization.OtherDesignation : o.OrganizationMember.Organization.Name,
                StateOrProvince = o.OrganizationMember.Organization.StateOrProvince,
                CanSendRequests = o.OrganizationMember.Organization.OrganizationTypeId != 10004,
                HasIncompleteProfile = !o.OrganizationMember.Organization.IsValidForSendingRequests(),
                MaxUploadBytes = o.MaxUploadBytes
            }) ?? Array.Empty<SendModel.Office>();

            return model;
        }

        [RequireAuthorizedOrganization]
        [HttpGet]
        [Route("Template/Organization/{organizationId:int}", Name = "SendOrganizationTemplates")]
        public async Task<JsonResult> OrganizationTemplates(Organization organization)
        {
            Func<Documents.Template, OrganizationTemplatesJsonModel.Template> toTemplateModel = t => new OrganizationTemplatesJsonModel.Template()
            {
                TemplateId = t.TemplateId,
                Summary = t.Name,
                ClinicalDateLabel = t.TemplateType.DateAssociation switch
                {
                    Documents.ClinicalDate.StartOfCare => "Start Of Care",
                    _ => "Effective Date"
                },
                DiagnosisCodeAllowed = t.TemplateType.ShowIcd9,
                DiagnosisCodeRequired = t.TemplateType.RequireDxCode,
                PdfRequired = !F2F_TEMPLATE_IDS.Contains(t.TemplateId)
            };
            var templatesByType = (await DocumentService.GetActiveTemplatesByOrganizationIdAsync(organization.OrganizationId)).GroupBy(t => t.OrganizationId == 0);

            return Json(new OrganizationTemplatesJsonModel()
            {
                StandardTemplateGroupName = "SutureSign Standard Templates",
                OrganizationTemplateGroupName = !string.IsNullOrWhiteSpace(organization.OtherDesignation) ? organization.OtherDesignation : organization.Name,
                StandardTemplates = (templatesByType.FirstOrDefault(grp => grp.Key)?.ToArray() ?? Array.Empty<Documents.Template>()).Select(toTemplateModel).OrderBy(t => t.Summary),
                OrganizationTemplates = (templatesByType.FirstOrDefault(grp => !grp.Key)?.ToArray() ?? Array.Empty<Documents.Template>()).Select(toTemplateModel).OrderBy(t => t.Summary)
            });
        }

        [HttpGet]
        [Route("Signer/{memberId:int}", Name = "SendSignerDetails")]
        public async Task<IActionResult> SignerDetails(int memberId)
        {
            var signer = await SecurityService.GetMemberByIdAsync(memberId);

            if (!signer.CanSign)
            {
                return StatusCode(400);
            }

            var organizations = await SecurityService.GetOrganizationMembersByMemberId(memberId)
                                                     .Where(om => om.IsActive && om.Organization.IsActive)
                                                     .Select(om => om.Organization)
                                                     .ToArrayAsync();

            return Json(new SignerDetailsJsonModel()
            {
                MemberId = memberId,
                Locations = organizations.Select(o => new SignerDetailsJsonModel.Location()
                {
                    OrganizationId = o.OrganizationId,
                    IsPayingClient = !o.IsFree,
                    Summary = $"{o.Name}, {o.StateOrProvince}"
                })
            });
        }

        [HttpGet]
        [Route("Signer/{memberId:int}/Location/{organizationId:int}", Name = "SendSignerLocationDetails")]
        public async Task<IActionResult> SignerLocationDetails(int memberId, int organizationId)
        {
            var signer = await SecurityService.GetMemberByIdAsync(memberId);

            if (!signer.CanSign)
            {
                return StatusCode(400);
            }

            var subordinates = await SecurityService.GetSubordinatesForMemberId(memberId)
                                                    .Where(mr => mr.IsActive == true && mr.Subordinate.IsActive)
                                                    .Select(mr => mr.Subordinate)
                                                    .ToArrayAsync();
            var organizationMembers = await SecurityService.GetOrganizationMembersByMemberId(subordinates.Select(m => m.MemberId).ToArray())
                                                           .Where(om => om.OrganizationId == organizationId)
                                                           .ToArrayAsync();
            var subordinatesAtLocation = subordinates.Join(organizationMembers, s => s.MemberId, om => om.MemberId, (s, om) => s);

            return Json(new SignerLocationDetailsJsonModel()
            {
                MemberId = memberId,
                OrganizationId = organizationId,
                Assistants = subordinatesAtLocation.Where(m => !m.IsCollaborator).Select(m => new SignerLocationDetailsJsonModel.Subordinate()
                {
                    MemberId = m.MemberId,
                    Summary = $"{m.FirstName} {m.LastName}{(!string.IsNullOrWhiteSpace(m.ProfessionalSuffix) ? $" {m.ProfessionalSuffix}" : string.Empty)}"
                }),
                Collaborators = subordinatesAtLocation.Where(m => m.IsCollaborator).Select(m => new SignerLocationDetailsJsonModel.Subordinate()
                {
                    MemberId = m.MemberId,
                    Summary = $"{m.FirstName} {m.LastName}{(!string.IsNullOrWhiteSpace(m.ProfessionalSuffix) ? $" {m.ProfessionalSuffix}" : string.Empty)}"
                })
            });
        }

        [RequireAuthorizedOrganization]
        [HttpGet]
        [Route("Organization/{organizationId:int}/Patient/{patientId:int}/Update", Name = "SendUpdatePatientModal")]
        public async Task<IActionResult> UpdatePatientModal(Organization organization, int patientId)
        {
            var patient = await PatientService.GetByIdAsync(patientId, organization.OrganizationId);
            var ssnId = patient?.Identifiers.Where(id => id.Type == KnownTypes.SocialSecurityNumber || id.Type == KnownTypes.SocialSecuritySerial)
                                            .OrderBy(id => id.Type != KnownTypes.SocialSecurityNumber).FirstOrDefault();

            if (patient == null)
            {
                return NotFound();
            }

            return PartialView("_UpdatePatient", new UpdatePatientModel()
            {
                Name = $"{patient.FirstName} {patient.LastName}",
                DateOfBirth = patient.Birthdate,
                Gender = patient.Gender switch { Gender.Male or Gender.Female => patient.Gender, _ => null },
                SocialSecurityNumber = ssnId?.Value,
                SocialSecurityNumberType = ssnId?.Type != KnownTypes.SocialSecurityNumber ? UpdatePatientModel.SocialSecurityNumberStyle.Last4 : UpdatePatientModel.SocialSecurityNumberStyle.Full,
                CanEditMedicareMbi = patient.Identifiers.Any(id => id.Type == KnownTypes.Medicare || id.Type == KnownTypes.MedicareBeneficiaryNumber || id.Type == KnownTypes.MedicareAdvantage),
                MedicareMbi = patient.Identifiers.FirstOrDefault(id => id.Type == KnownTypes.MedicareBeneficiaryNumber)?.Value
            });
        }

        [RequireAuthorizedOrganization]
        [HttpPost]
        [Route("Organization/{organizationId:int}/Patient/{patientId:int}/Update", Name = "SendUpdatePatient")]
        public async Task<IActionResult> UpdatePatient(Organization organization, int patientId, [FromBody] UpdatePatientModel model)
        {
            var patient = await PatientService.GetByIdAsync(patientId, organization.OrganizationId);
            var ssnId = patient?.Identifiers.Where(id => id.Type == KnownTypes.SocialSecurityNumber || id.Type == KnownTypes.SocialSecuritySerial)
                                            .OrderBy(id => id.Type != KnownTypes.SocialSecurityNumber).FirstOrDefault();
            bool ssnUpdateEligible = ssnId == null,
                 genderUpdateEligible = patient != null && (patient.Gender == Gender.Ambiguous || patient.Gender == Gender.Unknown),
                 mbiUpdateEligible = patient != null && patient.Identifiers.Any(id => id.Type == KnownTypes.Medicare || id.Type == KnownTypes.MedicareBeneficiaryNumber || id.Type == KnownTypes.MedicareAdvantage) &&
                                        !patient.Identifiers.Any(id => id.Type == KnownTypes.MedicareBeneficiaryNumber && !string.IsNullOrWhiteSpace(id.Value));
            var errors = new List<string>();
            var duplicate = false;

            if (patient == null)
            {
                return BadRequest();
            }

            if (genderUpdateEligible && !model.HasGender)
            {
                errors.Add("Gender is required.");
            }

            if (ssnUpdateEligible && !model.HasSocialSecurityNumberType)
            {
                errors.Add("SSN type is required.");
            }

            if (ssnUpdateEligible)
            {
                if (!model.HasSocialSecurityNumber)
                {
                    errors.Add("SSN is required.");
                }

                if (model.SocialSecurityNumberType == UpdatePatientModel.SocialSecurityNumberStyle.Full && !Regex.IsMatch(model.SocialSecurityNumber, @"^\d{9}$"))
                {
                    errors.Add("SSN is invalid.");
                }
                else
                {
                    var existing = PatientService.GetByIdentifier(KnownTypes.SocialSecurityNumber, model.SocialSecurityNumber);
                    if (existing.Any(p => p.PatientId != patientId))
                    {
                        duplicate = true;
                        errors.Add("This appears to be a duplicate patient. Click &quot;Don't Have The Information&quot; and then contact <a href=\"mailto:support@suturehealth.com\">support</a> so we can make any appropriate changes.");
                    }
                }

                if (model.SocialSecurityNumberType == UpdatePatientModel.SocialSecurityNumberStyle.Last4 && !Regex.IsMatch(model.SocialSecurityNumber, @"^\d{4}$"))
                {
                    errors.Add("SSN is invalid.");
                }
            }

            if (mbiUpdateEligible)
            {
                if (!string.IsNullOrWhiteSpace(model.MedicareMbi) && !Regex.IsMatch(model.MedicareMbi, @"^[1-9]{1}[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz]{1}[0-9]{1}-?[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz]{1}[0-9]{1}-?[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz|^0-9]{1}[0-9]{1}[0-9]{1}$"))
                {
                    errors.Add("Medicare MBI is invalid.");
                }
                else if (!duplicate)
                {
                    var existing = PatientService.GetByIdentifier(KnownTypes.MedicareBeneficiaryNumber, model.MedicareMbi);
                    if (existing.Any(p => p.PatientId != patientId))
                    {
                        errors.Add("This appears to be a duplicate patient. Click &quot;Don't Have The Information&quot; and then contact <a href=\"mailto:support@suturehealth.com\">support</a> so we can make any appropriate changes.");
                    }
                }
            }

            if (!errors.Any())
            {
                if (ssnUpdateEligible && model.HasSocialSecurityNumber)
                {
                    patient.Identifiers = patient.Identifiers.Where(id => !(id.Type == KnownTypes.SocialSecurityNumber || id.Type == KnownTypes.SocialSecuritySerial))
                                                             .Union(new Patients.PatientIdentifier[]
                                                             {
                                                                 new Patients.PatientIdentifier()
                                                                 {
                                                                     ParentId = patient.PatientId,
                                                                     Type = model.SocialSecurityNumberType == UpdatePatientModel.SocialSecurityNumberStyle.Full ? KnownTypes.SocialSecurityNumber : KnownTypes.SocialSecuritySerial,
                                                                     Value = model.SocialSecurityNumber
                                                                 }
                                                             })
                                                             .ToList();
                }
                if (genderUpdateEligible && model.HasGender)
                {
                    patient.Gender = model.Gender.Value;
                }
                if (mbiUpdateEligible && model.HasMedicareMbi)
                {
                    patient.Identifiers = patient.Identifiers.Where(id => id.Type != KnownTypes.MedicareBeneficiaryNumber)
                                                             .Union(new Patients.PatientIdentifier[]
                                                             {
                                                                 new Patients.PatientIdentifier()
                                                                 {
                                                                     ParentId = patient.PatientId,
                                                                     Type = KnownTypes.MedicareBeneficiaryNumber,
                                                                     Value = model.MedicareMbi
                                                                 }
                                                             })
                                                             .ToList();
                }

                await PatientService.UpdateAsync(patient, organization.OrganizationId, CurrentUser.Id);
            }

            return Json(new UpdatePatientResponse()
            {
                Errors = errors
            });
        }

        [RequireAuthorizedOrganization]
        [HttpPost]
        [Route("Organization/{organizationId:int}/PreSubmitVerification", Name = "SendPreSubmitVerification")]
        public async Task<IActionResult> PreSubmitVerification(Organization organization, [FromBody] PreSubmitVerificationPostModel model)
        {
            var duplicateRiskLevel = PreSubmitVerificationJsonModel.DuplicateRiskLevel.Indeterminate;
            var patient = model.PatientId.HasValue ? await PatientService.GetByIdAsync(model.PatientId.Value, organization.OrganizationId) : null;

            if (model.CanCheckForDuplicateRequests())
            {
                duplicateRiskLevel = await RequestService.GetMatchingSubmittedRequestsAsync(organization.OrganizationId, model.PatientId.Value, model.SignerMemberId.Value, model.TemplateId.Value, model.ClinicalDate.Value) switch
                {
                    0 => PreSubmitVerificationJsonModel.DuplicateRiskLevel.None,
                    1 => PreSubmitVerificationJsonModel.DuplicateRiskLevel.Warning,
                    2 => PreSubmitVerificationJsonModel.DuplicateRiskLevel.Error,
                    _ => PreSubmitVerificationJsonModel.DuplicateRiskLevel.Indeterminate
                };
            }

            return Json(new PreSubmitVerificationJsonModel()
            {
                DuplicateRequestRiskLevel = duplicateRiskLevel,
                PatientUpdateRequested = patient != null && PatientInformationMissing(patient)
            });
        }

        [RequireAuthorizedOrganization]
        [HttpPost]
        [Route("Request/Retry", Name = "RetryFailedFaxDocuments")]
        public async Task RetryFailedFaxDocuments()
        {
           
        }

        protected bool PatientInformationMissing(SutureHealth.Patients.Patient patient)
        {
            var hasMedicareOrMedicareAdvantage = patient.Identifiers.Any(id => id.Type == KnownTypes.Medicare || id.Type == KnownTypes.MedicareBeneficiaryNumber || id.Type == KnownTypes.MedicareAdvantage);

            if (patient.Gender == Gender.Ambiguous || patient.Gender == Gender.Unknown)
            {
                return true;
            }

            if (hasMedicareOrMedicareAdvantage && string.IsNullOrWhiteSpace(patient.Identifiers.FirstOrDefault(id => id.Type == KnownTypes.MedicareBeneficiaryNumber)?.Value))
            {
                return true;
            }

            if (!patient.Identifiers.Any(id => id.Type == KnownTypes.SocialSecurityNumber || id.Type == KnownTypes.SocialSecuritySerial))
            {
                return true;
            }

            return false;
        }
    }
}