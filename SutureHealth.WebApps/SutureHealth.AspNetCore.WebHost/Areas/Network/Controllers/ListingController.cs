using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using LinqKit;
using SutureHealth.AspNetCore.Areas.Network.Models;
using SutureHealth.AspNetCore.Areas.Network.Models.Listing;
using SutureHealth.Providers;
using SutureHealth.Providers.Services;
using SutureHealth.Application.Services;
using SutureHealth.Application;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Network.Controllers
{
    [Area("Network")]
    [Route("Network")]
    public class ListingController : Controller
    {
        protected const int PROVIDER_SERVICE_RADIUS_CAP = 256;

        protected ILogger<ListingController> Logger { get; }
        protected IApplicationService SecurityService { get; }
        protected INetworkServicesProvider ProviderService { get; }

        public ListingController
        (
            ILogger<ListingController> logger,
            IApplicationService securityService,
            INetworkServicesProvider providerService
        )
        {
            Logger = logger;
            SecurityService = securityService;
            ProviderService = providerService;
        }

        [HttpPost]
        [Route("Listing", Name = "NetworkListing")]
        [Route("Listing/{pageNumber:min(1)}/{pageSize:min(1)}")]
        public async Task<IActionResult> Listing(MemberIdentity sutureUser, [FromBody] ListingRequest request, int pageNumber = 1, int pageSize = 25, int retryCount = 0)
        {
            try
            {
                if (request.Preset != FilterPreset.Invitations)
                {
                    var entities = await QueryEntitiesAsync(sutureUser, request, (pageNumber - 1) * pageSize, pageSize);

                    return PartialView("_PagedEntityListing", new IndexViewModel.PagedEntityListing()
                    {
                        HasMorePages = entities.Count() >= pageSize,
                        IsFirstPage = pageNumber == 1,
                        Entities = await GetEntityListItemsFromProviderEntitiesAsync(sutureUser, entities, request.Preset)
                    });
                }
                else
                {
                    var entities = await QueryEntitiesAsync(sutureUser, request, 0, 500);
                    if (entities.ToArray().Length == 0)
                    {
                        return PartialView("_InvitationsVerbage");
                    }

                    return PartialView("_GroupedEntityListing", new IndexViewModel.GroupedEntityListing()
                    {
                        EntitiesByGroup = entities.ToArray()
                                                  .GroupBy(e =>
                                                  {
                                                      switch (e.InvitationStatus)
                                                      {
                                                          case "New":
                                                          case "Contacting":
                                                          case "Replied":
                                                          case "Discussing":
                                                          default:
                                                              return "Pending";
                                                          case "Accepted":
                                                          case "Joined":
                                                          case "Collaborating":
                                                              return "Accepted";
                                                          case "Declined":
                                                              return "Declined";
                                                          case "Unreachable":
                                                              return "Unreachable";
                                                          case "Canceled":
                                                              return "Canceled";
                                                      }
                                                  })
                                                  .OrderBy(grp =>
                                                  {
                                                      switch (grp.Key)
                                                      {
                                                          case "Accepted":
                                                              return 1;
                                                          case "Declined":
                                                              return 2;
                                                          case "Pending":
                                                              return 0;
                                                          case "Unreachable":
                                                              return 3;
                                                          case "Canceled":
                                                              return 4;
                                                          default:
                                                              return int.MaxValue;
                                                      }
                                                  })
                                                  .Select(async grp => new KeyValuePair<string, IEnumerable<EntityListItem>>(grp.Key, await GetEntityListItemsFromProviderEntitiesAsync(sutureUser, grp.OrderBy(request.SortOrder), request.Preset)))
                                                  .Select(t => t.Result)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Exception occurred in Listing() (pageNumber: {pageNumber})");

                return PartialView("_FailedEntityListing", new IndexViewModel.FailedEntityListing()
                {
                    RetryCount = retryCount + 1,
                    ShouldRetry = retryCount < 2,
                    DisplayMessage = pageNumber == 1
                });
            }
        }

        [HttpPost]
        [Route("Listing/Download", Name = "NetworkListingDownload")]
        public async Task<IActionResult> ListingDownload(MemberIdentity sutureUser, [FromForm] ListingDownloadRequest request)
        {
            var csvBuilder = new System.Text.StringBuilder();
            var textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;

            var entities = await QueryEntitiesAsync(sutureUser, request, 0, request.ProvidersOnPage);

            csvBuilder.AppendLine(string.Join(",", (new string[] { "Type", "Organization", "First Name", "Last Name", "Professional Suffix", "NPI", "Address1", "Address2", "City", "State", "Postal Code", "Phone", "Account Type", "Joined" }).Select(v => $"\"{v}\"")));

            foreach (var entity in entities)
            {
                var individual = (ProviderType)entity.ProviderType == ProviderType.Individual ? true : false;
                var row = new string[] {
                    individual ? "Clinician" : "Organization",
                    textInfo.ToTitleCase((individual ? entity.SuturePrimaryFacilityName?.ToLower() : textInfo.ToTitleCase(entity.FullName.ToLower())) ?? ""),
                    individual ? textInfo.ToTitleCase(entity.FirstName.ToLower()) : null,
                    individual ? textInfo.ToTitleCase(entity.LastName.ToLower()) : null,
                    individual ? entity.ProfessionalSuffix : null,
                    entity.NPI.ToString(),
                    textInfo.ToTitleCase(entity.AddressLine1.ToLower()), 
                    textInfo.ToTitleCase(entity.AddressLine2?.ToLower()), 
                    textInfo.ToTitleCase(entity.City.ToLower()), 
                    entity.State, 
                    StringFormatters.ToFormattedZip(entity.PostalCode), 
                    StringFormatters.ToFormattedPhoneNumber(entity.TelephoneNumber), 
                    entity.SutureCustomerType.ToString(), 
                    entity.SutureCreatedAt?.ToString("d") 
                };
                csvBuilder.AppendLine(string.Join(",", row.Select(v => v != null ? $"\"{v}\"" : $"\"\"")));
            }

            byte[] entityArray = System.Text.Encoding.ASCII.GetBytes(csvBuilder.ToString());
            var now = DateTime.Now.ToString("d");
            return File(entityArray, "text/csv", $"SutureHealth Providers - {now}.csv");
        }

        [HttpPost]
        [Route("Listing/Download/Form", Name = "NetworkListingDownloadForm")]
        public IActionResult ListingDownloadForm([FromBody] ListingDownloadRequest request)
        {
            return PartialView("_ListingDownloadForm", request);
        }

        protected async Task<IEnumerable<EntityListItem>> GetEntityListItemsFromProviderEntitiesAsync(MemberIdentity sutureUser, IEnumerable<NetworkProvider> entities, FilterPreset? activePreset = null)
        {
            var facility = await SecurityService.GetOrganizationByIdAsync(sutureUser.CurrentOrganizationId.GetValueOrDefault(sutureUser.PrimaryOrganizationId));
            return entities.Select(e =>
            {
                bool isSutureCustomer = e.SutureCustomerType != SutureCustomerType.NonMember,
                     isPaidCustomer = e.SutureCustomerType == SutureCustomerType.Enterprise,
                     enterpriseRelationship = isSutureCustomer && (isPaidCustomer || sutureUser.IsPayingClient),
                     inviteOnlyMode = activePreset == FilterPreset.InviteSenders;
                var providerType = (ProviderType)e.ProviderType;
                var availableButtons = Enum.GetValues(typeof(EntityListItem.ActionButton)).Cast<EntityListItem.ActionButton>()
                                           .Where(btn =>
                                           {
                                               var upgradeMembershipEnabled = !enterpriseRelationship && isSutureCustomer && sutureUser.IsUserSender();

                                               if (
                                                    (inviteOnlyMode && btn != EntityListItem.ActionButton.Invite) ||
                                                    (upgradeMembershipEnabled && btn != EntityListItem.ActionButton.UpgradeMembership)
                                                  )
                                               {
                                                   return false;
                                               }

                                               switch (btn)
                                               {
                                                   case EntityListItem.ActionButton.GetSignature:
                                                       return sutureUser.IsUserSender() && e.CanSign && enterpriseRelationship;
                                                   case EntityListItem.ActionButton.Invite:
                                                       return sutureUser.IsInvitationsEligible() && (!isSutureCustomer || e.IsSender && (e.SutureCustomerType == SutureCustomerType.Community || (e.SutureCustomerType == SutureCustomerType.Enterprise && !e.IsInClientsNetwork)));
                                                   case EntityListItem.ActionButton.Message:
                                                   case EntityListItem.ActionButton.Refer:
                                                   case EntityListItem.ActionButton.Video:
                                                       return !(sutureUser.IsUserSigningMember() && !sutureUser.IsPayingClient) || (isPaidCustomer && e.IsInClientsNetwork) || e.IsSigner;
                                                   case EntityListItem.ActionButton.UpgradeMembership:
                                                       return upgradeMembershipEnabled;
                                                   default:
                                                       return false;
                                               }
                                           });

                return new EntityListItem()
                {
                    ProviderId = e.ProviderId,
                    Npi = e.NPI.GetValueOrDefault(),
                    Name = providerType == ProviderType.Individual ?
                        ($"{e.FirstName} {e.LastName}").ToPascalCase() + (string.IsNullOrWhiteSpace(e.ProfessionalSuffix) ? string.Empty : $", {e.ProfessionalSuffix.ToUpper()}") :
                        e.FullName.ToPascalCase(),
                    Phone = System.Text.RegularExpressions.Regex.Replace(e.TelephoneNumber ?? string.Empty, @"[^\d]", string.Empty),
                    FullAddress = string.Join(", ", (new string[]
                    {
                        e.AddressLine1?.ToPascalCase(),
                        e.AddressLine2?.ToPascalCase(),
                        e.City?.ToPascalCase(),
                        $"{e.State} {e.PostalCode}"
                    }).Where(ap => !string.IsNullOrWhiteSpace(ap))),
                    Distance = Math.Round(e.Distance, 1),
                    ProviderType = providerType,
                    Relationships = (providerType == ProviderType.Organization ? e.Facilities.Where(f => f.Clinician != null && f.Clinician.IsActiveInSutureSign).Select(c => c.Clinician) : e.Clinicians.Where(c => !c.Primary.GetValueOrDefault(false)).Select(f => f.Facility)).Select(re => new EntityListItem.Relationship()
                    {
                        Name = string.Join(", ", (new string[] {
                            re.FullName?.ToPascalCase(),
                            re.ProfessionalSuffix
                        }).Where(np => !string.IsNullOrWhiteSpace(np))),
                        Phone = StringFormatters.ToFormattedPhoneNumber(System.Text.RegularExpressions.Regex.Replace(re.TelephoneNumber ?? string.Empty, @"[^\d]", string.Empty)),
                        FullAddress = string.Join(", ", (new string[]
                        {
                            re.AddressLine1?.ToPascalCase(),
                            re.AddressLine2?.ToPascalCase(),
                            re.City?.ToPascalCase(),
                            $"{re.State} {re.PostalCode}"
                        }).Where(ap => !string.IsNullOrWhiteSpace(ap)))
                    }).OrderBy(x => x.Name),
                    ServiceTypes = e.ServiceTypes.Where(st => !string.IsNullOrWhiteSpace(st.Service?.Description)).Select(st => st.Service.Description).Distinct(),
                    ClaimsWithUser = e.MedicareClaimsCountWithProvider,
                    HasBeenInvitedByUser = (e.InvitationDate != null),
                    IsPreselected = e.MedicareClaimsCountWithProvider > 0 && activePreset == FilterPreset.InviteSenders,
                    Buttons = availableButtons,
                    SutureCustomer = !isSutureCustomer ? null : new EntityListItem.SutureCustomerDetails()
                    {
                        SignerUserId = e.CanSign ? e.SutureUserId : null,
                        AccountType = e.SutureCustomerType.ToString(),
                        DateCreated = e.SutureCreatedAt.Value,
                        DateCreatedTicks = e.SutureCreatedAt.Value.Ticks,
                        EstimatedClaims = e.MedicareClaimsCountWithProvider > 10 ? ((e.MedicareClaimsCountWithProvider * 2) / 10) * 10 : e.MedicareClaimsCountWithProvider,
                        IsInNetwork = e.IsInClientsNetwork && e.SutureUserId != sutureUser.Id && e.SutureFacilityId != facility.OrganizationId,
                        ParentFacilityName = e.SuturePrimaryFacilityName
                    }
                };
            });
        }

        protected async Task<IEnumerable<NetworkProvider>> QueryEntitiesAsync(MemberIdentity sutureUser, SearchFilter filter, int skip = 0, int take = 100)
        {
            var userFacility = await SecurityService.GetOrganizationByIdAsync(sutureUser.CurrentOrganizationId.GetValueOrDefault(sutureUser.PrimaryOrganizationId));
            var searchRadius = filter.Radius != SearchRadius.EntireState ? (int?)filter.Radius : null;
            var providerPreset = null as PresetNetworkFilterType?;
            var fromDate = null as DateTimeOffset?;

            switch (filter.Preset)
            {
                case FilterPreset.RecentlyJoined:
                    fromDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(90));
                    break;
                case FilterPreset.MyNetwork:
                    providerPreset = PresetNetworkFilterType.MyNetwork;
                    fromDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(365));
                    break;
                case FilterPreset.ClaimsWithMe:
                    providerPreset = PresetNetworkFilterType.ClaimsWithMe;
                    break;
                case FilterPreset.Invitations:
                    providerPreset = PresetNetworkFilterType.Invitations;
                    break;
                case FilterPreset.InviteSenders:
                    providerPreset = PresetNetworkFilterType.InviteSenders;
                    break;
            }

            // The below logic is strictly to protect ourselves from executing an expensive query; remove or refactor as needed as provider service enhancements are made.
            if (
                searchRadius.HasValue && searchRadius.Value > PROVIDER_SERVICE_RADIUS_CAP &&
                fromDate == null &&
                providerPreset == null &&
                string.IsNullOrWhiteSpace(filter.Search) &&
                (filter.Filters == null || !filter.Filters.Any())
            )
            {
                searchRadius = PROVIDER_SERVICE_RADIUS_CAP;
            }

            var predicate = PredicateBuilder.New<NetworkProvider>();
            predicate.And(x => x.SutureCloseDate == null);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                if (long.TryParse(filter.Search, out long npi))
                {
                    predicate.And(pe => pe.FullName.ToLower().IndexOf(filter.Search.ToLower()) > -1 || pe.NPI == npi);
                }
                else
                {
                    predicate.And(pe => (pe.FullName.ToLower().IndexOf(filter.Search.ToLower()) > -1) || pe.SigningName.ToLower().Contains(filter.Search.ToLower()));
                }
            }

            if (!searchRadius.HasValue)
            {
                predicate.And(pe => pe.State == userFacility.StateOrProvince);
            }

            if (filter.Filters != null && filter.Filters.Any())
            {
                foreach (var filterGroup in filter.Filters.GroupBy(f => f.SectionKey))
                {
                    var expressions = new List<Expression<Func<NetworkProvider, bool>>>(filterGroup.Count());

                    foreach (var token in filterGroup)
                    {
                        switch (filterGroup.Key)
                        {
                            case FilterCategoryType.Organizations:
                                expressions.Add(NetworkConfiguration.FilterOrganizations[token.FilterKey].Mapping);
                                break;
                            case FilterCategoryType.AccountType:
                                expressions.Add(NetworkConfiguration.FilterAccountTypes[token.FilterKey].Mapping);
                                break;
                            case FilterCategoryType.Clinicians:
                                expressions.Add(NetworkConfiguration.FilterClinicians[token.FilterKey].Mapping);
                                break;
                        }
                    }

                    if (expressions.Any())
                    {
                        var filters = PredicateBuilder.New<NetworkProvider>();
                        foreach (var e in expressions)
                        {
                            filters.Or(e);
                        }

                        predicate.And(filters);
                    }
                }
            }

            var query = await this.ProviderService.FindProvidersByPreset(providerPreset,
                                                                         sutureUser.IsUserSigningMember() ? sutureUser.Id : null,
                                                                         sutureUser.IsUserSender() ? userFacility.OrganizationId : null,
                                                                         userFacility.PostalCode,
                                                                         searchRadius,
                                                                         fromDate.HasValue ? (DateTime?)fromDate.Value.DateTime : null,
                                                                         predicate: predicate.Expand() as Expression<Func<NetworkProvider, bool>>,
                                                                         orderBy: filter.SortOrder.AsProviderServiceSortMethod(),
                                                                         skip: skip,
                                                                         take: take);
            var d = query.ToArray();
            return d;
        }
    }
}
