using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Organizations
{
    public class OrganizationMemberConfigurator : IEntityTypeConfiguration<OrganizationMember>
    {
        public void Configure(EntityTypeBuilder<OrganizationMember> entityBuilder)
        {
            entityBuilder.ToTable("OrganizationMember")
                         .HasKey(x => x.OrganizationMemberId);
        }
    }
}