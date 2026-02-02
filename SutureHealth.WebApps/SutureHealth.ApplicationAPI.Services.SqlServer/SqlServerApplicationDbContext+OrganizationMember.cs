using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Application.Services.SqlServer
{
    public partial class SqlServerApplicationDbContext : IdentityDbContext
    {
        public override async Task<IEnumerable<OrganizationMember>> GetSigningOrganizationMembersAsync(string searchText, string organizationStateOrProvinceFilter = null, int count = 10)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return Array.Empty<OrganizationMember>();
            }
            searchText = searchText.Trim();

            var patternMatches = new[]
            {
                Regex.Match(searchText, @"^(?<FirstName>[A-Za-z0-9]+) (?<LastName>[A-Za-z0-9]+)(, (?<Suffix>[A-Za-z0-9]+))?"),
                Regex.Match(searchText, @"^(?<LastName>[A-Za-z0-9]+), (?<FirstName>[A-Za-z0-9]+)( (?<Suffix>[A-Za-z0-9]+))?")
            };
            var query = OrganizationMembers.AsNoTracking()
                                           .Include(om => om.Organization)
                                           .Include(om => om.Member)
                                           .Where(om => om.Member.CanSign && om.Member.IsActive && om.IsActive && om.EffectiveAt <= DateTime.Now);

            if (!string.IsNullOrWhiteSpace(organizationStateOrProvinceFilter))
            {
                query = query.Where(om => om.Organization.StateOrProvince == organizationStateOrProvinceFilter);
            }

            if (patternMatches.FirstOrDefault(m => m.Success) is Match match)
            {
                var firstName = match.Groups["FirstName"].Value;
                var lastName = match.Groups["LastName"].Value;
                var suffix = match.Groups["Suffix"].Value;

                query = query.Where(om => EF.Functions.Like(om.Member.FirstName, $"%{firstName}%") && EF.Functions.Like(om.Member.LastName, $"%{lastName}%"));

                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    query = query.Where(om => EF.Functions.Like(om.Member.Suffix, $"%{suffix}%"));
                }
            }
            else
            {
                query = query.Where(om => EF.Functions.Like(om.Member.FirstName, $"%{searchText}%") ||
                                          EF.Functions.Like(om.Member.LastName, $"%{searchText}%") ||
                                          EF.Functions.Like(om.Member.NPI, $"%{searchText}%"));
            }

            return await query.OrderByDescending(om => om.MemberId)
                              .ThenBy(om => om.OrganizationMemberId)
                              .Take(count)
                              .ToArrayAsync();
        }
    }
}
