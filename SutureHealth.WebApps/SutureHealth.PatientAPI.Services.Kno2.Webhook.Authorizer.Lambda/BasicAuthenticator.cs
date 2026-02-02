using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SutureHealth.Extensions.Configuration;
using System.Text;
using static Amazon.Lambda.APIGatewayEvents.APIGatewayCustomAuthorizerPolicy;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SutureHealth.PatientAPI.Services.Kno2.Webhook.Authorizer.Lambda;

public class BasicAuthenticator
{
    public BasicAuthenticator()
    {
        var host = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
        {
            var assemblies = new string[] {
                typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name!
            };
            webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
            webBuilder.ConfigureAppConfiguration((host, config) => config.AddDefaultConfigurations().Build())
                      .ConfigureServices((context, services) =>
                      {
                          services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());

                          var kno2WebhookConfig = context.Configuration.GetSection("Kno2:Webhook");
                          Username = kno2WebhookConfig["Username"] ?? throw new Exception("Kno2:Webhook:Username configuration could not be found.");
                          var password = kno2WebhookConfig["Password"] ?? throw new Exception("Kno2:Webhook:Password configuration could not be found.");
                          UserPassBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{password}"));
                      });
        })
        .Build();
    }

    public BasicAuthenticator(string username, string password)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        var p = password ?? throw new ArgumentNullException(nameof(password));
        UserPassBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{p}"));
    }

    private string Username { get; set; }
    private string UserPassBase64 { get; set; }

    public APIGatewayCustomAuthorizerResponse IsAuthorized(APIGatewayCustomAuthorizerRequest request, ILambdaContext _)
    {
        const string AuthorizationHeaderName = "authorization";

        var response = new APIGatewayCustomAuthorizerResponse
        {
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Statement = new List<IAMPolicyStatement>
                {
                    new IAMPolicyStatement
                    {
                        Action = new HashSet<string>{ "execute-api:Invoke" },
                        Effect = "Deny",
                        Resource = new HashSet<string> { "arn:aws:execute-api:us-east-1:267058832120:mglvw8y5g6/*/*" }
                    }
                },
                Version = "2012-10-17"
            },
            PrincipalID = Username
        };

        if (request is null)
        {
            LambdaLogger.Log("request is null");
            return response;
        }

        if (request.Headers is null)
        {
            LambdaLogger.Log("request.Headers is null");
            return response;
        }

        if (!request.Headers.ContainsKey(AuthorizationHeaderName))
        {
            LambdaLogger.Log("No Authorization header.");
            return response;
        }

        if (request.Headers[AuthorizationHeaderName] == $"Basic {UserPassBase64}")
        {
            response.PolicyDocument.Statement[0].Effect = "Allow";
            return response;
        }

        return response;
    }
}
