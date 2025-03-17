using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace WpfAppAITest.Helpers
{
    public static class ScreenCaptureHelper
    {
        public static BitmapSource CaptureScreen(Screen screen)
        {
            var bounds = screen.Bounds;
            var nativeBounds = ResolutionHelper.GetNativeResolution(screen);
            var realLocation = ResolutionHelper.GetRealLocation(bounds.Location, nativeBounds);
            using var bitmap = new Bitmap(nativeBounds.Width, nativeBounds.Height);
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(realLocation, System.Drawing.Point.Empty, nativeBounds);

            return ConvertBitmapToImageSource(bitmap);
        }



        private static BitmapSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}
