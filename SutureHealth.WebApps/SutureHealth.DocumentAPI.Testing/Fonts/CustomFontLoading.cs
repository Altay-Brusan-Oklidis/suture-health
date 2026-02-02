using Microsoft.VisualStudio.TestTools.UnitTesting;
using SutureHealth.Documents.Services.Assets.Fonts;

namespace SutureHealth.Documents.Testing.Fonts
{
    [TestClass]
    public class CustomFontLoading
    {
        [TestMethod]
        public void LoadAllFonts()
        {
            Assert.IsNotNull(SutureFontsRepository.FreestyleScript);
            Assert.IsNotNull(SutureFontsRepository.ZapfDingbats);
            Assert.IsNotNull(SutureFontsRepository.Arial);
        }
    }
}
