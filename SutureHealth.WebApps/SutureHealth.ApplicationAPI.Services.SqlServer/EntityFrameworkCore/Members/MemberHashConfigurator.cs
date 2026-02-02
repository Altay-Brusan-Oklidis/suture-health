using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberHashConfigurator : IEntityTypeConfiguration<MemberHash>
    {
        public void Configure(EntityTypeBuilder<MemberHash> entityBuilder)
        {
            entityBuilder.ToTable("MemberHash")
                         .HasKey(m => m.MemberHashId);
        }
    }
}