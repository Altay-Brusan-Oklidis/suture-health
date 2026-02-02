using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SutureHealth.Application;
namespace SutureHealth.Application.EntityFrameworkCore.Integrators
{
    public class IntegratorConfigurator : IEntityTypeConfiguration<Integrator>
    {
        public void Configure(EntityTypeBuilder<Integrator> entityBuilder)
        {
            entityBuilder.ToTable("Integrator")
                         .HasKey(i => i.IntegratorId);
        }
    }
}