using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Asp.Versioning;
using AutoMapper;
using AutoMapper.AspNet.OData;
using SutureHealth.Application.Services;
using ProducesResponseTypeAttribute = SutureHealth.AspNetCore.Mvc.ProducesResponseTypeAttribute;
using Microsoft.AspNetCore.Http;
using System;
using SutureHealth.Application.v0100.Models;
using System.Collections.Generic;
using System.IO;
using SutureHealth.Security;
using SutureHealth.Reporting.Services;
using SutureHealth.AspNetCore.SwaggerGen.Attributes;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Application.v0100.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
[ControllerName("ApiOrganization")]
public class OrganizationController : SutureHealth.AspNetCore.Mvc.ControllerBase
{
    private static readonly List<ImageFormat> AllowedImageTypes = new() { ImageFormat.Png };
    private static readonly int MaxAllowedImageSize = 5242880; // 5mb = 5 * 1024 * 1024 = 5242880
    private static readonly int MinWidthAndHeight = 96;
    private static readonly int MaxWidthAndHeight = 1024;


    [HttpGet("api/v{version:apiVersion}/organizations")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(Models.CustomOrganization))]
    public async Task<IActionResult> GetOrganizations
    (
       [FromServices] IApplicationService applicationService,
       [FromServices] IMapper mapper,
       [FromQuery] bool withLogo
    )
    {
        if (CurrentUser is null)
            return Unauthorized();

        var orgs = applicationService.GetOrganizationMembersByMemberId(CurrentUser.MemberId)
                                        .Where(om => om.IsActive)
                                        .Select(om => om.Organization)
                                        .Distinct();

        var orgIds = orgs.Select(o => o.OrganizationId).ToArray();

        var orgIdsThatMemberIsAdminAt = applicationService
                                       .GetAdminOrganizationMembersByOrganizationId(orgIds)
                                       .Where(om => om.MemberId == CurrentUser.MemberId)
                                       .Select(om => om.OrganizationId).ToArray();
        
        var orgImages = await applicationService.GetMultipleOrganizationImages(orgIds);
        var orgSubscriptions = await applicationService.GetBillableEntities(orgIds).ToArrayAsync();

        var listOfOrganizationLogo = new Dictionary<int, byte[]>();
        if (withLogo)
        {
            foreach (var orgImage in orgImages)
            {
                var logo = await applicationService.GetOrganizationImageFromS3(orgImage.OrganizationImageId.ToString());
                listOfOrganizationLogo.Add(orgImage.OrganizationId, logo.ToByteArray());
            }
        }

        var customOrganizationsList = mapper.Map<List<CustomOrganization>>(orgs, opt =>
                                    {
                                        opt.Items["CurrentUser"] = CurrentUser;
                                        opt.Items["Logos"] = listOfOrganizationLogo;
                                        opt.Items["AdminOrganizationIds"] = orgIdsThatMemberIsAdminAt;
                                        opt.Items["MarketingPromoSubscription"] = orgSubscriptions;
                                    })
                                    .OrderByDescending(o => o.IsPrimary).ThenBy(o => o.Name).ToArray();

        return Ok(customOrganizationsList);
    }


    [HttpGet("api/v{version:apiVersion}/primaryorganization")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(Models.Organization))]
    public async Task<IActionResult> GetPrimaryOrganization
    (
       [FromServices] IApplicationService applicationService
    ) => Ok(await applicationService.GetOrganizationByIdAsync(CurrentUser.PrimaryOrganizationId));

    [HttpPost("api/v{version:apiVersion}/upgradeorganizationplan")]
    [ProducesResponseType(HttpStatusCode.OK)]
    [FeatureFlag("UserMenu-SubscriptionManagement", HttpStatusCode.Forbidden)]
    public async Task<IActionResult> UpgradeOrganizationPlan
    (
        [FromServices] IOrganizationService organizationService,
        [FromServices] IDeliveryService deliveryService,
        [FromQuery] int organizationId
    )
    {              
        Organization organization = await organizationService.GetOrganizationByIdAsync(organizationId);
        await deliveryService.SendOrganizationPlanUpgradeEmailAsync(CurrentUser, organization.OrganizationId, organization.Name, organization.PhoneNumber);
        
        return Ok();
    }

    [HttpPost("api/v{version:apiVersion}/organizationimage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    [FeatureFlag("Header-SenderLogo", HttpStatusCode.Forbidden)]
    public async Task<IActionResult> CreateOrganizationImage
    (
        [FromServices] IOrganizationService organizationService,
        [FromServices] IOrganizationMemberService organizationMemberService,
        [FromServices] IImageProcessingService imageProcessingService,
        IFormFile organizationlogo,
        [FromQuery] int organizationId,
        [FromQuery] bool isPrimary
    )
    {
        var isUserAdmin = organizationMemberService.GetAdminOrganizationMembersByOrganizationId(organizationId)
                                                      .FirstOrDefault(om => om.MemberId == CurrentUser.MemberId);
        if (isUserAdmin == null || isUserAdmin == default)
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Only administrator of organization can upload the logo.")
                                          .Build());
        }


        if (imageProcessingService.IsImageFileEmptyOrNull(organizationlogo))
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Please upload an image in png format.")
                                          .Build());
        }

        if (!imageProcessingService.IsImageTypeValid(organizationlogo, AllowedImageTypes))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Incorrect format, please upload an image in png format.")
                                         .Build());
        }

        if (!imageProcessingService.IsImageFileSizeValid(organizationlogo, MaxAllowedImageSize))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded file should be smaller than 5 mb.")
                                         .Build());
        }

        if (!imageProcessingService.IsImageResolutionValid(organizationlogo, MinWidthAndHeight, MinWidthAndHeight,
                                                                             MaxWidthAndHeight, MaxWidthAndHeight))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded image should have a minimum resolution of 96x96 and a maximum resolution of 1024x1024.")
                                         .Build());
        }

        var organizationImages = await organizationService.GetOrganizationImages(organizationId);
        if (organizationImages != null && organizationImages.Count() >= 2)
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("You can not create more than two organization images for same organizationId")
                                         .Build());
        }
        
        if (organizationImages.Any(mi => mi.IsPrimary) && isPrimary)
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("You can not create more than one primary organization image for same organization")
                                         .Build());
        }
        if (organizationImages.Any(mi => !mi.IsPrimary) && !isPrimary)
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("You can not create more than one edited organization image for same organization")
                                         .Build());
        }

        var organizationImageId = Guid.NewGuid();
        await organizationService.CreateOrganizationImage(new OrganizationImage()
        {
            OrganizationImageId = organizationImageId,
            OrganizationId = organizationId,
            IsPrimary = isPrimary,
            UploadDate = DateTime.UtcNow,
            Active = true
        });

        var compressedLogo = imageProcessingService.CompressImage(organizationlogo);
        var response = await organizationService.InsertOrganizationImageToS3(organizationImageId.ToString(), compressedLogo);
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Insert operation confronted with errors.")
                                          .Build());
        }

        return Ok();
    }


    [HttpDelete("api/v{version:apiVersion}/organizationImage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    [FeatureFlag("Header-SenderLogo", HttpStatusCode.Forbidden)]

    public async Task<IActionResult> DeleteOrganizationImage
    (
        [FromServices] IOrganizationService organizationService,
        [FromServices] IOrganizationMemberService organizationMemberService,
        [FromBody] int organizationId
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }
        var isUserAdmin = organizationMemberService.GetAdminOrganizationMembersByOrganizationId(organizationId)
                                                      .FirstOrDefault(om => om.MemberId == CurrentUser.MemberId);
        if (isUserAdmin == null || isUserAdmin == default)
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Only administrator of organization can delete the logo.")
                                          .Build());
        }

        var orgImages = (await organizationService.GetOrganizationImages(organizationId)).ToArray();
        if (orgImages == null || orgImages.Count() == 0)
        {
            return BadRequest(ErrorBuilder.New()
                                        .SetMessage("There is no organization Image for given organization Id")
                                        .Build());
        }

        organizationService.DeleteOrganizationImages(orgImages);
        foreach (var orgImage in orgImages)
        {
            await organizationService.DeleteOrganizationImageFromS3(orgImage.OrganizationImageId.ToString());
        }

        return Ok();
    }

    [HttpPut("api/v{version:apiVersion}/organizationimage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    [FeatureFlag("Header-SenderLogo", HttpStatusCode.Forbidden)]
    public async Task<IActionResult> UpdateOrganizationImage
    (
        [FromServices] IOrganizationService organizationService,
        [FromServices] IImageProcessingService imageProcessingService,
        [FromServices] IOrganizationMemberService organizationMemberService,
        IFormFile organizationlogo,
        [FromQuery] int organizationId,
        [FromQuery] bool isPrimary
    )
    {
        var isUserAdmin = organizationMemberService.GetAdminOrganizationMembersByOrganizationId(organizationId)
                                                      .FirstOrDefault(om => om.MemberId == CurrentUser.MemberId);
        if (isUserAdmin == null || isUserAdmin == default)
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Only administrator of organization can update the logo.")
                                          .Build());
        }

        if (imageProcessingService.IsImageFileEmptyOrNull(organizationlogo))
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Please upload an image in png format.")
                                          .Build());
        }

        if (!imageProcessingService.IsImageTypeValid(organizationlogo, AllowedImageTypes))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Incorrect format, please upload an image in png format.")
                                         .Build());
        }

        if (!imageProcessingService.IsImageFileSizeValid(organizationlogo, MaxAllowedImageSize))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded file should be smaller than 5 mb.")
                                         .Build());
        }

        if (!imageProcessingService.IsImageResolutionValid(organizationlogo, MinWidthAndHeight, MinWidthAndHeight,
                                                                             MaxWidthAndHeight, MaxWidthAndHeight))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded image should have a minimum resolution of 96x96 and a maximum resolution of 1024x1024.")
                                         .Build());
        }

        var orgImage = await organizationService.GetOrganizationImage(organizationId, isPrimary);
        if (orgImage == default || orgImage == null)
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("There is no organization Image for given organization Id")
                                         .Build());
        }

        var compressedLogo = imageProcessingService.CompressImage(organizationlogo);
        var response = await organizationService.UpdateOrganizationImage(orgImage.OrganizationImageId.ToString(), compressedLogo);
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Insert operation confronted with errors.")
                                          .Build());
        }
        return Ok();
    }

    [HttpGet("api/v{version:apiVersion}/organizationImage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> GetOrganizationImage
    (
        [FromServices] IOrganizationService organizationService,
        [FromQuery] int organizationId,
        [FromQuery] bool isPrimary
    )
    {
        var organizationImage = await organizationService.GetOrganizationImage(organizationId, isPrimary);
        if (organizationImage == null || organizationImage == default)
        {
            return BadRequest(ErrorBuilder.New()
                                            .SetMessage("There is no organization Image for given organization Id")
                                            .Build());
        }
        var response = await organizationService.GetOrganizationImageFromS3(organizationImage.OrganizationImageId.ToString());

        if (!response.Error.IsNullOrEmpty())
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage(response.Error)
                                         .Build());
        }

        return File(response.Stream, response.ContentType);
    }

    [HttpGet("api/v{version:apiVersion}/isSubscribedToInboxMarketing")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> IsSubscribedToInboxMarketing(
        [FromServices] IApplicationService applicationService,
        [FromServices] IOrganizationMemberService organizationMemberService,
        int organizationId) {
        var isUserAdmin = organizationMemberService.GetAdminOrganizationMembersByOrganizationId(organizationId)
                                                      .FirstOrDefault(om => om.MemberId == CurrentUser.MemberId);
        if (isUserAdmin == null || isUserAdmin == default)
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Inbox Marketing only applies to administrators.")
                                          .Build());
        }
        if (!CurrentUser.IsUserSender())
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Inbox Marketing only applies to senders.")
                                          .Build());
        }
        return Ok(await applicationService.IsSubscribedToInboxMarketingAsync(organizationId));
    }

    [HttpPost("api/v{version:apiVersion}/updateInboxMarketingSubscription")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateInboxMarketingSubscription(
        [FromServices] IApplicationService applicationService,
        [FromServices] IOrganizationMemberService organizationMemberService,
        int organizationId, bool active) {
        var isUserAdmin = organizationMemberService.GetAdminOrganizationMembersByOrganizationId(organizationId)
                                                      .FirstOrDefault(om => om.MemberId == CurrentUser.MemberId);
        if (isUserAdmin == null || isUserAdmin == default)
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Inbox Marketing only applies to administrators.")
                                          .Build());
        }
        if (!CurrentUser.IsUserSender())
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Inbox Marketing only applies to senders.")
                                          .Build());
        }
        return Ok(await applicationService.UpdateInboxMarketingSubscriptionAsync(organizationId, active));
    }

    [HttpGet("api/v{version:apiVersion}/senderOrganizationImage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> GetSenderOrganizationImage
    (
        [FromServices] IOrganizationService organizationService,
        [FromQuery] int organizationId
    )
    {
        var hasSubscription = await organizationService.IsSubscribedToInboxMarketingAsync(organizationId);
        if(!hasSubscription)
        {
            return BadRequest(ErrorBuilder.New()
                                            .SetMessage("There is no inbox marketing subscription for the given organization Id")
                                            .Build());
        }

        var isPrimary = false; // We need the cropped image, not original one
        var organizationImage = await organizationService.GetOrganizationImage(organizationId, isPrimary);
        if (organizationImage == null || organizationImage == default)
        {
            return BadRequest(ErrorBuilder.New()
                                            .SetMessage("There is no organization Image for given organization Id")
                                            .Build());
        }
        var response = await organizationService.GetOrganizationImageFromS3(organizationImage.OrganizationImageId.ToString());

        if (!response.Error.IsNullOrEmpty())
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage(response.Error)
                                         .Build());
        }

        return File(response.Stream, response.ContentType);
    }
}
