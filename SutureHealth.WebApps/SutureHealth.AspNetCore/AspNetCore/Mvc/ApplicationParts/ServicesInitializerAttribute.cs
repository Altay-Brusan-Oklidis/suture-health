using Microsoft.Extensions.DependencyInjection;

namespace SutureHealth.AspNetCore.Mvc.ApplicationParts
{
   public interface ISupportDependentServices
   {
      void ConfigureAppServices(IServiceCollection services);
   }
}
