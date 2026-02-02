using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SutureHealth.AspNetCore.Models;
using SutureHealth.AspNetCore.Mvc.Extensions;
using SutureHealth.Requests;
using SutureHealth.Requests.Services;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.Patients.Services;
using SutureHealth.Patients;

namespace SutureHealth.AspNetCore.Pages
{
    public enum RequestStatus : short
    {
        Pending = 0,
        Signed = 1,
        Rejected = 2,
        Retracted = 3,
    }

    public class History : BasePageModel
    {
        private readonly int[] F2F_TEMPLATE_IDS = new int[] { 1000, 1001 };

        protected ILogger Logger { get; init; }
        protected IRequestServicesProvider RequestServices { get; init; }
        protected IApplicationService ApplicationService { get; init; }
        protected IServiceProvider ServiceProvider { get; init; }
        protected IPatientServicesProvider PatientService { get; init; }

        public History
        (
            ILogger<History> logger,
            IRequestServicesProvider requestServices,
            IApplicationService applicationService,
            IServiceProvider serviceProvider,
            IPatientServicesProvider patientService
        )
        {
            Logger = logger;
            RequestServices = requestServices;
            ApplicationService = applicationService;
            ServiceProvider = serviceProvider;
            PatientService = patientService;
        }

        public Filter<bool?> ArchivedFilter { get; set; }
        public Filter<bool?> FilingFilter { get; set; }
        public Filter<int?> LocationFilter { get; set; }
        public Filter<int?> SignerFilter { get; set; }
        public Filter<short?> StatusFilter { get; set; }

        public async Task OnGet(CancellationToken cancellationToken, bool contentOnly = false)
        {
            RequireClientHeader = !contentOnly;
            var member = await ApplicationService.GetMemberByIdAsync(CurrentUser.Id);

            async Task<Filter<int?>> InitializeLocationFilter()
            {
                var items = (await ApplicationService.GetOrganizationMembersByMemberId(member.MemberId)
                                                     .Where(om => om.IsActive && om.Organization.IsActive)
                                                     .Select(om => om.Organization)
                                                     .Where(o => !CurrentUser.IsUserSender() || (o.OrganizationTypeId == null || o.OrganizationTypeId != 10004))
                                                     .ToArrayAsync(cancellationToken))
                                                     .Select(o => new SelectListItem(CurrentUser.IsUserSender() ? (!string.IsNullOrWhiteSpace(o.OtherDesignation) ? o.OtherDesignation : o.Name)
                                                                                                                : $"{(!string.IsNullOrWhiteSpace(o.OtherDesignation) ? o.OtherDesignation : o.Name)}, {o.StateOrProvince}",
                                                                                     o.OrganizationId.ToString(),
                                                                                     o.OrganizationId == CurrentUser.PrimaryOrganizationId))
                                                    .OrderBy(item => item.Text)
                                                    .ToList();
                return new Filter<int?>
                {
                    DefaultValue = CurrentUser.PrimaryOrganizationId,
                    Enabled = items.Count > 1,
                    Items = items,
                    ValueConversion = s => int.TryParse(s, out int id) ? id : null
                };
            }

            async Task<Filter<int?>> InitializeSignerFilter()
            {
                IList<SelectListItem> items;

                if (CurrentUser.IsUserSigningMember())
                {
                    if (CurrentUser.IsUserSigner())
                    {
                        items = (await member.Subordinates(ApplicationService)
                                             .Where(s => (s.IsActive ?? false) && s.Subordinate.CanSign)
                                             .Select(s => s.Subordinate)
                                             .ToArrayAsync(cancellationToken))
                                             .Select(member => new SelectListItem(member.ShortName, member.MemberId.ToString()))
                                             .Union(new[] { new SelectListItem(member.ShortName, member.MemberId.ToString(), true) })
                                             .OrderBy(item => item.Selected)
                                             .ThenBy(item => item.Text)
                                             .ToList();
                    }
                    else
                    {
                        var relatedMembers = (await member.Supervisors(ApplicationService)
                                                          .Where(r => (r.IsActive ?? false) && r.Supervisor.CanSign)
                                                          .Select(r => r.Supervisor)
                                                          .ToArrayAsync(cancellationToken))
                                                   .Union(await member.Subordinates(ApplicationService)
                                                                      .Where(r => (r.IsActive ?? false) && r.Subordinate.CanSign)
                                                                      .Select(r => r.Subordinate)
                                                                      .ToArrayAsync(cancellationToken))
                                                   .Union(CurrentUser.CanSign ? new[] { member } : Array.Empty<Member>())
                                                   .OrderBy(m => m.ShortName)
                                                   .Distinct()
                                                   .ToList();
                        items = relatedMembers.Select((m, i) => new SelectListItem(m.ShortName, m.MemberId.ToString(), member.CanSign ? m.MemberId == member.MemberId : i == 0)).ToList();
                    }
                }
                else
                {
                    items = Array.Empty<SelectListItem>();
                }

                return new Filter<int?>
                {
                    Enabled = items.Count() > 1,
                    Items = items,
                    ValueConversion = s => int.TryParse(s, out int id) ? id : null
                };
            }

            ArchivedFilter = new Filter<bool?>
            {
                Enabled = !CurrentUser.IsUserSigningMember(),
                Items = new[]
                {
                    new SelectListItem("Sent", "false", true),
                    new SelectListItem("Archive", "true")
                },
                ValueConversion = (s) => s switch
                {
                    "false" => false,
                    "true" => true,
                    _ => null
                }
            };
            FilingFilter = new Filter<bool?>
            {
                Items = new[]
                {
                    new SelectListItem("Not Filed", "false"),
                    new SelectListItem("Filed", "true")
                },
                ValueConversion = (s) => bool.TryParse(s, out var value) ? value : null
            };
            LocationFilter = await InitializeLocationFilter();
            SignerFilter = await InitializeSignerFilter();
            StatusFilter = new Filter<short?>
            {
                Items = CurrentUser.IsUserPhysician() ? new[] {
                    new SelectListItem("Signed", "1", CurrentUser.IsUserSigningMember()),
                    new SelectListItem("Rejected", "2"),
                    new SelectListItem("Pending", "0")
                } : new[] {
                    new SelectListItem("Signed", "1", CurrentUser.IsUserSigningMember()),
                    new SelectListItem("Rejected", "2"),
                    new SelectListItem("Pending", "0"),
                    new SelectListItem("Retracted", "3")
                },
                ValueConversion = s => s switch
                {
                    "0" => null,
                    _ => short.TryParse(s, out short id) ? id : null
                }
            };
        }

        public async Task<IActionResult> OnPostMrnFilter([DataSourceRequest] DataSourceRequest request)
        {
            var result = Array.Empty<MrnSearchItem>() as IEnumerable<MrnSearchItem>;

            if (CurrentUser.IsUserSender() && request.Filters.Contains("MedicalRecordNumber"))
            {
                var searchFilter = request.Filters.GetFilterDescriptor("MedicalRecordNumber");
                var search = searchFilter.Value.ToString();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var organizationIds = await PatientService.GetOrganizationIdsInPatientScopeForSenderAsync(CurrentUser.MemberId);

                    result = (await PatientService.QueryPatientsForSenderByMemberId(CurrentUser.MemberId)
                                                  .Where(p => p.OrganizationKeys.Any(pok => EF.Functions.Like(pok.MedicalRecordNumber, $"%{search}%")))
                                                  .Include(p => p.OrganizationKeys)
                                                  .Take(10)
                                                  .ToArrayAsync())
                             .Select(p => new { p.PatientId, MedicalRecordNumbers = p.OrganizationKeys.Join(organizationIds, pok => pok.OrganizationId, oid => oid, (pok, oid) => pok)
                                                                                                      .Where(pok => !string.IsNullOrWhiteSpace(pok.MedicalRecordNumber)).Select(pok => pok.MedicalRecordNumber).Distinct() })
                             .SelectMany(p => p.MedicalRecordNumbers, (p, mrn) => new { p.PatientId, MedicalRecordNumber = mrn })
                             .Where(pok => pok.MedicalRecordNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                             .Select(pok => new MrnSearchItem()
                             {
                                 PatientId = pok.PatientId.ToString(),
                                 MedicalRecordNumber = pok.MedicalRecordNumber
                             });
                }
            }

            return new JsonResult(new DataSourceResult()
            {
                Total = result.Count(),
                Data = result
            });
        }

        public async Task<IActionResult> OnPostPatientFilter([DataSourceRequest] DataSourceRequest request)
        {
            var result = Array.Empty<KeyValuePair<int, string>>() as IEnumerable<KeyValuePair<int, string>>;

            if (request.Filters.Contains("SearchSummary"))
            {
                var searchFilter = request.Filters.GetFilterDescriptor("SearchSummary");
                var search = searchFilter.Value.ToString();

                if (CurrentUser.IsUserSender())
                {
                    var organizationIds = await PatientService.GetOrganizationIdsInPatientScopeForSenderAsync(CurrentUser.MemberId);
                    var query = PatientService.QueryPatientsForSenderByMemberId(CurrentUser.MemberId)
                                              .Include(p => p.OrganizationKeys) as IQueryable<Patients.Patient>;
                    var words = search.Split(' ').Select(w => Regex.Replace(w, @"[^A-Za-z0-9]+", string.Empty));

                    foreach (var word in words)
                    {
                        query = query.Where(p => EF.Functions.Like(p.LastName, $"%{word}%") || EF.Functions.Like(p.FirstName, $"%{word}%"));
                    }

                    result = (await query.OrderBy(p => p.LastName)
                                         .ThenBy(p => p.FirstName)
                                         .ThenBy(p => p.Suffix)
                                         .Take(10)
                                         .ToArrayAsync())
                             .SelectMany(p => p.OrganizationKeys.Join(organizationIds, pok => pok.OrganizationId, oid => oid, (pok, oid) => pok)
                                                                .Where(pok => !string.IsNullOrWhiteSpace(pok.MedicalRecordNumber))
                                                                .GroupBy(pok => pok.MedicalRecordNumber, (key, grp) => grp.First())
                                                                .DefaultIfEmpty(p.OrganizationKeys.FirstOrDefault()), (p, pok) => new KeyValuePair<int, string>(p.PatientId, p.GetSearchSummary(pok?.OrganizationId)));
                }
                else
                {
                    var query = CurrentUser.IsUserPhysician() ?
                                    PatientService.SearchPatientsForSigner(CurrentUser.Id, search) :
                                    PatientService.SearchPatientsForAssistant(CurrentUser.Id, search);

                    result = (await query.ToArrayAsync()).Select(p => new KeyValuePair<int, string>(p.PatientId, p.GetSearchSummary()));
                }

                request.Filters.Remove("PatientSearchSummary");
            }
            else
            {
                return new JsonResult(new DataSourceResult()
                {
                    Total = 0,
                    Data = Array.Empty<PatientSearchItem>()
                });
            }

            return new JsonResult(new DataSourceResult()
            {
                Total = result.Count(),
                Data = result.Select(r => new PatientSearchItem()
                {
                    PatientId = r.Key.ToString(),
                    SearchSummary = r.Value
                })
            });
        }

        public async Task<IActionResult> OnPost([DataSourceRequest] DataSourceRequest request, CancellationToken cancellationToken)
        {
            // filters
            request.Filters.Transform("Form", (CurrentUser.IsUserSender()) ? "TemplateType" : "TemplateDisplayName");
            request.Filters.Transform("FormId", "SutureSignRequestId");
            request.Filters.Transform("RequestStatus", "RequestStatus", (fd) =>
            {
                if (string.Equals(fd.ConvertedValue, ((short)RequestStatus.Pending).ToString())) fd.Value = null;
            });
            request.Filters.Transform("Practice", "SignerOrganization.Name");
            request.Filters.Transform("SubmitterOrganizationName", "SubmitterOrganization.Name");
            request.Filters.Transform("SignerName", "Signer.LastName");
            request.Filters.Transform("SignerLastName", "Signer.LastName");

            // sorts
            request.Sorts.Transform("Form", (s) => s.Member = CurrentUser.IsUserSender() ? "TemplateType" : "TemplateDisplayName");
            request.Sorts.Transform("FormId", (s) => s.Member = "SutureSignRequestId");
            request.Sorts.Transform("LastModified", (s) => s.Member = "LastModifiedAt");
            request.Sorts.Transform("Practice", (s) => s.Member = "SignerOrganization.Name");
            request.Sorts.Transform("PatientBirthdate", (s) => s.Member = "Patient.Birthdate");
            request.Sorts.Transform("SignerFiledString", (s) => s.Member = "SignerHasFiled");
            request.Sorts.Transform("Status", (s) => s.Member = "RequestStatus");
            request.Sorts.Transform("SubmitterFiledString", (s) => s.Member = "SubmitterHasFiled");
            request.Sorts.Transform("ApprovalStatus", (s) => s.Member = "IsApproved");

            if (request.Sorts.FirstOrDefault(s => s.Member == "PatientName") is SortDescriptor patientSort)
            {
                patientSort.Member = "Patient.LastName";
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Patient.FirstName",
                    SortDirection = patientSort.SortDirection
                });
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Patient.Suffix",
                    SortDirection = patientSort.SortDirection
                });
            }

            if (request.Sorts.FirstOrDefault(s => s.Member == "SignerName") is SortDescriptor signerSort)
            {
                signerSort.Member = "Signer.LastName";
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Signer.FirstName",
                    SortDirection = signerSort.SortDirection
                });
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Signer.Suffix",
                    SortDirection = signerSort.SortDirection
                });
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Signer.ProfessionalSuffix",
                    SortDirection = signerSort.SortDirection
                });
            }

            if (request.Sorts.FirstOrDefault(s => s.Member == "CollaboratorName") is SortDescriptor collaboratorSort)
            {
                collaboratorSort.Member = "Collaborator.LastName";
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Collaborator.FirstName",
                    SortDirection = collaboratorSort.SortDirection
                });
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Collaborator.Suffix",
                    SortDirection = collaboratorSort.SortDirection
                });
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = "Collaborator.ProfessionalSuffix",
                    SortDirection = collaboratorSort.SortDirection
                });
            }

            if (request.Sorts.FirstOrDefault(s => s.Member == "Office") is SortDescriptor officeSort)
            {
                officeSort.Member = $"{(CurrentUser.IsUserSender() ? "Submitter" : "Signer")}Organization.OtherDesignation";
                request.Sorts.Add(new SortDescriptor()
                {
                    Member = $"{(CurrentUser.IsUserSender() ? "Submitter" : "Signer")}Organization.Name",
                    SortDirection = officeSort.SortDirection
                });
            }

            var query = RequestServices.GetHistoricalRequests(CurrentUser.Id, CurrentUser.IsUserSender(), CurrentUser.IsUserSigner(), CurrentUser.CanSign);
            if (request.Filters.Contains("Signer.LastName"))
            {
                var nameFilter = request.Filters.GetFilterDescriptor("Signer.LastName").Value.ToString();
                request.Filters.Remove("Signer.LastName");
                query = query.Where(q => q.Signer.LastName.Contains(nameFilter) || q.Signer.FirstName.Contains(nameFilter));
            }

            if (request.Filters.Contains("SubmittedAtOneYear"))
            {
                request.Filters.Remove("SubmittedAtOneYear");
                if (!request.Filters.Contains("SubmittedAt"))
                    query = query.Where(q => q.SubmittedAt >= DateTime.UtcNow.AddYears(-1).Date);
            }

            if (request.Filters.Contains("SubmitterHasArchived") && CurrentUser.IsUserSender())
            {
                var filter = request.Filters.GetFilterDescriptor("SubmitterHasArchived");
                var archiveFilter = filter != null && bool.TryParse(filter.Value.ToString(), out var parsedFilterValue) ? parsedFilterValue : (bool?)null;
                switch (archiveFilter.HasValue, archiveFilter)
                {
                    case (true, true):
                        short pending = (short)RequestStatus.Pending;
                        query = query.Where(sr => (sr.RequestStatus ?? 0) != pending);
                        break;
                    case (true, false):
                        short retracted = (short)RequestStatus.Retracted;
                        query = query.Where(sr => sr.RequestStatus != retracted);
                        break;
                    case (_, _):
                        break;
                }
            }

            if (request.Filters.Contains("PatientName"))
            {
                var searchFilter = request.Filters.GetFilterDescriptor("PatientName");
                
                if (int.TryParse(searchFilter.Value.ToString(), out var patientId))
                {
                    query = query.Where(q => q.PatientId == patientId);
                }

                request.Filters.Remove("PatientName");
            }

            if (request.Filters.Contains("CollaboratorName"))
            {
                var summaryPattern = new Regex(@"^(?<LastName>[^,]+), (?<FirstName>[^ ]+)");
                var searchFilter = request.Filters.GetFilterDescriptor("CollaboratorName");
                var search = searchFilter.Value.ToString();
                var searchMatch = summaryPattern.Match(search);

                request.Filters.Remove("CollaboratorName");

                if (searchMatch.Success)
                {
                    string lastName = searchMatch.Groups["LastName"].Value,
                           firstName = searchMatch.Groups["FirstName"].Value;

                    if (!string.IsNullOrWhiteSpace(lastName))
                    {
                        query = query.Where(q => q.Collaborator.LastName == lastName);
                    }
                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        query = query.Where(q => q.Collaborator.FirstName == firstName);
                    }
                }
                else
                {
                    query = query.Where(q => EF.Functions.Like(q.Collaborator.LastName, $"%{search}%") || EF.Functions.Like(q.Collaborator.FirstName, $"%{search}%"));
                }
            }

            static string FormatRejectionReason(HistoricalRequest request)
            {
                var rejectionReason = new System.Text.StringBuilder();
                if (request.RejectionMemberId.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(request.RejectionReason))
                        rejectionReason.Append($"{Regex.Match(request.RejectionReason, "(?<=:\\s)[^|]*").Value} ");

                    var rejectedBy = request.Rejecter.SigningName;
                    var phoneNumber = request.SignerOrganization.Contacts?.Where(c => c.IsActive && c.Type == ContactType.Phone)
                                                                          .OrderBy(c => c.IsPrimary)
                                                                          .FirstOrDefault()?
                                                                          .Value ?? "phone unavailable";
                    rejectionReason.AppendFormat("(Rejected by " + rejectedBy + ((phoneNumber == "phone unavailable") ? " - " + phoneNumber : " at " + phoneNumber.ToFormattedPhoneNumber()) + ")");
                }

                return rejectionReason.ToString();
            }

            if (!request.Sorts.Any())
            {
                if (CurrentUser.IsUserSender())
                {
                    var filter = request.Filters.GetFilterDescriptor("SubmitterHasArchived");
                    var archiveFilter = filter != null && bool.TryParse(filter.Value.ToString(), out var parsedFilterValue) ? parsedFilterValue : (bool?)null;
                    switch (archiveFilter)
                    {
                        case true:
                            request.Sorts.Add(new SortDescriptor("RequestStatusDate", ListSortDirection.Descending));
                            break;
                        case false:
                            query = query.OrderBy(r => r.RequestStatus != 2)
                                         .ThenBy(r => r.RequestStatus != 1)
                                         .ThenBy(r => r.RequestStatus != null)
                                         .ThenBy(r => r.RequestStatus != 4);
                            break;
                        default:
                            request.Sorts.Add(new SortDescriptor("EffectiveDate", ListSortDirection.Descending));
                            break;
                    }
                }
                else
                {
                    request.Sorts.Add(new SortDescriptor("RequestStatusDate", ListSortDirection.Descending));
                }
            }

            var requests = await query.Where(request.Filters)
                                      .Sort(request.Sorts)
                                      .Page(request.Page - 1, request.PageSize)
                                      .Cast<HistoricalRequest>()
                                      .ToArrayAsync(cancellationToken);
            var data = requests.Select(a =>
            {
                var hasPdfOutcome = !string.IsNullOrWhiteSpace(a.OutcomeStorageKey);
                return new RequestHistoryItem
                {
                    Age = a.Age,
                    CollaboratorName = a.Collaborator?.ShortName ?? string.Empty,
                    EffectiveDate = a.EffectiveDate,
                    FormId = a.SutureSignRequestId,
                    Form = (CurrentUser.IsUserSender()) ? a.TemplateType : a.TemplateDisplayName,
                    LastModified = a.LastModifiedAt.HasValue ? $"Modified on {a.LastModifiedAt.Value.ToString("d")} by {a.Modifier.SigningName}" : null,
                    MedicalRecordNumber = a.MedicalRecordNumber,
                    PatientName = Person.GetFullName(a.Patient.LastName, a.Patient.FirstName, a.Patient.Suffix),
                    PatientBirthdate = a.Patient.Birthdate,
                    PdfUrl = !F2F_TEMPLATE_IDS.Contains(a.TemplateId) || hasPdfOutcome ? Url.PageLink("ViewPdf", values: new { area = "Request", requestId = a.SutureSignRequestId }) : null,
                    RejectionReason = FormatRejectionReason(a),
                    Office = CurrentUser.IsUserSender() ?
                            (!string.IsNullOrWhiteSpace(a.SubmitterOrganization.OtherDesignation) ? a.SubmitterOrganization.OtherDesignation : a.SubmitterOrganization.Name) :
                            (!string.IsNullOrWhiteSpace(a.SignerOrganization.OtherDesignation) ? a.SignerOrganization.OtherDesignation : a.SignerOrganization.Name),
                    SignerName = a.Signer.ShortName,
                    SignerLastName = a.Signer.LastName,
                    SignerMemberId = a.SignerMemberId,
                    SignerOrganizationId = a.SignerOrganizationId,
                    SignerFiledString = a.SignerHasFiled ? "Filed" : "Not Filed",
                    SignerHasFiled = a.SignerHasFiled,
                    ApprovalStatus = a.IsApproved ? "Approved" : "Unapproved",
                    Practice = a.SignerOrganization.Name,
                    RequestStatus = a.RequestStatus,
                    Status = a.RequestStatus switch
                    {
                        1 => "Signed",
                        2 => "Rejected",
                        3 => "Retracted",
                        _ => "Pending"
                    },
                    RequestStatusDate = a.RequestStatusDate,
                    SubmittedAt = a.SubmittedAt,
                    SubmitterHasArchived = a.SubmitterHasArchived,
                    SubmitterHasFiled = a.SubmitterHasFiled,
                    SubmitterFiledString = a.SubmitterHasFiled ? "Filed" : "Not Filed",
                    SubmitterOrganizationId = a.SubmitterOrganizationId,
                    SubmitterOrganizationName = a.SubmitterOrganization.Name,
                    RowTooltip = (F2F_TEMPLATE_IDS.Contains(a.TemplateId) && !hasPdfOutcome) ?
                                $"Document not Viewable or Downloadable - DocID: {a.SutureSignRequestId}" :
                                $"Click to View / Download PDF - DocID: {a.SutureSignRequestId}",
                    FaxStatus = a.FaxStatus switch
                    {
                        1 => "Pending",
                        2 => "Successful",
                        3 => "Retrying",
                        4 => "Unsuccessful",
                        5 => "Failed",
                        _ => "N/A"
                    },
                    ManualRetry = a.ManualRetry
                };
            });

            var result = new DataSourceResult
            {
                Data = data,
                Total = await query.Where(request.Filters)
                                   .Cast<HistoricalRequest>()
                                   .CountAsync(cancellationToken)
            };

            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostArchiveItems(int[] requestIds)
        {
            try
            {
                await RequestServices.MarkRequestArchivedAsync(CurrentUser.Id, requestIds);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    errorMessage = ex.Message
                });
            }
        }

        public async Task<IActionResult> OnPostFileItems(int[] requestIds)
        {
            try
            {
                await RequestServices.MarkRequestArchivedAsync(CurrentUser.Id, requestIds);
                await RequestServices.MarkRequestFiledAsync(CurrentUser.Id, requestIds);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    errorMessage = ex.Message
                });
            }
        }

        public async Task<IActionResult> OnPostItemHistory(int requestId)
        {
            try
            {
                var detail = RequestServices.GetRequestTasks(CurrentUser.Id, requestId).ToArray();
                if (detail != null)
                    return new JsonResult(detail.OrderBy(t => t.TaskDate));
                else
                    return new JsonResult(new object());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    errorMessage = ex.Message
                });
            }
        }

        public async Task<JsonResult> OnPostPhysicianRead([DataSourceRequest] DataSourceRequest request, CancellationToken cancellationToken)
        {
            var query = ApplicationService.GetMembersByOrganizationId(CurrentUser.PrimaryOrganizationId)
                                          .Where(x => x.MemberType == MemberType.Physician);
            return new JsonResult(await query.ToDataSourceResultAsync(request, a => new Member
            {
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                LastName = a.LastName,
                NPI = a.NPI,
                MemberId = a.MemberId,
                ProfessionalSuffix = a.ProfessionalSuffix,
                SigningName = a.SigningName,
                UserName = a.UserName,
                IsActive = a.IsActive,
                Suffix = a.Suffix
            }, cancellationToken));
        }

        public IActionResult OnPostRequestStatusItems(bool? archivedFilterValue = null)
        {
            IList<SelectListItem> items = new List<SelectListItem>();
            if (CurrentUser.IsUserSigningMember())
            {
                items.AddRange(new[] {
                    new SelectListItem("Signed", "1", CurrentUser.IsUserSigningMember()),
                    new SelectListItem("Rejected", "2"),
                    new SelectListItem("Pending", "0")
                });
            }
            else
            {
                items.Add(new SelectListItem("Signed", "1", CurrentUser.IsUserSigningMember()));
                items.Add(new SelectListItem("Rejected", "2"));
                if (!archivedFilterValue.HasValue || !archivedFilterValue.Value)
                    items.Add(new SelectListItem("Pending", "0"));
                if (!archivedFilterValue.HasValue || archivedFilterValue.Value)
                    items.Add(new SelectListItem("Retracted", "3"));
            }

            return
                new JsonResult(items);
        }

        public async Task<IActionResult> OnPostResendItems(int[] requestIds)
        {
            try
            {
                await RequestServices.MarkRequestResentAsync(CurrentUser.Id, requestIds);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    errorMessage = ex.Message
                });
            }
        }

        public async Task<IActionResult> OnPostRetractItems(int[] requestIds)
        {
            try
            {
                await RequestServices.MarkRequestIncompleteAsync(CurrentUser.Id, requestIds);
                await RequestServices.MarkRequestArchivedAsync(CurrentUser.Id, requestIds);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    errorMessage = ex.Message
                });
            }
        }
    }

    public class Filter<T>
    {
        public T DefaultValue { get; set; }
        public bool Enabled { get; set; } = true;
        public IList<SelectListItem> Items { get; set; } = System.Array.Empty<SelectListItem>();
        public T Value { get => ValueConversion(Items.FirstOrDefault(item => item.Selected)?.Value); }
        public Func<string, T> ValueConversion { get; set; }
    }

    public class RequestHistoryItem
    {
        public int FormId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public object SubmittedAtOneYear => null;   // Used only so Kendo will create an arbitrary filter with this name
        public DateTime? EffectiveDate { get; set; }
        public string Form { get; set; }
        public DateTime? RequestStatusDate { get; set; }
        public int Age { get; set; }
        public short? RequestStatus { get; set; }
        public string Status { get; set; }
        public string PdfUrl { get; set; }
        public string Office { get; set; }

        public bool SubmitterHasArchived { get; internal set; }

        public string LastModified { get; set; }

        public string PatientName { get; set; }
        public DateTime PatientBirthdate { get; set; }
        public string MedicalRecordNumber { get; set; }

        public string RejectionReason { get; set; }

        public int SignerMemberId { get; set; }
        public int? SignerOrganizationId { get; set; }
        public string SignerName { get; set; }
        public string SignerLastName { get; set; }
        public string Practice { get; set; }
        public string CollaboratorName { get; set; }

        public string SubmitterOrganizationName { get; set; }
        public int? SubmitterOrganizationId { get; set; }
        public bool SubmitterHasFiled { get; internal set; }
        public string SubmitterFiledString { get; internal set; }
        public string SignerFiledString { get; internal set; }
        public bool SignerHasFiled { get; internal set; }
        public string ApprovalStatus { get; internal set; }
        public string FaxStatus { get; internal set; }
        public int ManualRetry { get; internal set; }

        public string RowTooltip { get; set; }
    }

    public class PatientSearchItem
    {
        public string PatientId { get; set; }
        public string SearchSummary { get; set; }
    }

    public class MrnSearchItem
    {
        public string PatientId { get; set; }
        public string MedicalRecordNumber { get; set; }
    }
}