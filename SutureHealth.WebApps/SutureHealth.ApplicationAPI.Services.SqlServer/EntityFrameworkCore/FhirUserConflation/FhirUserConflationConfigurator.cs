using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Application.EntityFrameworkCore.FhirUserConflation;

public class FhirUserConflationConfigurator : IEntityTypeConfiguration<Application.FhirUserConflation>
{
    public void Configure(EntityTypeBuilder<Application.FhirUserConflation> builder)
    {
        builder.ToTable("FhirUsers");
    }
}