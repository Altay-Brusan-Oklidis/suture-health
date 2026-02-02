using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SutureHealth.Reporting.Services;
using System.Threading.Tasks;

namespace SutureHealth.AspNetCore.WebHost.Testing.Area.Request
{
    [TestCategory("Integration")]
    [TestClass]
    public class SignController : IntegrationTestBase
    {
        [TestMethod]
        public async Task GetSendHelp()
        {
            int signerId = 3000005,
                assistantId = 3001070;
            var templateDisplayName = "Template Display Name";

            //await AuthenticateAsync();
            //var response = await HttpClient.PostAsync("/Request/Sign/Help", new FormUrlEncodedContent(new Dictionary<string,string> {
            //    ["signerId"] = "3000005",
            //    ["assistantId"] = "3001070",
            //    ["templateDisplayName"] = "Template Display Name"
            //}));
            //Assert.IsTrue(response.IsSuccessStatusCode);

            using var scope = this.ApplicationFactory.Services.CreateScope();
            var delivery = scope.ServiceProvider.GetRequiredService<IDeliveryService>();
            await delivery.SendRequestForAssistanceEmailAsync(signerId, assistantId, templateDisplayName);
        }
    }
}
