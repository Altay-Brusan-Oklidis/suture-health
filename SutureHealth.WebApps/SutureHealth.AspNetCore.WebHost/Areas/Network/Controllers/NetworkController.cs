using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Network.Models;
using SutureHealth.AspNetCore.Areas.Network.Models.Network;
using SutureHealth.AspNetCore.Areas.Network.Models.Listing;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Network.Controllers
{
    [Area("Network")]
    [Route("Network")]
    public class NetworkController : Controller
    {
        protected IApplicationService SecurityService { get; }

        public NetworkController
        (
            IApplicationService securityService
        )
        {
            SecurityService = securityService;
        }

        [HttpGet]
        [Route("", Name = "Network")]
        [Route("{preset:alpha}", Name = "NetworkPreset")]
        public async Task<IActionResult> Index(FilterPreset? preset, bool contentOnly = false)
        {
            if (!preset.HasValue)
            {
                return RedirectToRoute("NetworkPreset", new { preset = FilterPreset.NearMe, contentOnly });
            }

            ViewBag.Title = "Network";

            return View(new IndexViewModel()
            {
                CurrentUser = CurrentUser,
                Initialization = GetInitializationParameters(preset),
                Mode = new IndexViewModel.ModeSelector()
                {
                    Presets = (new PresetSelection[]
                    {
                        new PresetSelection(FilterPreset.NearMe),
                        new PresetSelection(FilterPreset.MyNetwork),
                        new PresetSelection(FilterPreset.ClaimsWithMe),
                        new PresetSelection(FilterPreset.Invitations),
                        new PresetSelection(FilterPreset.RecentlyJoined)
                    }).Where(ps => ps.Preset != FilterPreset.Invitations || CurrentUser.IsInvitationsEligible())
                },
                Filter = new IndexViewModel.FilterSelector()
                {
                    AccountType = new UnifiedFilterCategory()
                    {
                        Title = FilterCategoryType.AccountType.GetEnumDescription(),
                        Filters = NetworkConfiguration.FilterAccountTypes.Select(f => new KeyValuePair<string, string>(f.Key, f.Value.Name)),
                        CanClearSection = false,
                        Type = FilterCategoryType.AccountType
                    },
                    Provider = new SectionedFilterCategory()
                    {
                        Title = "Provider",
                        Sections = new FilterCategorySection[]{
                            new FilterCategorySection()
                            {
                                Title = FilterCategoryType.Organizations.GetEnumDescription(),
                                Filters = NetworkConfiguration.FilterOrganizations.Select(f => new KeyValuePair<string, string>(f.Key, f.Value.Name)),
                                Type = FilterCategoryType.Organizations
                            },
                            new FilterCategorySection()
                            {
                                Title = FilterCategoryType.Clinicians.GetEnumDescription(),
                                Filters = NetworkConfiguration.FilterClinicians.Select(f => new KeyValuePair<string, string>(f.Key, f.Value.Name)),
                                Type = FilterCategoryType.Clinicians
                            }
                        },
                        CanClearSection = true
                    }
                },
                Header = new IndexViewModel.SearchHeader(),
                Listing = new IndexViewModel.EntityListing()
                {
                    PageSize = NetworkConfiguration.LISTING_PAGE_SIZE,
                    LastInvocationDateTicks = (await SecurityService.InvokeUserLastAccessDateAsync(CurrentUser.Id)).Ticks,
                    UpgradeUrl = await SecurityService.GetApplicationSettings().Where(s => s.Key == "Network.UpgradeUrl" && s.IsActive == true).Select(s => s.ItemString).FirstOrDefaultAsync() ?? @"https://www.suturehealth.com/UpgradeSender",
                    NewFeatureUrl = await SecurityService.GetApplicationSettings().Where(s => s.Key == "Network.NewFeatureUrl" && s.IsActive == true).Select(s => s.ItemString).FirstOrDefaultAsync() ?? @"https://www.suturehealth.com/feedback"
                },
                RequireClientHeader = !contentOnly
            });
        }

        protected IndexViewModel.InitializationParameters GetInitializationParameters(FilterPreset? preset)
        {
            var presets = new Dictionary<string, ListingRequest>()
            {
                { FilterPreset.NearMe.ToString(), new ListingRequest()
                    {
                        Preset = FilterPreset.NearMe,
                        SortOrder = SearchFilterSortMethod.DistanceClosestFirst,
                        Radius = SearchRadius.Fifty
                    }
                },
                { FilterPreset.MyNetwork.ToString(), new ListingRequest() { Preset = FilterPreset.MyNetwork } },
                { FilterPreset.ClaimsWithMe.ToString(), new ListingRequest()
                    {
                        Preset = FilterPreset.ClaimsWithMe,
                        SortOrder = SearchFilterSortMethod.ClaimsHistory
                    }
                },
                { FilterPreset.Invitations.ToString(), new ListingRequest()
                    {
                        Preset = FilterPreset.Invitations,
                        SortOrder = SearchFilterSortMethod.Name,
                        Radius = SearchRadius.Infinity
                    }
                },
                { FilterPreset.RecentlyJoined.ToString(), new ListingRequest()
                    {
                        Preset = FilterPreset.RecentlyJoined,
                        SortOrder = SearchFilterSortMethod.RecentlyJoined,
                        Radius = CurrentUser.IsUserSigningMember() ? SearchRadius.Fifty : SearchRadius.OneHundred,
                        Filters = new FilterToken[]
                        {
                            new FilterToken() { SectionKey = FilterCategoryType.AccountType, FilterKey = "Enterprise" },
                            new FilterToken() { SectionKey = FilterCategoryType.AccountType, FilterKey = "Community" }
                        }
                    }
                },
                { FilterPreset.InviteSenders.ToString(), new ListingRequest()
                    {
                        Preset = FilterPreset.InviteSenders,
                        SortOrder = SearchFilterSortMethod.ClaimsHistory,
                        Radius = SearchRadius.OneHundred,
                        Filters = new FilterToken[]
                        {
                            new FilterToken() { SectionKey = FilterCategoryType.AccountType, FilterKey = "Community" },
                            new FilterToken() { SectionKey = FilterCategoryType.AccountType, FilterKey = "Extended" },
                            new FilterToken() { SectionKey = FilterCategoryType.Organizations, FilterKey = "HomeHealth" },
                            new FilterToken() { SectionKey = FilterCategoryType.Organizations, FilterKey = "Hospice" },
                            new FilterToken() { SectionKey = FilterCategoryType.Organizations, FilterKey = "MedicalEquipment" },
                            new FilterToken() { SectionKey = FilterCategoryType.Organizations, FilterKey = "SkilledNursingFacility" }
                        }
                    }
                }
            };

            return new IndexViewModel.InitializationParameters()
            {
                ViewStyle = null,
                Presets = presets,
                QueryState = preset.HasValue && presets.ContainsKey(preset.Value.ToString()) ? presets[preset.Value.ToString()] : new ListingRequest()
            };
        }
    }
}
