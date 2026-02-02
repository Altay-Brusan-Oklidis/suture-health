using System.Linq;

namespace Microsoft.Extensions.Hosting
{
    public static class HostEnvironmentEnvExtensions
    {
        public static bool IsEnvironment(this IHostEnvironment hostEnvironment, params string[] environmentName)
            => environmentName.Any(a => string.Equals(a, hostEnvironment.EnvironmentName));
    }
}
