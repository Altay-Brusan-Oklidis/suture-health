using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application.EntityFrameworkCore.FeatureFlags
{
    public class FeatureFlagAuditConfigurator : IEntityTypeConfiguration<FeatureFlagAudit>
    {
        public void Configure(EntityTypeBuilder<FeatureFlagAudit> entityBuilder)
        {
            // Primary Key
            entityBuilder.HasKey(e => e.Id);

            // Foreign Key Relationship with FeatureFlags table
            entityBuilder
                .HasOne(e => e.FeatureFlag)
                .WithMany()
                .HasForeignKey(e => e.FeatureFlagId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Property Configurations
            entityBuilder.Property(e => e.CreateDate);
            entityBuilder.Property(e => e.OldActive);
            entityBuilder.Property(e => e.NewActive);
            entityBuilder.Property(e => e.OldHasCohort);
            entityBuilder.Property(e => e.NewHasCohort);
        }
    }
}
