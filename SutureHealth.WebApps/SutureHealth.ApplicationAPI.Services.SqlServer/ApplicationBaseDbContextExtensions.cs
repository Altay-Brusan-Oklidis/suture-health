using Microsoft.EntityFrameworkCore;
using SutureHealth.Application.EntityFrameworkCore;
using SutureHealth.Application.EntityFrameworkCore.FeatureFlags;
using SutureHealth.Application.EntityFrameworkCore.FhirUserConflation;
using SutureHealth.Application.EntityFrameworkCore.Integrators;
using SutureHealth.Application.EntityFrameworkCore.Members;
using SutureHealth.Application.EntityFrameworkCore.Organizations;
using SutureHealth.Application.EntityFrameworkCore.Organizations.BillableEntities;

namespace SutureHealth.Application
{
    public static class ApplicationBaseDbContextExtensions
    {
        public static ModelBuilder ApplyApplicationBaseDbContextConfigurations(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ApplicationSettingConfigurator());
            modelBuilder.ApplyConfiguration(new IntegratorConfigurator());
            modelBuilder.ApplyConfiguration(new IntegratorOrganizationConfigurator());
            modelBuilder.ApplyConfiguration(new MemberConfigurator());
            modelBuilder.ApplyConfiguration(new MemberIdentityConfigurator());
            modelBuilder.ApplyConfiguration(new MemberContactConfigurator());
            modelBuilder.ApplyConfiguration(new MemberSettingConfigurator());
            modelBuilder.ApplyConfiguration(new MemberImageConfigurator());
            modelBuilder.ApplyConfiguration(new OrganizationConfigurator());
            modelBuilder.ApplyConfiguration(new OrganizationContactConfigurator());
            modelBuilder.ApplyConfiguration(new OrganizationMemberConfigurator());
            modelBuilder.ApplyConfiguration(new MemberRelationshipConfigurator());
            modelBuilder.ApplyConfiguration(new OrganizationSettingConfigurator());
            modelBuilder.ApplyConfiguration(new OrganizationImageConfigurator());
            modelBuilder.ApplyConfiguration(new BillableEntityConfigurator());
            modelBuilder.ApplyConfiguration(new FeatureFlagAuditConfigurator());
            modelBuilder.ApplyConfiguration(new FeatureFlagConfigurator());
            modelBuilder.ApplyConfiguration(new FeatureFlagsUsersConfigurator());
            modelBuilder.ApplyConfiguration(new FhirUserConflationConfigurator());
            return modelBuilder;
        }
    }
}
