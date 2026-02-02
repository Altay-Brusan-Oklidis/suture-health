using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using System.Text;

namespace SutureHealth.PatientAPI.Services.Kno2.Webhook.Authorizer.Lambda.Tests;

public class BasicAuthenticatorTests
{
    [Fact]
    public void TestWithValidAuthorizationHeaderReturnsAllowPolicy()
    {
        var username = "user";
        var password = "pass";
        var authenticator = new BasicAuthenticator(username, password);

        var userPassBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        var request = new APIGatewayCustomAuthorizerRequest
        {
            Headers = new Dictionary<string, string>
            {
                {"authorization", $"Basic {userPassBase64}" }
            }
        };

        var context = new TestLambdaContext();

        var result = authenticator.IsAuthorized(request, context);

        Assert.Equal("Allow", result.PolicyDocument.Statement[0].Effect);
    }

    [Fact]
    public void TestWithAnyAuthorizationHeaderReturnsDenyPolicy()
    {
        var username = "user";
        var password = "pass";
        var authenticator = new BasicAuthenticator(username, password);

        var userPassBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"eve:attack"));
        var request = new APIGatewayCustomAuthorizerRequest
        {
            Headers = new Dictionary<string, string>
            {
                {"authorization", $"Basic {userPassBase64}" }
            }
        };

        var context = new TestLambdaContext();

        var result = authenticator.IsAuthorized(request, context);

        Assert.Equal("Deny", result.PolicyDocument.Statement[0].Effect);
    }

    [Fact]
    public void TestWithoutAuthorizationHeaderReturnsDenyPolicy()
    {
        var username = "user";
        var password = "pass";
        var authenticator = new BasicAuthenticator(username, password);
        var request = new APIGatewayCustomAuthorizerRequest
        {
            Headers = new Dictionary<string, string>
            {
            }
        };

        var context = new TestLambdaContext();

        var result = authenticator.IsAuthorized(request, context);

        Assert.Equal("Deny", result.PolicyDocument.Statement[0].Effect);
    }
}