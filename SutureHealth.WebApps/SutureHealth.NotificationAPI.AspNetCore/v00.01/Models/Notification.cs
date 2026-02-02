using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SutureHealth.Notifications.AspNetCore.v0001.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Notification
    {
        [Required]
        public Channel Type { get; set; } = Channel.Email;
        [Required]
        public string Subject { get; set; }
        [Required]
        public string DestinationUri { get; set; }
        public DateTime TerminationDate { get; set; } = DateTime.UtcNow.AddDays(30);
        public string CallbackUrl { get; set; }
        public string SourceUrl { get; set; }
        public string SourceText { get; set; }
        [Required]
        public string SourceId { get; set; }
        public Dictionary<string,object> AdditionalOptions { get; set; }
        public DateTime? DesiredSendDateTime { get; set; }
    }

    public class NotificationStatus : Notification
    {
        public Guid UniqueNotificationId { get; set; }
        public DateTime? NotificationDate { get; set; }
        public bool? Success { get; set; }
        public bool? Complete { get; set; }
    }
}