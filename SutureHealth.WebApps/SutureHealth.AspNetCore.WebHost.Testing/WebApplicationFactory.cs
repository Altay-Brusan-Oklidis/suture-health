using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.AspNetCore.Mvc.Testing
{
    public class WebApplicationFactory<T> : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<T>
        where T : class
    {
        private readonly Action<IConfigurationBuilder> _configure;

        public WebApplicationFactory(Action<IConfigurationBuilder> configure)
        {
            _configure = configure;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (_configure != null)
            {
                builder.ConfigureAppConfiguration(_configure);
            }
        }
    }
}
