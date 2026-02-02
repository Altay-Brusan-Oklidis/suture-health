using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore
{
    public class HierarchicalSettingConfigurator : IEntityTypeConfiguration<HierarchicalSetting>
    {
        public void Configure(EntityTypeBuilder<HierarchicalSetting> builder)
        {
            builder.HasNoKey();
        }
    }
}
