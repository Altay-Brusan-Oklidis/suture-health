using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Organizations.BillableEntities;

public class BillableEntityConfigurator : IEntityTypeConfiguration<BillableEntity>
{
    public void Configure(EntityTypeBuilder<BillableEntity> entityBuilder)
    {
        entityBuilder.HasKey(e => new { e.BillableEntityId, e.SystemServiceId });
        entityBuilder.ToTable("SystemBillableEntity");
    }
}


