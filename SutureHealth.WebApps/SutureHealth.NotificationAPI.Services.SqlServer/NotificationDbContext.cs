using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using SutureHealth.Notifications;
using System.Threading.Tasks;

namespace SutureHealth.Notifications.Services
{
    public partial class SqlServerNotificationDbContext : NotificationDbContext
    {
        public SqlServerNotificationDbContext(DbContextOptions<SqlServerNotificationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NotificationStatus>()
                .ToTable("Notification")
                .Ignore(x => x.AdditionalOptions);
            modelBuilder.Entity<NotificationStatus>()
                        .Property(x => x.Channel)
                        .HasColumnName("Type");
        }
    }
}
