using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SutureHealth.Notifications.AspNetCore.v0001.Models;
using SutureHealth.Notifications.Services;

namespace SutureHealth.Notifications.AspNetCore.v0001.Controllers
{
    /// <summary>
    /// Class <see cref="NotificationController"/> used for implementing cloud api for notification. 
    /// </summary>
    [ApiController]
    [ApiVersion("0.1")]
    [Authorize(Policy = AuthorizationPolicies.SutureHealthApplication)]
    [Route("Notification")]
    public class NotificationController : ControllerBase
    {
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(NotificationStatus))]
        [HttpGet("{notificationId:Guid}", Name = "GetNotification")]
        public async Task<IActionResult> GetNotification
        (
            [FromServices] IMapper mapper,
            [FromServices] INotificationService serviceProvider,
            [FromRoute] Guid notificationId
        ) => Ok(mapper.Map<Models.NotificationStatus>(await serviceProvider.GetNotificationByStatusId(notificationId)));

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [AllowAnonymous]
        [HttpPost("{notificationId:Guid}/callback/{providerId:Guid}", Name = "NotificationCallback")]
        public async Task<IActionResult> HandleProviderCallback
        (
            [FromRoute]
            Guid notificationId,
            [FromRoute]
            Guid providerId,
            [FromServices]
            INotificationService servicesProvider
        )
        {
            await servicesProvider.UpdateNotificationStatus(notificationId, providerId, this.Request);
            return Ok();
        }

        [HttpPost("{uniqueNotificationId:Guid}/notify")]
        public async Task<IActionResult> HandleOriginatorsCallback
        (
            [FromRoute]
            Guid uniqueNotificationId,
            [FromServices]
            INotificationService servicesProvider,
            [FromServices]
            IHttpClientFactory httpClientFactory,
            [FromServices]
            ILogger<NotificationController> logger,
            [FromServices]
            IMapper mapper
        )
        {
            try
            {
                var notification = await servicesProvider.GetLatestNotificationById(uniqueNotificationId);
                var http = httpClientFactory.CreateClient();
                var content = new StringContent(JsonSerializer.Serialize(mapper.Map<Models.Notification>(notification)), System.Text.Encoding.UTF8, "application/json");
                var result = await http.PostAsync(notification.CallbackUrl, content);

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Encountered error when notifing originating caller of notification ({uniqueNotificationId}) state change.");
                return this.Problem();
            }
        }

        [ProducesResponseType((int)HttpStatusCode.Accepted, Type = typeof(NotificationStatus))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost(Name = "CreateNotification")]
        public async Task<IActionResult> Post
        (
            [FromServices]
            INotificationService servicesProvider,
            [FromServices]
            IMapper mapper,
            [FromBody]
            Notification notification
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide a valid notification");
            }

            var result = await servicesProvider.CreateNotificationAsync
            (
                notification.Type,
                notification.Subject,
                notification.DestinationUri,
                notification.TerminationDate,
                notification.DesiredSendDateTime,
                Uri.TryCreate(notification.SourceUrl, UriKind.Absolute, out Uri sourceUrl) ? sourceUrl : null,
                notification.SourceText,
                Uri.TryCreate(notification.CallbackUrl, UriKind.Absolute, out Uri callbackUrl) ? callbackUrl : null,
                notification.SourceId,
                notification.AdditionalOptions
            );

            return Accepted(mapper.Map<Models.NotificationStatus>(result));
        }
    }
}
