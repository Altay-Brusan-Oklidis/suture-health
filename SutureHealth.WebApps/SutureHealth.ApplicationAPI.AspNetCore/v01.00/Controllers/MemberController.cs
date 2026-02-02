using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using AutoMapper;
using ProducesResponseTypeAttribute = SutureHealth.AspNetCore.Mvc.ProducesResponseTypeAttribute;
using SutureHealth.Application.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using SutureHealth.AspNetCore.SwaggerGen.Attributes;
using SutureHealth.Application.Extensions;


namespace SutureHealth.Application.v0100.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
[ControllerName("ApiMember")]
public class MemberController : SutureHealth.AspNetCore.Mvc.ControllerBase
{
    private static readonly List<ImageFormat> AllowedImageTypes = new() { ImageFormat.Jpeg, ImageFormat.Jpg, ImageFormat.Png };
    private static readonly List<ImageFormat> AllowedImageTypesMobile = new() { ImageFormat.Jpeg, ImageFormat.Jpg, ImageFormat.Png, ImageFormat.Heif, ImageFormat.Heic };
    private static readonly int MaxAllowedImageSize = 5242880; // 5MB = 5 * 1024 * 1024 = 5242880
    private static readonly int MinWidthAndHeight = 96;
    private static readonly int MaxWidthAndHeight = 1024;

    private static readonly int SmallImageSize = 32;
    private static readonly int MediumImageSize = 64;
    private static readonly int LargeImageSize = 128;

    [HttpGet("api/v{version:apiVersion}/me")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(Models.Member))]
    public async Task<IActionResult> Me
    (
       [FromServices] IApplicationService applicationService,
       [FromServices] IMapper mapper
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }
        else
        {
            var relationshipQuery = applicationService.GetMemberRelationships();
            var user = mapper.Map<Models.Member>(CurrentUser, opt =>
                opt.Items["MemberRelationship"] = relationshipQuery);

            return Ok(user);
        }
    }

    [HttpPost("api/v{version:apiVersion}/memberImage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    [FeatureFlag("UserMenu-AvatarUpload", HttpStatusCode.Forbidden)]
    public async Task<IActionResult> CreateMemberImage
    (
        [FromServices] IMemberService memberService,
        [FromServices] IImageProcessingService imageProcessingService,
        IFormFile memberImage,
        [FromQuery] bool isPrimary,
        [FromQuery] bool isMobile = false
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }
        if (imageProcessingService.IsImageFileEmptyOrNull(memberImage))
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Please upload an image in jpg, jpeg or png format.")
                                          .Build());
        }

        if (isMobile && !imageProcessingService.IsImageTypeValid(memberImage, AllowedImageTypesMobile))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Incorrect format, please upload a image in jpg, jpeg, png, heif or heic format.")
                                         .Build());
        }
        if (!isMobile && !imageProcessingService.IsImageTypeValid(memberImage, AllowedImageTypes))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Incorrect format, please upload a image in jpg, jpeg or png format.")
                                         .Build());
        }

        if (!imageProcessingService.IsImageFileSizeValid(memberImage, MaxAllowedImageSize))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded file should be smaller than 5 mb.")
                                         .Build());
        }


        if (!isMobile! && !imageProcessingService.IsImageResolutionValid(memberImage, MinWidthAndHeight, MinWidthAndHeight,
                                                                                     MaxWidthAndHeight, MaxWidthAndHeight))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded image should have a minimum resolution of 96x96 and a maximum resolution of 1024x1024.")
                                         .Build());
        }

        if (isMobile && !imageProcessingService.IsImageResolutionValidForMobile(memberImage, MinWidthAndHeight, MinWidthAndHeight))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded image should have a minimum resolution of 96x96.")
                                         .Build());
        }

        var memberImages = await memberService.GetMemberImages(CurrentUser.MemberId);

        if (isPrimary && memberImages != null && memberImages.Any(mi => mi.IsPrimary))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("You can not create more than one primary member image for same member")
                                         .Build());
        }

        if (!isPrimary && memberImages != null && memberImages.Any(mi => !mi.IsPrimary))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("You can not create more than one compressed image for same member")
                                         .Build());
        }

        var imagesToBeCreated = new List<MemberImage>();

        if (isPrimary)
        {
            imagesToBeCreated.Add(new MemberImage()
            {
                MemberImageId = Guid.NewGuid(),
                MemberId = CurrentUser.MemberId,
                IsPrimary = true,
                SizeType = MemberImageSizeType.Original,
                Active = true,
                UploadDate = DateTime.UtcNow
            });
        }
        else
        {
            imagesToBeCreated.Add(new MemberImage()
            {
                MemberImageId = Guid.NewGuid(),
                MemberId = CurrentUser.MemberId,
                IsPrimary = false,
                SizeType = MemberImageSizeType.Cropped,
                Active = true,
                UploadDate = DateTime.UtcNow
            });
            imagesToBeCreated.Add(new MemberImage()
            {
                MemberImageId = Guid.NewGuid(),
                MemberId = CurrentUser.MemberId,
                IsPrimary = false,
                SizeType = MemberImageSizeType.Small,
                Active = true,
                UploadDate = DateTime.UtcNow
            });
        }

        await memberService.CreateMemberImages(imagesToBeCreated.ToArray());

        foreach (var image in imagesToBeCreated)
        {
            IFormFile newImg = null;

            switch (image.SizeType)
            {
                case MemberImageSizeType.Original:
                    newImg = imageProcessingService.CompressImage(memberImage);
                    break;
                case MemberImageSizeType.Cropped:
                    newImg = imageProcessingService.CompressImage(memberImage);
                    break;
                case MemberImageSizeType.Small:
                    newImg = imageProcessingService.CompressImage(memberImage, SmallImageSize, SmallImageSize);
                    break;
            }
            var response = await memberService.InsertMemberImageToS3(image.MemberImageId.ToString(), newImg);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return BadRequest(ErrorBuilder.New()
                                              .SetMessage("Insert operation confronted with errors.")
                                              .Build());
            }
        }

        return Ok();
    }

    [HttpDelete("api/v{version:apiVersion}/memberImage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    [FeatureFlag("UserMenu-AvatarUpload", HttpStatusCode.Forbidden)]
    public async Task<IActionResult> DeleteMemberImage
    (
        [FromServices] IMemberService memberService
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }
        var memImages = (await memberService.GetMemberImages(CurrentUser.MemberId)).ToArray();
        if (memImages == null || memImages.Count() == 0)
        {
            return BadRequest(ErrorBuilder.New()
                                        .SetMessage("There is no member Image for given member Id")
                                        .Build());
        }

        await memberService.DeleteMemberImages(memImages);
        foreach (var memImage in memImages)
        {
            await memberService.DeleteMemberImageFromS3(memImage.MemberImageId.ToString());
        }

        return Ok();
    }

    [HttpPut("api/v{version:apiVersion}/memberImage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    [FeatureFlag("UserMenu-AvatarUpload", HttpStatusCode.Forbidden)]
    public async Task<IActionResult> UpdateMemberImage
    (
        [FromServices] IMemberService memberService,
        [FromServices] IImageProcessingService imageProcessingService,
        IFormFile memberImage,
        [FromQuery] bool isPrimary,
        [FromQuery] bool isMobile = false
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }
        if (imageProcessingService.IsImageFileEmptyOrNull(memberImage))
        {
            return BadRequest(ErrorBuilder.New()
                                          .SetMessage("Please upload an image in jpg, jpeg or png format.")
                                          .Build());
        }
        if (isMobile && !imageProcessingService.IsImageTypeValid(memberImage, AllowedImageTypesMobile))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Incorrect format, please upload a image in jpg, jpeg, png, heif or heic format.")
                                         .Build());
        }
        if (!isMobile && !imageProcessingService.IsImageTypeValid(memberImage, AllowedImageTypes))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Incorrect format, please upload an image in jpg, jpeg or png format.")
                                         .Build());
        }

        if (!imageProcessingService.IsImageFileSizeValid(memberImage, MaxAllowedImageSize))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded file should be smaller than 5 mb.")
                                         .Build());
        }

        if (!isMobile && !imageProcessingService.IsImageResolutionValid(memberImage, MinWidthAndHeight, MinWidthAndHeight,
                                                                                     MaxWidthAndHeight, MaxWidthAndHeight))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded image should have a minimum resolution of 96x96 and a maximum resolution of 1024x1024.")
                                         .Build());
        }

        if (isMobile && !imageProcessingService.IsImageResolutionValidForMobile(memberImage, MinWidthAndHeight, MinWidthAndHeight))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("Uploaded image should have a minimum resolution of 96x96.")
                                         .Build());
        }

        var memberImages = await memberService.GetMemberImages(CurrentUser.MemberId);
        if (!memberImages.Any(mi => mi.IsPrimary == isPrimary))
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage("There is no member Image for given member Id")
                                         .Build());
        }


        MemberImage[] imagesToBeDeleted = null;
        var imagesToBeCreated = new List<MemberImage>();
        if (isPrimary)
        {
            imagesToBeDeleted = memberImages.Where(mi => mi.IsPrimary == true).ToArray();
            imagesToBeCreated.Add(new MemberImage()
            {
                MemberId = CurrentUser.MemberId,
                IsPrimary = true,
                SizeType = MemberImageSizeType.Original,
                Active = true,
                MemberImageId = Guid.NewGuid(),
                UploadDate = DateTime.UtcNow
            });
        }
        else
        {
            imagesToBeDeleted = memberImages.Where(mi => mi.IsPrimary == false).ToArray();
            imagesToBeCreated.Add(new MemberImage()
            {
                MemberId = CurrentUser.MemberId,
                IsPrimary = false,
                SizeType = MemberImageSizeType.Cropped,
                Active = true,
                MemberImageId = Guid.NewGuid(),
                UploadDate = DateTime.UtcNow
            });
            imagesToBeCreated.Add(new MemberImage()
            {
                MemberId = CurrentUser.MemberId,
                IsPrimary = false,
                SizeType = MemberImageSizeType.Small,
                Active = true,
                MemberImageId = Guid.NewGuid(),
                UploadDate = DateTime.UtcNow
            });
        }


        await memberService.DeleteMemberImages(imagesToBeDeleted);
        await memberService.CreateMemberImages(imagesToBeCreated.ToArray());

        foreach (var image in imagesToBeCreated)
        {
            IFormFile newImg = null;

            switch (image.SizeType)
            {
                case MemberImageSizeType.Original:
                    newImg = imageProcessingService.CompressImage(memberImage);
                    break;
                case MemberImageSizeType.Cropped:
                    newImg = imageProcessingService.CompressImage(memberImage);
                    break;
                case MemberImageSizeType.Small:
                    newImg = imageProcessingService.CompressImage(memberImage, SmallImageSize, SmallImageSize);
                    break;
            }

            var response = await memberService.InsertMemberImageToS3(image.MemberImageId.ToString(), newImg);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return BadRequest(ErrorBuilder.New()
                                              .SetMessage("Insert operation confronted with errors.")
                                              .Build());
            }
        }

        return Ok();
    }

    [HttpGet("api/v{version:apiVersion}/memberImage")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> GetMemberImage
    (
        [FromServices] IMemberService memberService,
        [FromQuery] bool isPrimary
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }

        var isOriginal = isPrimary ? MemberImageSizeType.Original : MemberImageSizeType.Cropped;
        var memberImage = await memberService.GetMemberImage(CurrentUser.MemberId, isPrimary, isOriginal);
        if (memberImage == null || memberImage == default)
        {
            return BadRequest(ErrorBuilder.New()
                                            .SetMessage("There is no member Image for given member Id")
                                            .Build());
        }
        var response = await memberService.GetMemberImageFromS3(memberImage.MemberImageId.ToString());

        if (!response.Error.IsNullOrEmpty())
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage(response.Error)
                                         .Build());
        }

        return File(response.Stream, response.ContentType);
    }

    [HttpGet("api/v{version:apiVersion}/memberAvatar")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> GetMemberAvatar
    (
      [FromServices] IMemberService memberService,
      [FromQuery] string imageGuid
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }
        var response = await memberService.GetMemberImageFromS3(imageGuid.ToLower());

        if (!response.Error.IsNullOrEmpty())
        {
            return BadRequest(ErrorBuilder.New()
                                         .SetMessage(response.Error)
                                         .Build());
        }

        Response.Headers.Add("Cache-Control", "public, max-age=3600");

        return File(response.Stream, response.ContentType);
    }

    [HttpGet("api/v{version:apiVersion}/relationalMembers")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> RelationalMembers
    (
        [FromServices] IMemberService memberService,
        [FromServices] IMapper mapper
    )
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }

        var relationalMembers = mapper.Map<List<Models.Member>>(memberService.GetMemberRelationships()
                                                            .Where(mr => (mr.SubordinateMemberId == CurrentUser.MemberId ||
                                                                        mr.SupervisorMemberId == CurrentUser.MemberId) &&
                                                                        mr.IsActive == true).AsEnumerable()
                                                                        .SelectMany(mr => new[] { mr.Subordinate, mr.Supervisor })
                                                                        .Distinct(new MemberIdEqualityComparer()));

        foreach (var member in relationalMembers)
        {
            var membersExceptCurrent = relationalMembers.Where(m => m != member);

            if (membersExceptCurrent.Count(m => (m.MemberTypeId == 2001 ||
                                                 m.MemberTypeId == 2008)) == 1)
            {
                member.HasSingleCollaborator = true;
            }
            if (membersExceptCurrent.Count(m => (m.MemberTypeId == 2000 ||
                                                 m.MemberTypeId == 2001 ||
                                                 m.MemberTypeId == 2002 ||
                                                 m.MemberTypeId == 2008 ||
                                                 m.MemberTypeId == 2012 ||
                                                 m.MemberTypeId == 2014 ||
                                                 m.MemberTypeId == 2015)) == 1)
            {
                member.HasSingleSigner = true;
            }
        }

        return Ok(relationalMembers);
    }
}