using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Organizations
{
    public class OrganizationSettingConfigurator : IEntityTypeConfiguration<OrganizationSetting>
    {
        public void Configure(EntityTypeBuilder<OrganizationSetting> entityBuilder)
        {
            entityBuilder.ToTable("OrganizationSetting")
                         .HasKey(s => s.SettingId);
        }
    }
}