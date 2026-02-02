using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ProducesResponseTypeAttribute = SutureHealth.AspNetCore.Mvc.ProducesResponseTypeAttribute;
using System.Collections.Generic;
using AutoMapper;
using SutureHealth.Application.Services;
using System;
using System.Threading.Tasks;
using SutureHealth.Application.v0100.Models;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Application.v01._00.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
    [ControllerName("FeatureFlags")]


    public class FeatureFlagsController : SutureHealth.AspNetCore.Mvc.ControllerBase
    {
        protected IFeatureFlagsServices featureFlagsServices { get; }
        protected IApplicationService SecurityService { get; }

        public FeatureFlagsController
        (
            IFeatureFlagsServices featureFlagsServices,
            IApplicationService securityService
        )
        {
            this.featureFlagsServices = featureFlagsServices;
            SecurityService = securityService;
        }

        [HttpGet("api/v{version:apiVersion}/featureflag")]
        [ProducesResponseType(HttpStatusCode.OK)]
        public async Task<IActionResult> GetFeatureFlagsAsync()
        {
            if (CurrentUser is null)
            {
                return Unauthorized();
            }
            else
            {
                try
                {
                    var featureFlags = await featureFlagsServices.GetFeatureFlagsByUserId(CurrentUser.Id);
                    return Ok(new { data = featureFlags });
                }
                catch (Exception ex)
                {
                    return BadRequest(ErrorBuilder.New()
                                                 .SetMessage($"{ex.Message}")
                                                 .Build());
                }
            }
        }        

        [HttpGet("api/v{version:apiVersion}/featureflags")]
        [ProducesResponseType(HttpStatusCode.OK)]
        public async Task<IActionResult> GetFeatureFlagsByFilter
        (
            [FromQuery] bool? isDeleted,
            [FromQuery] bool? isRestored,
            [FromQuery] bool? isActive
        )
        {
            if (CurrentUser is null || !CurrentUser.IsApplicationAdministrator())
            {
                return Unauthorized();
            }

            var featureFlags = featureFlagsServices.GetFeatureFlags()
                                            .Where(ff => ((isDeleted.HasValue && isDeleted == true) ?
                                                            ff.DeleteDate != null && ff.Active == false :
                                                            (isDeleted.HasValue && isDeleted == false) ?
                                                            ff.DeleteDate == null && ff.Active == true :
                                                            true) &&
                                                            ((isRestored.HasValue && isRestored == true) ?
                                                            ff.RestoreDate!= null && ff.Active == true :
                                                            (isRestored.HasValue && isRestored == false) ?
                                                            ff.RestoreDate == null && ff.Active == false :
                                                            true) &&
                                                            ((isActive.HasValue && isActive == true) ?
                                                            ff.Active == true :
                                                            (isActive.HasValue && isActive == false) ?
                                                            ff.Active == false :
                                                            true));

            return Ok(featureFlags);                                                   
        }        


        [HttpPut("api/v{version:apiVersion}/featureflag")]
        [ProducesResponseType(HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateFeatureFlagAsync
        (
            [FromBody] FeatureFlagUpdate featureFlagUpdate
        )
        {
            if (CurrentUser is null || !CurrentUser.IsApplicationAdministrator())
            {
                return Unauthorized();
            }
            var featureFlag = await featureFlagsServices.GetFeatureFlagByFlagId(featureFlagUpdate.Id);
            featureFlag.Name = string.Join("-", new[]
            {featureFlagUpdate.ProductArea, featureFlagUpdate.FeatureName, featureFlagUpdate.SubfeatureName}
                        .Where(ff => !string.IsNullOrEmpty(ff)));            
            featureFlag.Description = featureFlagUpdate.Description;
            featureFlag.UpdateDate = DateTime.UtcNow;
            featureFlag.UpdatedBy = CurrentUser.MemberId;

            await featureFlagsServices.UpdateFeatureFlag(featureFlag, CurrentUser.MemberId);

            return Ok();
        }

        [HttpPut("api/v{version:apiVersion}/bulkupdatefeatureflag")]
        [ProducesResponseType(HttpStatusCode.OK)]
        public async Task<IActionResult> BulkUpdateFeatureFlagsAsync
        (
            [FromBody] FeatureFlagBulk[] featureFlagUpdates
        )
        {
            if (CurrentUser is null || !CurrentUser.IsApplicationAdministrator())
            {
                return Unauthorized();
            }

            var featureFlags = featureFlagsServices.GetFeatureFlags().AsEnumerable()
                                                  .Where(ff => featureFlagUpdates.Any(ffu => ffu.Id == ff.Id)).ToList();
            featureFlags.ForEach(ff => ff.Active = featureFlagUpdates.FirstOrDefault(ffu => ffu.Id == ff.Id).IsActive);            

            await featureFlagsServices.UpdateFeatureFlags(featureFlags.ToArray(), CurrentUser.MemberId);

            return Ok();
        }
    }
}
