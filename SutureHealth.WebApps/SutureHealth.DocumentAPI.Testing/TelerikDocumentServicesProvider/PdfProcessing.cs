using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SutureHealth.Documents.Testing
{
    public partial class TelerikDocumentServicesProviderTestBase
    {
        [TestCategory("Development")]
        [TestMethod]
        public void VerifyValidPdf()
        {
            Assert.IsTrue(DocumentService.ValidatePdf(System.IO.File.ReadAllBytes(@"C:\Users\mculotta\Desktop\TEST_SIGNING_RESULT.pdf")));
        }

        [TestCategory("Development")]
        [TestMethod]
        public void VerifyInvalidPdf()
        {
            Assert.IsFalse(DocumentService.ValidatePdf(new byte[0]));
        }
    }
}
