using System;
using Telerik.Documents.Core.Fonts;
using Telerik.Documents.Primitives;
using Telerik.Windows.Documents.Fixed.Model.ColorSpaces;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Fixed.Model.Editing.Flow;
using Telerik.Windows.Documents.Fixed.Model.Text;
using SutureHealth.Documents.Services.Assets.Fonts;

namespace SutureHealth.Documents.Services.Extensions
{
    public static class FixedContentEditorExtensions
    {
        public static void DrawTemplateAnnotationAsEditing(this FixedContentEditor editor, TemplateAnnotation annotation)
        {
            double multiplier;
            Rect boundingBox;

            if (annotation.PageHeight.GetValueOrDefault() > 0)
            {
                boundingBox = new Rect(0, 0, (annotation.HtmlCoordinateRight.Value - annotation.HtmlCoordinateLeft.Value) * Constants.HTML_WIDTH_SCALING_MULTIPLIER, (annotation.HtmlCoordinateBottom.Value - annotation.HtmlCoordinateTop.Value) * Constants.HTML_HEIGHT_SCALING_MULTIPLIER);
            }
            else
            {
                multiplier = Constants.ABCPDF_TO_TELERIK_SCALING_MULTIPLIER;
                boundingBox = new Rect(0, 0, (annotation.PdfCoordinateRight.Value - annotation.PdfCoordinateLeft.Value) * multiplier, (annotation.PdfCoordinateTop.Value - annotation.PdfCoordinateBottom.Value) * multiplier);
            }

            switch (annotation.AnnotationType)
            {
                case AnnotationType.TextArea:
                    if (string.IsNullOrEmpty(annotation.Value)/* && annotation.IsRequired.GetValueOrDefault()*/)
                    {
                        editor.DrawAnnotationBoundingBox(boundingBox.Size, Constants.BACKGROUND_COLOR_RED, Constants.BORDER_COLOR_RED);
                    }
                    else
                    {
                        editor.DrawAnnotationBoundingBox(boundingBox.Size, Constants.BACKGROUND_COLOR_YELLOW);
                    }
                    editor.DrawAnnotationTextAreaText(boundingBox.Size, string.IsNullOrEmpty(annotation.Value) ? "Enter text here..." : annotation.Value);

                    break;
                case AnnotationType.VisibleSignature:
                    editor.DrawAnnotationBoundingBox(boundingBox.Size, Constants.BACKGROUND_COLOR_BLUE);
                    editor.DrawAnnotationSignaturePlaceholder(boundingBox.Size);

                    break;
                case AnnotationType.DateSigned:
                    editor.DrawAnnotationBoundingBox(boundingBox.Size, Constants.BACKGROUND_COLOR_BLUE);
                    editor.DrawAnnotationDateTimePlaceholder(boundingBox.Size);

                    break;
                case AnnotationType.CheckBox:
                    editor.DrawAnnotationBoundingBox(boundingBox.Size, Constants.BACKGROUND_COLOR_DARK_YELLOW);
                    editor.DrawAnnotationCheckboxValue(boundingBox.Size, bool.TryParse(annotation.Value, out var isChecked) ? isChecked : false);

                    break;
            }
        }

        public static void DrawTemplateAnnotationAsSigning(this FixedContentEditor editor, TemplateAnnotation annotation, string signature, string signatureId, DateTimeOffset dateSigned)
        {
            var multiplier = annotation.PageHeight.HasValue ? (editor.Root.Size.Height / annotation.PageHeight.Value) : 1.0;
            var boundingBox = new Rect(0, 0, (annotation.HtmlCoordinateRight.Value - annotation.HtmlCoordinateLeft.Value) * multiplier, (annotation.HtmlCoordinateBottom.Value - annotation.HtmlCoordinateTop.Value) * multiplier);            

            switch (annotation.AnnotationType)
            {
                case AnnotationType.TextArea:
                    editor.DrawAnnotationTextAreaText(boundingBox.Size, annotation.Value ?? string.Empty);

                    break;
                case AnnotationType.VisibleSignature:
                    editor.DrawAnnotationSignatureText(boundingBox.Size, signature, signatureId, dateSigned);

                    break;
                case AnnotationType.DateSigned:
                    editor.DrawAnnotationDateTimeText(boundingBox.Size, dateSigned);

                    break;
                case AnnotationType.CheckBox:
                    editor.DrawAnnotationCheckboxValue(boundingBox.Size, bool.TryParse(annotation.Value, out var isChecked) ? isChecked : false);

                    break;
            }
        }

        public static void DrawRequestFooter(this FixedContentEditor editor, string footerId)
        {
            var requestBlock = new Block();

            requestBlock.TextProperties.Font = SutureFontsRepository.ArialBold;
            requestBlock.TextProperties.RenderingMode = RenderingMode.Fill;
            requestBlock.TextProperties.FontSize = 11;
            requestBlock.GraphicProperties.FillColor = Constants.COLOR_BLACK;
            requestBlock.VerticalAlignment = VerticalAlignment.Bottom;
            requestBlock.InsertText($"SutureHealth DocID: {footerId} eSigned with SutureSign™. Powered by SutureHealth™. support@suturehealth.com");            

            editor.Position.Translate(0, editor.Root.Size.Height - 20);
            requestBlock.Draw(editor, new Rect(0, 0, editor.Root.Size.Width, 20));            
        }

        public static void DrawRejectionWatermark(this FixedContentEditor editor)
        {
            var block = new Block();

            block.HorizontalAlignment = HorizontalAlignment.Center;
            block.VerticalAlignment = VerticalAlignment.Center;
            block.GraphicProperties.FillColor = Constants.REJECTION_WATERMARK_COLOR;
            block.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
            block.TextProperties.FontSize = 210 * (editor.Root.Size.Height / 1100); // Adjust font size based on page height
            block.TextProperties.RenderingMode = RenderingMode.Fill;
            block.InsertText("REJECTED");

            using (editor.SavePosition())
            {
                editor.Position.Translate(-editor.Root.Size.Width * 0.5, -editor.Root.Size.Height * 0.5);
                editor.Position.RotateAt(-55, editor.Root.Size.Width, editor.Root.Size.Height);
                editor.DrawBlock(block, new Size(editor.Root.Size.Width * 2, editor.Root.Size.Height * 2));
            }
        }

        public static void DrawRejectionAttributes(this FixedContentEditor editor, RejectedPdfAttributes attributes)
        {
            var processBlock = new Block();
            var reasonBlock = new Block();

            using (editor.SavePosition())
            {
                editor.Position.Translate(307, 196);
                processBlock.TextProperties.Font = SutureFontsRepository.Arial;
                processBlock.TextProperties.FontSize = 16;
                processBlock.LineSpacing = 1.73;
                processBlock.InsertText($"{attributes.DateProcessed:HH:mm:ss K} on {attributes.DateProcessed.DateTime.ToShortDateString()}");
                processBlock.InsertLineBreak();
                processBlock.InsertText(attributes.ProcessedBy ?? string.Empty);
                processBlock.InsertLineBreak();
                processBlock.InsertText(attributes.ProcessingOffice ?? string.Empty);
                processBlock.InsertLineBreak();
                processBlock.InsertText(attributes.ProcessingOfficePhone ?? string.Empty);
                editor.DrawBlock(processBlock, new Size(500, 135));

                editor.Position.Translate(307, 324);
                reasonBlock.TextProperties.Font = SutureFontsRepository.Arial;
                reasonBlock.TextProperties.FontSize = 16;
                reasonBlock.InsertText(attributes.RejectionReason);
                editor.DrawBlock(reasonBlock, new Size(390, 325));
            }
        }

        public static void DrawFaceToFaceAttributes(this FixedContentEditor editor, FaceToFaceAttributes attributes)
        {
            Block headerBlock = new Block(),
                  patientBlock = new Block(),
                  effectiveDateBlock = new Block(),
                  encounterDateBlock = new Block(),
                  medicalConditionBlock = new Block(),
                  clinicalReasonBlock = new Block(),
                  homeBoundBlock = new Block(),
                  treatmentPlanBlock = new Block(),
                  signatureBlock = new Block(),
                  signatureDateBlock = new Block(),
                  signatureFooterBlock = new Block(),
                  docIdBlock = new Block();

            using (editor.SavePosition())
            {
                editor.Position.Translate(170, 110);
                headerBlock.HorizontalAlignment = HorizontalAlignment.Center;
                headerBlock.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
                headerBlock.LineSpacing = 1.2;
                headerBlock.InsertText($"From the Office of: {attributes.Signature}");
                headerBlock.InsertLineBreak();
                headerBlock.InsertText($"Prepared For: {attributes.SendingOrganizationName}");
                editor.DrawBlock(headerBlock, new Size(475, 45));

                editor.Position.Translate(315, 174);
                patientBlock.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
                patientBlock.InsertText(attributes.Patient);
                editor.DrawBlock(patientBlock, new Size(450, 20));

                editor.Position.Translate(275, 192);
                effectiveDateBlock.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
                effectiveDateBlock.InsertText(attributes.EpisodeEffectiveDate);
                editor.DrawBlock(effectiveDateBlock, new Size(470, 20));

                editor.Position.Translate(380, 254);
                encounterDateBlock.InsertText(attributes.EncounterDate);
                editor.DrawBlock(encounterDateBlock, new Size(370, 20));

                editor.Position.Translate(95, 320);
                medicalConditionBlock.InsertText(attributes.MedicalCondition);
                editor.DrawBlock(medicalConditionBlock, new Size(650, 40));

                if (string.IsNullOrWhiteSpace(attributes.TreatmentPlan))
                {
                    editor.Position.Translate(95, 445);
                    clinicalReasonBlock.InsertText(attributes.ClinicalReasonForHomeCare);
                    editor.DrawBlock(clinicalReasonBlock, new Size(650, 110));

                    editor.Position.Translate(95, 690);
                    homeBoundBlock.InsertText(attributes.ReasonForBeingHomebound);
                    editor.DrawBlock(homeBoundBlock, new Size(650, 120));
                }
                else
                {
                    editor.Position.Translate(95, 570);
                    clinicalReasonBlock.InsertText(attributes.ClinicalReasonForHomeCare);
                    editor.DrawBlock(clinicalReasonBlock, new Size(650, 115));

                    editor.Position.Translate(95, 720);
                    homeBoundBlock.InsertText(attributes.ReasonForBeingHomebound);
                    editor.DrawBlock(homeBoundBlock, new Size(650, 95));

                    editor.Position.Translate(95, 445);
                    treatmentPlanBlock.InsertText(attributes.TreatmentPlan);
                    editor.DrawBlock(treatmentPlanBlock, new Size(650, 105));
                }

                editor.Position.Translate(95, 875);
                signatureBlock.TextProperties.RenderingMode = RenderingMode.Fill;
                signatureBlock.GraphicProperties.FillColor = Constants.COLOR_BLACK;
                signatureBlock.TextProperties.FontSize = 21;
                signatureBlock.TextProperties.Font = SutureFontsRepository.FreestyleScript;
                signatureBlock.InsertText(attributes.Signature);
                editor.DrawBlock(signatureBlock, new Size(250, 27));

                editor.Position.Translate(588, 865);
                signatureDateBlock.TextProperties.RenderingMode = RenderingMode.Fill;
                signatureDateBlock.GraphicProperties.FillColor = Constants.COLOR_BLACK;
                signatureDateBlock.TextProperties.FontSize = 13;
                signatureDateBlock.TextProperties.Font = SutureFontsRepository.FreestyleScript;
                signatureDateBlock.VerticalAlignment = VerticalAlignment.Center;
                signatureDateBlock.LineSpacing = 1.1;
                signatureDateBlock.InsertText(attributes.DateSigned.DateTime.ToShortDateString());
                signatureDateBlock.InsertLineBreak();
                signatureDateBlock.InsertText(attributes.DateSigned.ToString("HH:mm:ss zzz"));
                editor.DrawBlock(signatureDateBlock, new Size(100, 35));

                editor.Position.Translate(97, 905);
                signatureFooterBlock.LineSpacing = 0.95;
                signatureFooterBlock.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
                signatureFooterBlock.TextProperties.FontSize = 13;
                signatureFooterBlock.InsertText($"{attributes.Signature}{(!string.IsNullOrWhiteSpace(attributes.Npi) ? $" (NPI: {attributes.Npi})" : string.Empty)}");
                signatureFooterBlock.InsertLineBreak();
                signatureFooterBlock.TextProperties.FontSize = 10;
                signatureFooterBlock.InsertText($"({attributes.DateSigned:MM/dd/yyyy HH:mm:ss zzz}) eSign ID: {attributes.Pid}");
                editor.DrawBlock(signatureFooterBlock, new Size(550, 40));

                editor.Position.Translate(97, 955);
                docIdBlock.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
                docIdBlock.TextProperties.FontSize = 11;
                docIdBlock.InsertText($"SutureHealth DocID: {attributes.RequestId}");
                editor.DrawBlock(docIdBlock, new Size(200, 20));

                if (attributes.NursingRequired)
                {
                    var block = new Block();

                    block.TextProperties.RenderingMode = RenderingMode.Fill;
                    block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
                    block.TextProperties.FontSize = 13;
                    block.TextProperties.Font = SutureFontsRepository.ZapfDingbats;
                    block.VerticalAlignment = VerticalAlignment.Center;
                    block.HorizontalAlignment = HorizontalAlignment.Center;
                    block.InsertText("3");

                    editor.Position.Translate(97, 397);
                    editor.DrawBlock(block);
                }
                if (attributes.PhysicialTherapyRequired)
                {
                    var block = new Block();

                    block.TextProperties.RenderingMode = RenderingMode.Fill;
                    block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
                    block.TextProperties.FontSize = 13;
                    block.TextProperties.Font = SutureFontsRepository.ZapfDingbats;
                    block.VerticalAlignment = VerticalAlignment.Center;
                    block.HorizontalAlignment = HorizontalAlignment.Center;
                    block.InsertText("3");

                    editor.Position.Translate(241, 397);
                    editor.DrawBlock(block);
                }
                if (attributes.SpeechTherapyRequired)
                {
                    var block = new Block();

                    block.TextProperties.RenderingMode = RenderingMode.Fill;
                    block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
                    block.TextProperties.FontSize = 13;
                    block.TextProperties.Font = SutureFontsRepository.ZapfDingbats;
                    block.VerticalAlignment = VerticalAlignment.Center;
                    block.HorizontalAlignment = HorizontalAlignment.Center;
                    block.InsertText("3");

                    editor.Position.Translate(433, 397);
                    editor.DrawBlock(block);
                }
                if (attributes.OccupationalTherapyRequired)
                {
                    var block = new Block();

                    block.TextProperties.RenderingMode = RenderingMode.Fill;
                    block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
                    block.TextProperties.FontSize = 13;
                    block.TextProperties.Font = SutureFontsRepository.ZapfDingbats;
                    block.VerticalAlignment = VerticalAlignment.Center;
                    block.HorizontalAlignment = HorizontalAlignment.Center;
                    block.InsertText("3");

                    editor.Position.Translate(588, 397);
                    editor.DrawBlock(block);
                }
            }
        }

        private static void DrawAnnotationBoundingBox(this FixedContentEditor editor, Size size, ColorBase backgroundColor, ColorBase borderColor = null)
        {
            editor.SaveGraphicProperties();

            editor.GraphicProperties.FillColor = backgroundColor;
            editor.GraphicProperties.StrokeColor = borderColor ?? Constants.BORDER_COLOR_GREY;
            editor.DrawRectangle(new Rect(0, 0, size.Width, size.Height));

            editor.RestoreGraphicProperties();
        }

        private static void DrawAnnotationSignatureText(this FixedContentEditor editor, Size size, string signature, string signatureId, DateTimeOffset dateSigned)
        {
            var block = new Block();

            block.TextProperties.RenderingMode = RenderingMode.Fill;
            block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
            block.TextProperties.FontSize = 21;
            block.TextProperties.Font = SutureFontsRepository.FreestyleScript;
            block.VerticalAlignment = VerticalAlignment.Bottom;
            block.InsertText(signature);

            block.InsertLineBreak();

            block.LineSpacing = 0.95;
            block.TextProperties.FontSize = 8;
            block.TextProperties.Font = SutureFontsRepository.ArialBold;           

            block.InsertText($"({dateSigned:MM/dd/yyyy HH:mm:ss zzz}) eSign ID: {signatureId}");

            editor.DrawBlock(block);
        }

        private static void DrawAnnotationSignaturePlaceholder(this FixedContentEditor editor, Size size)
        {
            var block = new Block();

            block.TextProperties.RenderingMode = RenderingMode.Fill;
            block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
            block.TextProperties.FontSize = 13;
            block.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
            block.VerticalAlignment = VerticalAlignment.Bottom;
            block.InsertText("Signature");
            editor.DrawBlock(block, size);
        }

        private static void DrawAnnotationDateTimePlaceholder(this FixedContentEditor editor, Size size)
        {
            var block = new Block();

            block.TextProperties.RenderingMode = RenderingMode.Fill;
            block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
            block.TextProperties.FontSize = 11;
            block.TextProperties.TrySetFont(new FontFamily("Helvetica"), FontStyles.Normal, FontWeights.Bold);
            block.VerticalAlignment = VerticalAlignment.Center;
            block.LineSpacing = 1.2;
            block.InsertText("Datetime");
            block.InsertLineBreak();
            block.InsertText("Signed");
            editor.DrawBlock(block, size);
        }

        private static void DrawAnnotationTextAreaText(this FixedContentEditor editor, Size size, string text)
        {
            var block = new Block();

            block.TextProperties.RenderingMode = RenderingMode.Fill;
            block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
            block.TextProperties.FontSize = 12;
            block.InsertText(new FontFamily("Courier"), FontStyles.Normal, FontWeights.Normal, text ?? string.Empty);
            editor.DrawBlock(block, size);
        }

        private static void DrawAnnotationDateTimeText(this FixedContentEditor editor, Size size, DateTimeOffset date)
        {
            var block = new Block();

            block.TextProperties.RenderingMode = RenderingMode.Fill;
            block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
            block.TextProperties.FontSize = 13;
            block.TextProperties.Font = SutureFontsRepository.FreestyleScript;
            block.VerticalAlignment = VerticalAlignment.Center;
            block.LineSpacing = 1.1;
            block.InsertText(date.DateTime.ToShortDateString());
            block.InsertLineBreak();
            block.InsertText(date.ToString("HH:mm:ss zzz"));
            editor.DrawBlock(block, size);
        }

        private static void DrawAnnotationCheckboxValue(this FixedContentEditor editor, Size size, bool @checked)
        {
            var block = new Block();

            block.TextProperties.RenderingMode = RenderingMode.Fill;
            block.GraphicProperties.FillColor = Constants.COLOR_BLACK;
            block.TextProperties.FontSize = 13;
            block.TextProperties.Font = SutureFontsRepository.ZapfDingbats;
            block.VerticalAlignment = VerticalAlignment.Center;
            block.HorizontalAlignment = HorizontalAlignment.Center;
            block.InsertText(@checked ? "3" : string.Empty);
            editor.DrawBlock(block, size);
        }
    }
}
