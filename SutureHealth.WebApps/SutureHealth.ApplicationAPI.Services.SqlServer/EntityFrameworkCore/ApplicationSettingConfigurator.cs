using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore
{
    public class ApplicationSettingConfigurator : IEntityTypeConfiguration<ApplicationSetting>
    {
        public void Configure(EntityTypeBuilder<ApplicationSetting> entityBuilder)
        {
            entityBuilder.ToTable("ApplicationSetting")
                         .HasKey(s => s.SettingId);
            entityBuilder.Ignore(s => s.ParentId);
        }
    }
}
