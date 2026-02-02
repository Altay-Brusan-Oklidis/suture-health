using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SutureHealth.Documents.Testing
{
    public partial class TelerikDocumentServicesProviderTestBase
    {
        /*
        [TestCategory("Development")]
        [DataRow(500009)]   // HTML Coordinates
        //[DataRow(548872)]   // PDF Coordinates w/ negative
        [DataTestMethod]
        public async Task RenderEditorAnnotations(int templateId)
        {
            var template = await DocumentService.GetEditorTemplatePngsByIdAsync(templateId);

            await System.IO.File.WriteAllBytesAsync(@"C:\Users\mculotta\Desktop\TEST_EDITOR_RESULT.png", template[0]);
        }

        [TestCategory("Development")]
        [DataRow(500009)]
        [DataTestMethod]
        public async Task RenderSigningAnnotations(int templateId)
        {
            var template = await DocumentService.GetSignedTemplatePngsByIdAsync(templateId, new SignedPdfAttributes
            {
                Signature = "RichardSignerr Sign Jr., MD",
                SignatureId = "C7246579-3D01-4B24-A1AF-AC383847E221",
                DateSigned = DateTime.Now,
                RequestId = "100000001"
            });

            await System.IO.File.WriteAllBytesAsync(@"C:\Users\mculotta\Desktop\TEST_SIGNING_RESULT.png", template[0]);
        }
        */

        [TestCategory("Development")]
        [DataRow(500009)]
        [DataTestMethod]
        public async Task RenderSignedPdf(int templateId)
        {
            var template = await DocumentService.GetSignedTemplatePdfByIdAsync(templateId, new SignedPdfAttributes
            {
                Signature = "RichardSignerr Sign Jr., MD",
                SignatureId = "C7246579-3D01-4B24-A1AF-AC383847E221",
                DateSigned = DateTime.Now,
                RequestId = "100000001"
            });

            await System.IO.File.WriteAllBytesAsync(@"C:\Users\mculotta\Desktop\TEST_SIGNING_RESULT.pdf", template);
        }

        [TestCategory("Development")]
        [DataRow(500009)]
        [DataTestMethod]
        public async Task RenderRejectedPdf(int templateId)
        {
            var template = await DocumentService.GetRejectedTemplatePdfByIdAsync(templateId, new RejectedPdfAttributes
            {
                RequestId = "100000001",
                DateProcessed = DateTime.Now,
                ProcessedBy = "RichardSignerr Sign Jr., MD",
                ProcessingOffice = "Sign1 - Org 1",
                ProcessingOfficePhone = "(555) 311-6969",
                RejectionReason = "The quick brown fox jumps over the lazy dog.  The quick brown fox jumps over the lazy dog.  The quick brown fox jumps over the lazy dog.  The quick brown fox jumps over the lazy dog.  The quick brown fox jumps over the lazy dog.  The quick brown fox jumps over the lazy dog."
            });

            await System.IO.File.WriteAllBytesAsync(@"C:\Users\mculotta\Desktop\TEST_REJECTION_RESULT.pdf", template);
        }

        [TestCategory("Development")]
        [DataRow(false)]
        [DataRow(true)]
        [DataTestMethod]
        public void GenerateFaceToFaceDocument(bool includeTreatmentPlan)
        {
            System.IO.File.WriteAllBytes($@"C:\Users\mculotta\Desktop\TEST_F2F{(includeTreatmentPlan ? "_TREATMENT" : string.Empty)}.pdf", DocumentService.GenerateFaceToFacePdf(new FaceToFaceAttributes()
            {
                Signature = "RichardSignerr Sign Jr., MD",
                DateSigned = DateTime.Now,
                Npi = "1306957386",
                RequestId = "100000001",
                SignatureId = "C7246579-3D01-4B24-A1AF-AC383847E221",
                EpisodeEffectiveDate = DateTime.Now.ToString(),
                Patient = "Colby Willoughby (1/1/1990, 3462)",
                NursingRequired = true,
                OccupationalTherapyRequired = true,
                PhysicialTherapyRequired = true,
                SpeechTherapyRequired = true,
                EncounterDate = DateTime.Now.ToString(),
                SendingOrganizationName = "Suture Home Health",
                MedicalCondition = "Insomnia",
                ClinicalReasonForHomeCare = "Clinical reason for home care",
                ReasonForBeingHomebound = "Reason for being home bound",
                TreatmentPlan = includeTreatmentPlan ? "Plan for above treatment" : null
            }));
        }
    }
}
