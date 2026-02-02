using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberTokenConfigurator : IEntityTypeConfiguration<MemberToken>
    {
        public void Configure(EntityTypeBuilder<MemberToken> entityBuilder)
        {
            entityBuilder.ToTable("MemberToken")
                         .HasKey(s => s.MemberTokenId);
        }
    }
}