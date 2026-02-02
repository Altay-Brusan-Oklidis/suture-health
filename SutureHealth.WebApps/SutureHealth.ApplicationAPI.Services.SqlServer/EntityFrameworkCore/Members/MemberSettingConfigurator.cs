using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberSettingConfigurator : IEntityTypeConfiguration<MemberSetting>
    {
        public void Configure(EntityTypeBuilder<MemberSetting> entityBuilder)
        {
            entityBuilder.ToTable("MemberSetting")
                         .HasKey(s => s.SettingId);
        }
    }
}