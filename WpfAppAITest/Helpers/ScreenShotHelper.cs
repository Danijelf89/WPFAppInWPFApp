using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Application;
using Point = System.Windows.Point;
using System.Windows.Forms.Integration;
using Image = System.Windows.Controls.Image;


namespace WpfAppAITest.Helpers
{
    public static class ScreenShotHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h,
            IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        private const int SRCCOPY = 0x00CC0020;

        private static BitmapSource CaptureRegion(Rect region)
        {
            int width = (int)region.Width;
            int height = (int)region.Height;
            IntPtr desktopHdc = GetDC(IntPtr.Zero);
            IntPtr memoryHdc = CreateCompatibleDC(desktopHdc);
            IntPtr bitmap = CreateCompatibleBitmap(desktopHdc, width, height);
            IntPtr oldBitmap = SelectObject(memoryHdc, bitmap);

            // Screenshot dela ekrana gde se nalazi Grid1
            BitBlt(memoryHdc, 0, 0, width, height, desktopHdc, (int)region.X, (int)region.Y, SRCCOPY);

            BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Oslobodi resurse
            SelectObject(memoryHdc, oldBitmap);
            DeleteObject(bitmap);
            DeleteDC(memoryHdc);
            ReleaseDC(IntPtr.Zero, desktopHdc);

            return bmpSource;
        }

        public static void CaptureGridAndSetAsBackground(Image source, System.Windows.Controls.Panel target)
        {
            // Dobijanje apsolutne pozicije Grid1 na ekranu
            Point screenPos = source.PointToScreen(new Point(0, 0));
            Rect captureRegion = new Rect(screenPos.X, screenPos.Y, source.ActualWidth, source.ActualHeight); // Specifikuj dimenzije za screenshot

            // Uhvati screenshot tog regiona
            BitmapSource screenshot = CaptureRegion(captureRegion);

            // Kreiraj ImageBrush sa slikom
            ImageBrush brush = new ImageBrush
            {
                ImageSource = screenshot,
                Stretch = Stretch.Uniform // Stretch.Fill osigurava da slika popuni ceo prostor
            };

            // Postavi brush kao pozadinu za Panel ili Grid
            target.Background = brush;



        }

        public static void CaptureImageScreenshot(Image? sharedImage, Canvas canvas)
        {
            if (sharedImage?.Source == null)
                return;

            // Renderuj Image kontrolu u bitmapu
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)sharedImage.ActualWidth,
                (int)sharedImage.ActualHeight,
                96, 96, PixelFormats.Pbgra32);

            rtb.Render(sharedImage);

            // Kreiraj ImageBrush od RenderTargetBitmap-a
            ImageBrush brush = new ImageBrush(rtb);

            // Postavi kao pozadinu Canvas-a
            canvas.Background = brush;
        }


        private static BitmapSource ResizeBitmap(BitmapSource source, int width, int height)
        {
            RenderTargetBitmap resizedBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawImage(source, new Rect(0, 0, width, height));
            }
            resizedBitmap.Render(drawingVisual);

            return resizedBitmap;
        }

    }
}
