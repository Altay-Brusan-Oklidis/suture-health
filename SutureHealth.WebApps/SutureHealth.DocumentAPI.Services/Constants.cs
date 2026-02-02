using Telerik.Windows.Documents.Fixed.Model.ColorSpaces;

namespace SutureHealth.Documents.Services
{
    public static class Constants
    {
        public static readonly RgbColor COLOR_BLACK = new RgbColor(0, 0, 0);
        public static readonly RgbColor BORDER_COLOR_RED = new RgbColor(255, 0, 0);
        public static readonly RgbColor BORDER_COLOR_GREY = new RgbColor(140, 140, 140);
        public static readonly RgbColor BACKGROUND_COLOR_YELLOW = new RgbColor(96, 255, 255, 204);
        public static readonly RgbColor BACKGROUND_COLOR_DARK_YELLOW = new RgbColor(96, 244, 245, 8);
        public static readonly RgbColor BACKGROUND_COLOR_RED = new RgbColor(114, 241, 106, 106);
        public static readonly RgbColor BACKGROUND_COLOR_BLUE = new RgbColor(96, 212, 224, 240);
        public static readonly RgbColor REJECTION_WATERMARK_COLOR = new RgbColor(192, 232, 232, 232);

        public const double IMAGE_DPI_SCALING_MULTIPLIER = (1 / 0.72) / 100;
        public const double ABCPDF_TO_TELERIK_SCALING_MULTIPLIER = 4 / 3d;
        public const double HTML_WIDTH_SCALING_MULTIPLIER = 0.959;
        public const double HTML_HEIGHT_SCALING_MULTIPLIER = 0.956;
    }
}
