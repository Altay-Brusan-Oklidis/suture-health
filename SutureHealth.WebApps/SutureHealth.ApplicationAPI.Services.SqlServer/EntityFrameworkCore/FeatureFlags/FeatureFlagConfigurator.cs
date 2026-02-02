using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application.EntityFrameworkCore.FeatureFlags
{
    public class FeatureFlagConfigurator : IEntityTypeConfiguration<FeatureFlag>
    {
        public void Configure(EntityTypeBuilder<FeatureFlag> entityBuilder)
        {

            // Primary Key
            entityBuilder.HasKey(e => e.Id);

            // Property Configurations
            entityBuilder.Property(e => e.Name).HasMaxLength(255);
            entityBuilder.Property(e => e.Description).HasColumnType("TEXT");
            entityBuilder.Property(e => e.Active);
            entityBuilder.Property(e => e.CreateDate);
            entityBuilder.Property(e => e.UpdateDate);
            entityBuilder.Property(e => e.HasCohort);
        }
    }
}
