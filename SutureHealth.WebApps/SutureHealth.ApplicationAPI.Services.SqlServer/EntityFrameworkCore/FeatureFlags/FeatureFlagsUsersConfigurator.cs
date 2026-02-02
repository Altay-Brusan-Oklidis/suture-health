using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application.EntityFrameworkCore.FeatureFlags
{
    public class FeatureFlagsUsersConfigurator : IEntityTypeConfiguration<FeatureFlagsUsers>
    {
        public void Configure(EntityTypeBuilder<FeatureFlagsUsers> entityBuilder)
        {
            // Primary Key
            entityBuilder.HasKey(e => e.Id);

            // Foreign Key Relationships
            entityBuilder
                .HasOne(e => e.FeatureFlag)
                .WithMany()
                .HasForeignKey(e => e.FeatureFlagId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Property Configurations
            entityBuilder.Property(e => e.CreateDate);
        }
    }
}
