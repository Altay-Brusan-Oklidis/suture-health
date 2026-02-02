using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberRelationshipConfigurator : IEntityTypeConfiguration<MemberRelationship>
    {
        public void Configure(EntityTypeBuilder<MemberRelationship> entityBuilder)
        {
            entityBuilder.ToTable("MemberRelationship")
                         .HasKey(m => new { m.SupervisorMemberId, m.SubordinateMemberId });
            entityBuilder.HasOne(m => m.Supervisor);
            entityBuilder.HasOne(m => m.Subordinate);
        }
    }
}