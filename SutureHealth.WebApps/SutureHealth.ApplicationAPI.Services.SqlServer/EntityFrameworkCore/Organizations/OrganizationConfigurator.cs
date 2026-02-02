using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Organizations
{
    public class OrganizationConfigurator : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> entityBuilder)
        {
            entityBuilder.ToTable("Organization")
                         .HasKey(o => o.OrganizationId);
            entityBuilder.HasMany(p => p.Contacts)
                         .WithOne(p => p.Parent)
                         .HasForeignKey(p => p.ParentId);
            entityBuilder.HasOne(p => p.OrganizationType)
                         .WithMany()
                         .HasForeignKey(p => p.OrganizationTypeId);
        }
    }
}