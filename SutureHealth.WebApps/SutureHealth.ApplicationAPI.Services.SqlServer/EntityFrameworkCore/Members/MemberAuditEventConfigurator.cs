using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberAuditEventConfigurator : IEntityTypeConfiguration<MemberAuditEvent>
    {
        protected bool IsDemo { get; }

        public MemberAuditEventConfigurator(string environmentName = null)
            => IsDemo = string.Equals(environmentName, "demo", System.StringComparison.OrdinalIgnoreCase);

        public void Configure(EntityTypeBuilder<MemberAuditEvent> entityBuilder)
        {
            entityBuilder.ToTable("MemberAuditEvent", builder =>
            {
                if (IsDemo)
                {
                    builder.HasTrigger("TR_MemberAuditEvent_UnSignDocuments_I");
                }
            })
                         .HasKey(mae => mae.MemberAuditEventId);
            entityBuilder.Property(mae => mae.MemberAuditEventId)
                         .IsRequired()
                         .UseIdentityColumn();

        }
    }
}