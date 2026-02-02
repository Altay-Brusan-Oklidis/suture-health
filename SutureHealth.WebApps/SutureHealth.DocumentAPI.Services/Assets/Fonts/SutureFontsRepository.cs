using System.IO;
using System.Reflection;
using Telerik.Documents.Core.Fonts;
using Telerik.Windows.Documents.Fixed.Model.Fonts;

namespace SutureHealth.Documents.Services.Assets.Fonts
{
    public static class SutureFontsRepository
    {
        private const string FONT_NAME_FREESTYLE_SCRIPT = "Freestyle Script";
        private const string FONT_NAME_ZAPF_DINGBATS = "Dingbats";
        private const string FONT_NAME_ARIAL = "Arial";
        private const string FONT_NAME_ARIAL_BOLD = "ArialBold";

        public static FontBase FreestyleScript =>
            FontsRepository.TryCreateFont(new FontFamily(FONT_NAME_FREESTYLE_SCRIPT), out var font) ? font : null;

        public static FontBase ZapfDingbats =>
            FontsRepository.TryCreateFont(new FontFamily(FONT_NAME_ZAPF_DINGBATS), out var font) ? font : null;

        public static FontBase Arial =>
            FontsRepository.TryCreateFont(new FontFamily(FONT_NAME_ARIAL), out var font) ? font : null;
        public static FontBase ArialBold =>
            FontsRepository.TryCreateFont(new FontFamily(FONT_NAME_ARIAL_BOLD), out var font) ? font : null;

        static SutureFontsRepository()
        {
            FontsRepository.RegisterFont(new FontFamily(FONT_NAME_FREESTYLE_SCRIPT), FontStyles.Normal, FontWeights.Normal, ReadEmbeddedFont("FreestyleScript"));
            FontsRepository.RegisterFont(new FontFamily(FONT_NAME_ZAPF_DINGBATS), FontStyles.Normal, FontWeights.Normal, ReadEmbeddedFont("ZapfDingbats"));
            FontsRepository.RegisterFont(new FontFamily(FONT_NAME_ARIAL), FontStyles.Normal, FontWeights.Normal, ReadEmbeddedFont("Arial"));
            FontsRepository.RegisterFont(new FontFamily(FONT_NAME_ARIAL_BOLD), FontStyles.Normal, FontWeights.Normal, ReadEmbeddedFont("ArialBold"));
        }

        private static byte[] ReadEmbeddedFont(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream($"SutureHealth.Documents.Services.Assets.Fonts.{name}.ttf"))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
