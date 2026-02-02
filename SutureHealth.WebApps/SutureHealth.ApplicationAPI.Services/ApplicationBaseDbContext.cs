using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Application.Services
{
    public abstract class ApplicationBaseDbContext : DbContext
    {
        protected ApplicationBaseDbContext(DbContextOptions options) : 
            base(options) { }

        public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
        public DbSet<IntegratorOrganization> IntegratorOrganizations { get; set; }
        public DbSet<Integrator> Integrators { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberContact> MemberContact { get; set; }
        public DbSet<MemberSetting> MemberSettings { get; set; }
        public DbSet<MemberRelationship> MemberRelationships { get; set; }
        public DbSet<MemberImage> MemberImages { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationSetting> OrganizationSettings { get; set; }
        public DbSet<OrganizationMember> OrganizationMembers { get; set; }
        public DbSet<OrganizationType> OrganizationTypes { get; set; }
        public DbSet<OrganizationImage> OrganizationImages { get; set; }
        public DbSet<BillableEntity> BillableEntites { get; set; }
        public DbSet<FhirUserConflation> FhirUserConflations { get; set; }
    }
}
