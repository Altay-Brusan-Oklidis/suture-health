using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace SutureHealth.DataScrapingAPI.Testing.Tests
{
    [TestClass]
    internal class HelperFunctionTests : IntegrationTestBase
    {
        [Test]
        public async Task GenderIdentifierTests()
        {
            string male = "Male";
            string male2 = "male";
            string male3 = "MALE";
            string female = "Female";
            string female2 = "female";
            string female3 = "FEMALE";
            string ambigious = "ambigious";
            string ambgious2 = "AMBIGIOUS";
            string unknown = "UNKNOWN";
            string unknown2 = "RandomMeaninglessString";

            var maleRes1 = await DataScrapingService.GenderIdentifierAsync(male);
            var maleRes2 = await DataScrapingService.GenderIdentifierAsync(male2);
            var maleRes3 = await DataScrapingService.GenderIdentifierAsync(male3);
            var femaleRes1 = await DataScrapingService.GenderIdentifierAsync(female);
            var femaleRes2 = await DataScrapingService.GenderIdentifierAsync(female2);
            var femaleRes3 = await DataScrapingService.GenderIdentifierAsync(female3);
            var ambigiousRes1 = await DataScrapingService.GenderIdentifierAsync(ambigious);
            var ambigiousRes2 = await DataScrapingService.GenderIdentifierAsync(ambgious2);
            var unknownRes1 = await DataScrapingService.GenderIdentifierAsync(unknown);
            var unknownRes2 = await DataScrapingService.GenderIdentifierAsync(unknown2);

            NUnit.Framework.Assert.That(maleRes1, Is.EqualTo(Gender.Male));
            NUnit.Framework.Assert.That(maleRes2, Is.EqualTo(Gender.Male));
            NUnit.Framework.Assert.That(maleRes3, Is.EqualTo(Gender.Male));
            NUnit.Framework.Assert.That(femaleRes1, Is.EqualTo(Gender.Female));
            NUnit.Framework.Assert.That(femaleRes2, Is.EqualTo(Gender.Female));
            NUnit.Framework.Assert.That(femaleRes3, Is.EqualTo(Gender.Female));
            NUnit.Framework.Assert.That(ambigiousRes1, Is.EqualTo(Gender.Ambiguous));
            NUnit.Framework.Assert.That(ambigiousRes2, Is.EqualTo(Gender.Ambiguous));
            NUnit.Framework.Assert.That(unknownRes1, Is.EqualTo(Gender.Unknown));
            NUnit.Framework.Assert.That(unknownRes2, Is.EqualTo(Gender.Unknown));
        }
    }
}
