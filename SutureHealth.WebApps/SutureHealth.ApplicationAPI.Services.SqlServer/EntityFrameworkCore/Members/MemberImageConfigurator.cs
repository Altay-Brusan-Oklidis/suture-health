using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberImageConfigurator : IEntityTypeConfiguration<MemberImage>
    {
        public void Configure(EntityTypeBuilder<MemberImage> entityBuilder)
        {
            entityBuilder.ToTable("MemberImage")
                         .HasKey(m => m.MemberImageId);
        }
    }
}