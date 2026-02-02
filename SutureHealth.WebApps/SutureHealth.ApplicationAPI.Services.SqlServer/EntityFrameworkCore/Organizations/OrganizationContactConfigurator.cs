using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Organizations
{
    public class OrganizationContactConfigurator : IEntityTypeConfiguration<OrganizationContact>
    {
        public void Configure(EntityTypeBuilder<OrganizationContact> entityBuilder)
        {
            entityBuilder.ToTable("OrganizationContact")
                         .HasKey(oc => oc.Id);
            entityBuilder.Property(oc => oc.Id).HasColumnName("OrganizationContactId");
            entityBuilder.Property(oc => oc.ParentId).HasColumnName("OrganizationId");
            entityBuilder.Property(oc => oc.Type).HasConversion<string>();
        }
    }
}