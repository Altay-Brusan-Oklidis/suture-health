using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Organizations
{
    public class OrganizationTypeConfigurator : IEntityTypeConfiguration<OrganizationType>
    {
        public void Configure(EntityTypeBuilder<OrganizationType> entityBuilder)
        {
            entityBuilder.ToTable("OrganizationType")
                         .HasKey(ot => ot.OrganizationTypeId);
        }
    }
}