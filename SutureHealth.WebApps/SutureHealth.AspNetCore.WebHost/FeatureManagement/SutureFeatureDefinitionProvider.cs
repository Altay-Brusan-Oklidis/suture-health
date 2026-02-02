using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using System.Collections.Concurrent;

namespace SutureHealth.AspNetCore.FeatureManagement
{
    public class SutureFeatureDefinitionProvider : IFeatureDefinitionProvider
    {
        private IConfiguration Configuration { get; }
        private IServiceProvider ServiceProvider { get; }
        private ConcurrentDictionary<string, (DateTime Expiration, FeatureDefinition Feature)> SidelineCache { get; }

        private const string FeatureFiltersSectionName = "EnabledFor";
        private IDisposable ChangeSubscription;
        private int _stale = 0;
        private const int CACHE_EXPIRE_SECONDS = 5;


        public SutureFeatureDefinitionProvider
        (
            IConfiguration configuration,
            IServiceProvider serviceProvider
        )
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ServiceProvider = serviceProvider;
            SidelineCache = new ConcurrentDictionary<string, (DateTime, FeatureDefinition)>();

            ChangeSubscription = ChangeToken.OnChange(
                () => Configuration.GetReloadToken(),
                () => _stale = 1);
        }

        public void Dispose()
        {
            ChangeSubscription?.Dispose();
            ChangeSubscription = null;
        }

        public async Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName)
        {
            if (featureName == null)
            {
                throw new ArgumentNullException(nameof(featureName));
            }

            if (Interlocked.Exchange(ref _stale, 0) != 0)
            {
                SidelineCache.Clear();
            }

            if (SidelineCache.TryGetValue(featureName, out var feature) && feature.Expiration > DateTime.UtcNow)
                return feature.Feature;

            (DateTime Expiration, FeatureDefinition Feature) definition = default;
            using (IServiceScope scope = ServiceProvider.CreateScope())
            {
                // Query by feature name
                var securityService = scope.ServiceProvider.GetService<IApplicationService>();
                var setting = await securityService.GetApplicationSettings()
                                                   .Where(s => s.Key == featureName && s.ItemType == ItemType.Boolean)
                                                   .OrderBy(s => s.IsActive)
                                                   .FirstOrDefaultAsync();
                definition = ReadFeatureDefinition(setting);
                SidelineCache.AddOrUpdate(featureName, definition, (k, v) => definition);
            }

            return definition.Feature;
        }

        //
        // The async key word is necessary for creating IAsyncEnumerable.
        // The need to disable this warning occurs when implementaing async stream synchronously. 
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync()
#pragma warning restore CS1998
        {
            if (Interlocked.Exchange(ref _stale, 0) != 0)
            {
                SidelineCache.Clear();
            }

            IEnumerable<ApplicationSetting> settings = Array.Empty<ApplicationSetting>();
            using (IServiceScope scope = ServiceProvider.CreateScope())
            {
                var securityService = scope.ServiceProvider.GetService<IApplicationService>();
                settings = await securityService.GetApplicationSettings()
                                                .Where(s => s.ItemType == ItemType.Boolean)
                                                .ToArrayAsync();
            }

            foreach (var setting in settings)
            {
                yield return ReadFeatureDefinition(setting).Feature;
            }
        }

        private (DateTime Expiration, FeatureDefinition Feature) ReadFeatureDefinition(ApplicationSetting setting)
        {
            var enabledFor = new List<FeatureFilterConfiguration>();

            if (setting.IsActive.GetValueOrDefault(false) && setting.ItemBool.GetValueOrDefault(false))
            {
                enabledFor.Add(new FeatureFilterConfiguration
                {
                    Name = "AlwaysOn"
                });
            }

            return new(DateTime.UtcNow.AddSeconds(CACHE_EXPIRE_SECONDS), new FeatureDefinition()
            {
                Name = setting.Key,
                EnabledFor = enabledFor
            });
        }
    }
}