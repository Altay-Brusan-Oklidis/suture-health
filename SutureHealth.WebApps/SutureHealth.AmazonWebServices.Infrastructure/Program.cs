using Amazon.CDK;
using Microsoft.Extensions.Configuration;

namespace SutureHealth.AmazonWebServices.Infrastructure
{
    public class DeploymentSettings
    {
        public string AwsAccountId { get; set; }
        public string AwsRegion { get; set; }
        public bool Debug { get; set; }
    }

    sealed class Program
    {
        public static void Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();

            var deploymentSettings = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    {"AwsAccountId", "TODO - Enter your AWS Account Id here"},
                                   {"AwsRegion", "eu-west-1"},
                                   {"RequestQuoteProcessorMemorySize", "128"},
                                   {"MarketingDingEmail", "developer+QA-Env@CloudAutoGroup.com"},
                                   {"Debug", "true"}
                })
                // relative to the root of the Cdk.csproj
                .AddJsonFile("settings.json", optional: true)
                .Build()
                .Get<DeploymentSettings>();

            if (deploymentSettings.Debug)
                Debugger.Launch();

            var app = new App();
            new SutureSignStack(app, $"SutureSign-{app.StackProps.Deployment}", app.StackProps);
            app.Synth();
        }
    }
}