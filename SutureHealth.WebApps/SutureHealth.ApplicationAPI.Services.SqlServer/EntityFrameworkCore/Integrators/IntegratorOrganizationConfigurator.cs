using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Integrators
{
    public class IntegratorOrganizationConfigurator : IEntityTypeConfiguration<IntegratorOrganization>
    {
        public void Configure(EntityTypeBuilder<IntegratorOrganization> entityBuilder)
        {
            entityBuilder.ToTable("IntegratorOrganization")
                         .HasKey(io => io.IntegratorOrganizationId);
        }
    }
}