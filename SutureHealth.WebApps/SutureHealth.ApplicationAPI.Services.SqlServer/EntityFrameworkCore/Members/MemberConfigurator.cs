using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberConfigurator : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> entityBuilder)
        {
            entityBuilder.ToTable("Member")
                         .HasKey(m => m.MemberId);
            entityBuilder.Property(p => p.MemberId)
                         .HasColumnName("MemberId");

            entityBuilder.HasMany(p => p.Contacts)
                         .WithOne(p => p.Parent)
                         .HasForeignKey(p => p.ParentId);
        }
    }
}