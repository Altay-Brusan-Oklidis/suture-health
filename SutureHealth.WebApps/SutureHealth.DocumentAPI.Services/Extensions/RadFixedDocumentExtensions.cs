using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using Telerik.Windows.Documents.Fixed.Model.Annotations;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Fixed.Model.InteractiveForms;
using Telerik.Windows.Documents.Fixed.Model.Resources;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using SutureHealth.Documents.Services.Docnet;

namespace SutureHealth.Documents.Services.Extensions
{
    public static class RadFixedDocumentExtensions
    {
        private static PdfFormatProvider Provider { get; }

        static RadFixedDocumentExtensions()
        {
            Provider = new PdfFormatProvider();
        }

        public static RadFixedDocument OpenBinary(byte[] data)
        {
            var document = Provider.Import(data);

            // #2846: There are cases where Telerik can import a PDF but not export; validate here to see if we need to open a rasterized version.
            try
            {
                Provider.Export(document);
            }
            catch
            {
                document = Provider.Import(ImageProcessing.RasterizePdf(data));
            }

            return document;
        }

        public static byte[] SaveBinary(this RadFixedDocument document)
        {
            return Provider.Export(document);
        }

        public static byte[] SavePageToPng(this RadFixedDocument document, int pageIndex, int dpi)
        {
            return ImageProcessing.SavePdfPageToPng(Provider.Export(document), pageIndex, dpi, out _);
        }

        public static void AppendRejectionTemplate(this RadFixedDocument document, RejectedPdfAttributes attributes)
        {
            RadFixedDocument template;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SutureHealth.Documents.Services.Assets.RejectionTemplate.pdf"))
            {
                template = Provider.Import(stream);
            }

            new FixedContentEditor(template.Pages.First()).DrawRejectionAttributes(attributes);

            document.Merge(template);
        }

        public static RadFixedDocument OpenFaceToFaceTemplate(FaceToFaceTemplateType templateType)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(templateType switch
            {
                FaceToFaceTemplateType.General => "SutureHealth.Documents.Services.Assets.F2F1000Template.pdf",
                FaceToFaceTemplateType.WithTreatmentPlan => "SutureHealth.Documents.Services.Assets.F2F1001Template.pdf",
                _ => throw new InvalidOperationException()
            });

            return Provider.Import(stream);
        }

        #region Flatten PDF
        // https://docs.telerik.com/devtools/document-processing/knowledge-base/flatten-form-fields
        // NOTE: The framework has a method on its AcroForm property that should provide this functionality, but
        //       as of writing it does not work correctly.
        public static void FlattenFormFields(this RadFixedDocument document)
        {
            if (document.AcroForm.FormFields.Count == 0)
            {
                return;
            }

            foreach (RadFixedPage page in document.Pages)
            {
                List<Widget> widgetsToRemove = new List<Widget>();
                FixedContentEditor pageEditor = new FixedContentEditor(page);

                foreach (Annotation annotation in page.Annotations)
                {
                    if (annotation.Type == global::Telerik.Windows.Documents.Fixed.Model.Annotations.AnnotationType.Widget)
                    {
                        Widget widget = (Widget)annotation;
                        FlattenWidgetAppearance(pageEditor, widget);
                        widgetsToRemove.Add(widget);
                    }
                }

                foreach (Widget widget in widgetsToRemove)
                {
                    page.Annotations.Remove(widget);
                }
            }

            foreach (FormField field in document.AcroForm.FormFields.ToArray())
            {
                document.AcroForm.FormFields.Remove(field);
            }
        }

        private static void FlattenWidgetAppearance(FixedContentEditor pageEditor, Widget widget)
        {
            FormSource widgetAppearance = GetWidgetNormalAppearance(widget);

            if (widgetAppearance == null)
            {
                return;
            }

            pageEditor.Position.Translate(widget.Rect.Left, widget.Rect.Top);
            pageEditor.DrawForm(widgetAppearance, widget.Rect.Width, widget.Rect.Height);
        }

        private static FormSource GetWidgetNormalAppearance(Widget widget)
        {
            FormSource widgetAppearance;
            switch (widget.WidgetContentType)
            {
                case WidgetContentType.PushButtonContent:
                    widgetAppearance = ((PushButtonWidget)widget).Content.NormalContentSource;
                    break;
                case WidgetContentType.SignatureContent:
                    widgetAppearance = ((SignatureWidget)widget).Content.NormalContentSource;
                    break;
                case WidgetContentType.VariableContent:
                    widgetAppearance = ((VariableContentWidget)widget).Content.NormalContentSource;
                    break;
                case WidgetContentType.TwoStatesContent:
                    TwoStatesButtonWidget twoStatesWidget = (TwoStatesButtonWidget)widget;
                    widgetAppearance = GetTwoStatesWidgetNormalAppearance(twoStatesWidget);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Not supported widget content type {0}", widget.WidgetContentType));
            }

            return widgetAppearance;
        }

        private static FormSource GetTwoStatesWidgetNormalAppearance(TwoStatesButtonWidget twoStatesWidget)
        {
            FormField field = twoStatesWidget.Field;
            bool isOnState;

            switch (field.FieldType)
            {
                case FormFieldType.CheckBox:
                    CheckBoxField checkBox = (CheckBoxField)field;
                    isOnState = checkBox.IsChecked;
                    break;
                case FormFieldType.RadioButton:
                    RadioButtonField radio = (RadioButtonField)field;
                    RadioButtonWidget radioWidget = (RadioButtonWidget)twoStatesWidget;

                    if (radio.ShouldUpdateRadiosInUnison)
                    {
                        isOnState = radio.Value != null && radio.Value.Value.Equals(radioWidget.Option.Value);
                    }
                    else
                    {
                        isOnState = radio.Value == radioWidget.Option;
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format("Not supported field type {0} for TwoStateButtonWidget", field.FieldType));
            }

            FormSource widgetAppearance = (isOnState ? twoStatesWidget.OnStateContent : twoStatesWidget.OffStateContent).NormalContentSource;

            return widgetAppearance;
        }
        #endregion

        public enum FaceToFaceTemplateType
        {
            Unknown = 0,
            General = 1000,
            WithTreatmentPlan = 1001
        }
    }
}
