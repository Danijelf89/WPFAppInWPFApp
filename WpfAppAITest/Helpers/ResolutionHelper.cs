using System.Runtime.InteropServices;

namespace WpfAppAITest.Helpers
{
    public static class ResolutionHelper
    {
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        const int DESKTOPHORZRES = 118; // Native width
        const int DESKTOPVERTRES = 117; // Native height

        public static Size GetNativeResolution(Screen screen)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hdc = g.GetHdc();
                int nativeWidth = GetDeviceCaps(hdc, DESKTOPHORZRES);
                int nativeHeight = GetDeviceCaps(hdc, DESKTOPVERTRES);
                g.ReleaseHdc(hdc);

                return new Size(nativeWidth, nativeHeight);
            }
        }

        public static Point GetRealLocation(Point boundsFromWindows, Size nativeScreenSize)
        {
            var realBounds = boundsFromWindows;
            if(realBounds.X > nativeScreenSize.Width)
            {
                realBounds.X = nativeScreenSize.Width;
            }
            if (realBounds.Y > nativeScreenSize.Height)
            {
                realBounds.Y = nativeScreenSize.Height;
            }
            return realBounds;
        }
    }
}
