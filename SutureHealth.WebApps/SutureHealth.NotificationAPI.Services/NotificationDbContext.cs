using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Notifications.Services
{
    public abstract partial class NotificationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<NotificationStatus> Notification { get; set; }

        public NotificationDbContext(DbContextOptions options) : base(options) { }
    }
}