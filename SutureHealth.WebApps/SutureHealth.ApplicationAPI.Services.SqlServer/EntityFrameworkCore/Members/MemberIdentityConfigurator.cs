using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SutureHealth.Application;

namespace SutureHealth.Application.EntityFrameworkCore.Members
{
    public class MemberIdentityConfigurator : IEntityTypeConfiguration<MemberIdentity>
    {
        public void Configure(EntityTypeBuilder<MemberIdentity> entityBuilder)
        {
            entityBuilder.ToTable("MemberIdentity")
                         .HasKey(m => m.MemberId);
            entityBuilder.Ignore(m => m.CurrentOrganizationId);
        }
    }

    public class PublicIdentityConfiguration : IEntityTypeConfiguration<PublicIdentity>
    {
        public void Configure(EntityTypeBuilder<PublicIdentity> entityBuilder)
        {
            entityBuilder.ToTable("MemberPublicIdentity")
                         .HasKey(m => m.PublicIdentityId);
            entityBuilder.Property(m => m.UseType)
                         .HasConversion(new EnumToStringConverter<IdentityUseType>());
            entityBuilder.Property(m => m.PublicIdentityId)
                         .HasColumnName("MemberPublicIdentityId");
            entityBuilder.Property(m => m.Value)
                         .HasColumnName("PublicIdentity");
        }
    }
}