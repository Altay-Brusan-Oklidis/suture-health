using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Asp.Versioning;
using ExpressiveAnnotations.MvcCoreUnobtrusive;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using SutureHealth.AspNetCore.FeatureManagement;
using SutureHealth.AspNetCore.Filters;
using SutureHealth.AspNetCore.Mvc.ApplicationModels;
using SutureHealth.AspNetCore.Routing;
using SutureHealth.AspNetCore.SwaggerGen;
using SutureHealth.Diagnostics;
using SutureHealth.Notifications;
using SutureHealth.Notifications.Services;
using System.Net;
using System.Text.RegularExpressions;
using SutureHealth.Linq;

namespace SutureHealth.AspNetCore.WebHost;

public class Startup
{
    public const string CorsLegacyPolicy = "_legacy.suturehealth.com";
    public const string CorsNextPolicy = "_next.suturehealth.com";
    public const string CorsLocalPolicy = "localhost:*";

    public Startup(IWebHostEnvironment environment, IConfiguration configuration)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

        services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.EnableForHttps = true;
            options.MimeTypes = new[] {
                "application/json",
                "application/javascript",
                "text/tab-separated-values",
                "application/javascript",
                "text/csv",
                "text"
               };
        });

        services.AddSingleton<IFeatureDefinitionProvider, SutureFeatureDefinitionProvider>()
                .AddFeatureManagement();

        services.AddOptions<NotificationGenerationOptions>()
                .Configure<IConfiguration, LinkGenerator, IHttpContextAccessor>((options, configuration, generator, context) =>
                {
                    options.SiteBaseUrl = context.HttpContext.Request.Host.Host;
                });

        services.AddOptions<NotificationOptions>()
                .Bind(Configuration.GetSection("SutureHealth:NotificationServicesProvider"))
                .Configure<LinkGenerator, IHttpContextAccessor>((options, generator, context) =>
                {
                    options.GenerateCallbackUrl = notification => generator.GetUriByRouteValues("NotificationCallback", new
                    {
                        notificationId = notification.Id,
                        providerId = notification.ProviderId,
                        version = notification.ApiVersion
                    }, System.Uri.UriSchemeHttps, new Microsoft.AspNetCore.Http.HostString(options.ApiBaseUri));
                });

        services.AddOptions<SessionMonitorOptions>()
                .Configure<IConfiguration, LinkGenerator, IHttpContextAccessor, IHostEnvironment>((options, configuration, generator, context, env) =>
                {
                    options.Enable = !env.IsEnvironment("ci", "qa", "dev");
                    options.CushionTimeout = TimeSpan.TryParse(Configuration["SutureHealth:Authentication:SessionMonitor:CushionTimeout"], out var cushionTimeout) ? cushionTimeout : TimeSpan.FromSeconds(6);
                    options.EnableUI = bool.TryParse(Configuration["SutureHealth:Authentication:SessionMonitor:EnableUI"], out var enableUI) && enableUI;
                    options.IdleTimeout = TimeSpan.TryParse(Configuration["SutureHealth:Authentication:SessionMonitor:IdleTimeout"], out var idleTimeout) ? idleTimeout : TimeSpan.FromMinutes(30);
                    options.LoginUrl = generator.GetPathByPage(context.HttpContext, "/Account/Login", null, new { area = "Identity" });
                    options.PingUrl = generator.GetPathByName("SessionPing", null);
                    options.WarningTimeout = TimeSpan.TryParse(Configuration["SutureHealth:Authentication:SessionMonitor:WarningTimeout"], out var warningTimeout) ? warningTimeout : TimeSpan.FromMinutes(2);
                });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = new PathString("/identity/account/login");
            options.AccessDeniedPath = new PathString("/identity/account/login");
            options.LogoutPath = new PathString("/identity/account/login");
        });

        services.Configure<CorsOptions>(options =>
        {
            options.AddPolicy(name: Startup.CorsLegacyPolicy,
                              builder => builder.WithOrigins(Configuration["SutureHealth:WebBaseUri"] ?? "web.suturehealth.com")
                                                .AllowAnyMethod()
                                                .AllowAnyHeader()
                                                .AllowCredentials());

            options.AddPolicy(name: Startup.CorsNextPolicy,
                              builder => builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                                                .WithOrigins("https://suturehealth.com", "https://app.*.suturehealth.com",
                                                "https://api.*.suturehealth.com", "https://*.suturehealth.com")
                                                .AllowAnyMethod()
                                                .AllowAnyHeader()
                                                .AllowCredentials());
        });

        /*
        services.Configure<JsonOptions>(options =>
        {
             options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
             options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
        */
        services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new ControllerHidingAppConvention());
            options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(AuthorizationPolicies.AuthorizedUser));
            options.Filters.Add(new SutureUserActionFilter());
            options.Filters.Add(new SutureUserPageFilter());
            options.Filters.Add<OperationCancelledExceptionFilter>();
        });

        services.Configure<RewriteOptions>(options =>
        {
            options.AddApacheModRewrite(Environment.ContentRootFileProvider, $"htaccess.{Environment.EnvironmentName}");
        });

        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add("ReportType", typeof(EnumerationRouteConstraint<SutureHealth.AspNetCore.Mvc.Routing.ReportType>));
            options.LowercaseUrls = true;
        });

        services.AddAuthorization();
        services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                .AddOData();
        services.AddApiVersioning(options =>
                {
                    options.ReportApiVersions = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ApiVersionReader = ApiVersionReader.Combine
                    (
                        new QueryStringApiVersionReader(),
                        new HeaderApiVersionReader("api-version", "x-ms-version")
                    );
                })
                .AddMvc()
                .AddOData()
                .AddODataApiExplorer(options => options.GroupNameFormat = "'v'V.v");
        services.AddCors();
        services.AddExpressiveAnnotations();
        services.AddHttpContextAccessor();
        services.AddKendo();
        services.AddRazorPages();
        //services.AddSignalR();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v0.1", new OpenApiInfo
            {
                Title = "Suture Health API",
                Version = "v0.1",
                Description = "Suture Health API",
                Contact = new OpenApiContact
                {
                    Name = "Suture Health",
                    Email = "support@suturehealth.com"
                }
            });

            options.SwaggerDoc("v1.0", new OpenApiInfo
            {
                Title = "Suture Health API",
                Version = "v1.0",
                Description = "Suture Health API",
                Contact = new OpenApiContact
                {
                    Name = "Suture Health",
                    Email = "support@suturehealth.com"
                }
            });

            options.CustomSchemaIds(type =>
            {
                var schemaId = type.FullName;
                if (type.FullName.StartsWith("SutureHealth"))
                {
                    var splits = type.FullName.Split(".");
                    var sh = splits[0];
                    var api = splits[1];
                    schemaId = string.Join(".", new[] { sh, string.Equals(api, "aspnetcore", System.StringComparison.OrdinalIgnoreCase) ? null : api, type.Name }.Where(x => x != null));
                }

                return schemaId;
            });

            // integrate xml comments
            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            foreach (var docfile in Directory.GetFiles(basePath, "SutureHealth.*.AspNetCore.xml", SearchOption.TopDirectoryOnly))
            {
                options.IncludeXmlComments(docfile, true);
            }

            // add a custom operation filter which sets default values
            options.OperationFilter<SwaggerDefaultValuesOperationFilter>();
        });

        if (Environment.IsEnvironment("ci"))
        {
            services.AddScoped<ITracingService, NullTracingService>();
        }
        else
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            AWSXRayRecorder.InitializeInstance(Configuration);
            AWSXRayRecorder.RegisterLogger(Amazon.LoggingOptions.Console);
            AWSXRayRecorder.RegisterLogger(Amazon.LoggingOptions.SystemDiagnostics);

            services.AddScoped<ITracingService, XrayTracingService>();
        }
    }

    public void Configure
    (
        IApplicationBuilder app,
        IConfiguration configuration,
        IWebHostEnvironment env
    )
    {
        app.UseForwardedHeaders();
        app.Use((context, next) =>
        {
            context.Response.Headers["referrer-policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["strict-transport-security"] = "max-age=315360000";
            context.Response.Headers["x-content-type-options"] = "nosniff";
            context.Response.Headers["content-security-policy"] = "frame-ancestors \'self\' *.suturehealth.com:*";

            context.Response.Headers.Remove("x-powered-by");

            return next.Invoke();
        });

        app.Use((context, next) =>
        {
            context.Request.Scheme = Uri.UriSchemeHttps;
            return next.Invoke();
        });

#if !DEBUG
        app.UseRewriter();
        app.Use((context, next) =>
        {
            context.Request.Scheme = Uri.UriSchemeHttps;
            return next.Invoke();
        });
#endif

        if (env.IsEnvironment("ci", "qa", "stage", "dev"))
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseXRay("SutureHealth-WebApps");
        }

        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = ctx =>
            {
#if DEBUG
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
#else
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", Configuration["SutureHealth:WebBaseUri"] ?? $"{Environment.EnvironmentName}.suturehealth.com");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
#endif
            },
        });
        app.UseRouting();
        app.UseCors();
        app.UseMiddleware<RedirectFromAppMiddleware>();
        app.UseMiddleware<RedirectionMiddlewareOnLogout>();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
            endpoints.MapSwagger(setupAction: options => options.PreSerializeFilters.Add((swagger, httpReq) => swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } }));
            //endpoints.MapHub<AccountAuditHub>("/identity/accounts/auditing");

            if (env.IsEnvironment("ci", "qa", "demo", "stage", "dev"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in endpoints.DescribeApiVersions().OrderByDescending(vd => vd.ApiVersion.MajorVersion).ThenByDescending(vd => vd.ApiVersion.MinorVersion))
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });
            }
        });
    }
}

public class RedirectFromAppMiddleware
{
    private readonly RequestDelegate _next;

    public RedirectFromAppMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var shouldRedirect = ShouldRedirect(ref context);
        if (!shouldRedirect)
        {
            await _next(context);
            return;
        }

        var request = context.Request;
        var host = request.Host;
        var regex = new Regex(@"^app\.([a-zA-Z0-9-]+)\.([a-zA-Z0-9-]+)\.([a-zA-Z]+)$", RegexOptions.IgnoreCase);
        var match = regex.Match(host.Host);
        if (!match.Success)
        {
            await _next(context);
        }
        //var request = context.Request;
        //var host = request.Host.Host;
        //var path = request.Path;

        var subdomain = match.Groups[1].Value;
        var domain = match.Groups[2].Value;
        var tld = match.Groups[3].Value;
        var port = host.Port;
        var sPort = port == 443 ? string.Empty : $":{port.ToString()}";

        var newUrl = $"https://{subdomain}.{domain}.{tld}{sPort}{request.Path}{request.QueryString}";
        context.Response.Redirect(newUrl, permanent: false);
    }

    private static bool IsLocalDevelopment(HostString host)
    {
        var regex = new Regex(@"^app.dev|^localhost");
        return regex.Match(host.Host).Success;
    }

    private static bool ShouldRedirect(ref HttpContext context)
    {
        var request = context.Request;
        var redirectPaths = new[] { "/request/send", "/request/sign", "/request/history", "/visit", "/revenue", "/organization", "/member", "/" };

        bool pathMatch = false;
        foreach (var path in redirectPaths)
        {
            if (request.Path.Value.StartsWith(path))
            {
                var remainingPath = request.Path.Value.Substring(path.Length).TrimStart('/');
                var parts = remainingPath.Split('/');
                if (parts.Length > 0 && int.TryParse(parts[0], out _))
                {
                    pathMatch = true;
                    break;
                }
            }
        }

        var contentOnlyFlag = string.Equals(request.Query["contentOnly"], "true", StringComparison.OrdinalIgnoreCase);

        return !IsLocalDevelopment(request.Host) && pathMatch && !contentOnlyFlag;
    }
}

public class RedirectionMiddlewareOnLogout
{
    private readonly RequestDelegate _next;

    public RedirectionMiddlewareOnLogout(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == 302 || context.Response.StatusCode == 307)
        {
            if (!context.Response.Headers["Location"].Any(l => l.ToLower().Contains("/identity/account/login?returnurl=%2fapi%2f")))
                return;

            string newLocation = "/identity/account/login";
            context.Response.Headers["Location"] = newLocation;

            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }
    }
}


