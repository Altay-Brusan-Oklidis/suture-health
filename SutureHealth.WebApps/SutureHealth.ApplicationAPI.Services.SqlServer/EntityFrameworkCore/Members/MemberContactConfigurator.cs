using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberContactConfigurator : IEntityTypeConfiguration<MemberContact>
    {
        public void Configure(EntityTypeBuilder<MemberContact> entityBuilder)
        {
            entityBuilder.ToTable("MemberContact")
                     .HasKey(mci => mci.Id);
            entityBuilder.Property(mci => mci.Id).HasColumnName("MemberContactId");
            entityBuilder.Property(oc => oc.ParentId).HasColumnName("MemberId");
            entityBuilder.Property(oc => oc.Type).HasConversion<string>();
        }
    }
}