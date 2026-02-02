using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Kendo.Mvc;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Notifications.Services;
using SutureHealth.AspNetCore.Mvc.Extensions;
using Channel = SutureHealth.Notifications.Channel;

namespace SutureHealth.AspNetCore.Areas.Admin.Pages
{
    public class NotificationStatus
    {
        public long NotificationId { get; set; }
        public Guid Id { get; set; }
        public Channel Channel { get; set; }
        public string ChannelText { get; set; }
        public string DestinationUri { get; set; }
        public DateTime OriginationDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public string StatusCode { get; set; }
        public bool? Success { get; set; }
        public bool? Complete { get; set; }
        public string Subject { get; set; }
        public DateTime? NotificationDate { get; set; }
        public DateTime? DesiredSendDateTime { get; set; }
        public DateTime? SendDateTime { get; set; }
        public string ProviderExternalKey { get; set; }
        public string DetailUrl { get; set; }
    }

    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class NotificationsModel : BasePageModel
    {
        protected IApplicationService ApplicationService { get; init; }
        protected ILogger Logger { get; init; }
        protected INotificationService NotificationService { get; init; }
        protected IServiceProvider ServiceProvider { get; init; }

        public IEnumerable<KeyValuePair<int, string>> Channels { get; } = Enum.GetValues<Channel>().Select(c => new KeyValuePair<int, string>((int)c, c.ToString()));

        public NotificationsModel
        (
            ILogger<NotificationsModel> logger,
            IApplicationService applicationService,
            INotificationService notificationServices,
            IServiceProvider serviceProvider
        )
        {
            ApplicationService = applicationService;
            Logger = logger;
            NotificationService = notificationServices;
            ServiceProvider = serviceProvider;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost([DataSourceRequest] DataSourceRequest request, CancellationToken cancellationToken)
        {
            var query = NotificationService.GetNotifications();

            if (request.Filters.GetFilterDescriptor("Complete") is FilterDescriptor completeFilterDescriptor)
            {
                request.Filters.Remove("Complete");
                if ((bool)completeFilterDescriptor.Value)
                {
                    query = query.Where(n => n.Complete == true);
                }
                else
                {
                    query = query.Where(n => n.Complete == null || n.Complete == false);
                }
            }
            if (request.Filters.GetFilterDescriptor("Success") is FilterDescriptor successFilterDescriptor)
            {
                request.Filters.Remove("Success");
                if ((bool)successFilterDescriptor.Value)
                {
                    query = query.Where(n => n.Success == true);
                }
                else
                {
                    query = query.Where(n => n.Success == null || n.Success == false);
                }
            }

            var data = await query.Where(request.Filters)
                                  .Sort(request.Sorts)
                                  .Page(request.Page - 1, request.PageSize)
                                  .Cast<SutureHealth.Notifications.NotificationStatus>()
                                  .Select(n => new NotificationStatus
                                  {
                                      NotificationId = n.NotificationId,
                                      Id = n.Id,
                                      Channel = n.Channel,
                                      ChannelText = n.Channel.ToString(),
                                      DestinationUri = n.DestinationUri,
                                      OriginationDate = n.OriginationDate,
                                      TerminationDate = n.TerminationDate,
                                      StatusCode = n.StatusCode,
                                      Success = n.Success.GetValueOrDefault(false),
                                      Complete = n.Complete.GetValueOrDefault(false),
                                      Subject = n.Subject,
                                      NotificationDate = n.NotificationDate,
                                      DesiredSendDateTime = n.DesiredSendDateTime,
                                      SendDateTime = n.SendDateTime,
                                      ProviderExternalKey = n.ProviderExternalKey,
                                      DetailUrl = Url.RouteUrl("AdminNotificationDetail", new { notificationId = n.NotificationId })
                                  })
                                  .ToArrayAsync(cancellationToken);
            var result = new DataSourceResult
            {
                Data = data,
                Total = await query.Where(request.Filters)
                                   .Cast<SutureHealth.Notifications.NotificationStatus>()
                                   .CountAsync(cancellationToken)
            };

            return new JsonResult(result);
        }
    }
}
