using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace SutureHealth.Application.EntityFrameworkCore.Organizations
{
    public class OrganizationImageConfigurator : IEntityTypeConfiguration<OrganizationImage>
    {
        public void Configure(EntityTypeBuilder<OrganizationImage> entityBuilder)
        {
            entityBuilder.ToTable("OrganizationImage")
                         .HasKey(m => m.OrganizationImageId);
        }
    }
}
