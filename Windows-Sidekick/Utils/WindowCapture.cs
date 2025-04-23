// path: ./WindowsSidekick/Utils/WindowCapture.cs
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsSidekick.Utils
{
    public static class WindowCapture
    {
        // Win32 & DWM APIs
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(
            IntPtr hwnd, int attribute, out RECT pvAttribute, int cbAttribute);
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;          // true window bounds (no shadows)
        private const uint PW_RENDERFULLCONTENT = 0x00000002;  // include composition surfaces

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        /// <summary>
        /// Captures the specified window or full desktop if the handle is invalid,
        /// minimized, hidden, or corresponds to the desktop shell itself.
        /// </summary>
        public static Bitmap Capture(IntPtr hwnd)
        {
            // 1) Reject invalid, minimized, invisible, or desktop-shell windows
            if (hwnd == IntPtr.Zero
                || IsIconic(hwnd)
                || !IsWindowVisible(hwnd)
                || IsDesktopWindow(hwnd))
            {
                Debug.WriteLine("Capture: FullScreen (invalid/minimized/desktop)");
                return CaptureFullScreen();
            }

            // 2) Obtain true on-screen bounds via DWM (fall back to GetWindowRect)
            if (DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out var rect, Marshal.SizeOf<RECT>()) != 0)
            {
                Debug.WriteLine("DWM bounds failed; using GetWindowRect");
                GetWindowRect(hwnd, out rect);
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            if (width <= 0 || height <= 0)
            {
                Debug.WriteLine($"Invalid size ({width}×{height}); full-screen fallback");
                return CaptureFullScreen();
            }

            // 3) Allow partial off-screen capture (intersection with virtual screen)
            var windowRect = new Rectangle(rect.Left, rect.Top, width, height);
            var desktop = SystemInformation.VirtualScreen;
            var captureRect = Rectangle.Intersect(desktop, windowRect);
            if (captureRect.IsEmpty)
            {
                Debug.WriteLine("Window outside virtual screen; full-screen fallback");
                return CaptureFullScreen();
            }

            // 4) Try PrintWindow with full-content flag
            var bmp = new Bitmap(captureRect.Width, captureRect.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();
                bool ok = PrintWindow(hwnd, hdc, PW_RENDERFULLCONTENT);
                g.ReleaseHdc(hdc);
                if (ok)
                {
                    Debug.WriteLine("Capture: PrintWindow succeeded");
                    return bmp;
                }
                Debug.WriteLine("PrintWindow failed; falling back to CopyFromScreen");
            }

            // 5) Fallback to CopyFromScreen
            bmp.Dispose();
            bmp = new Bitmap(captureRect.Width, captureRect.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(captureRect.Location, Point.Empty, captureRect.Size, CopyPixelOperation.SourceCopy);
                Debug.WriteLine("Capture: CopyFromScreen succeeded");
            }
            return bmp;
        }

        /// <summary>
        /// Captures the entire virtual desktop (all monitors).
        /// </summary>
        public static Bitmap CaptureFullScreen()
        {
            var vs = SystemInformation.VirtualScreen;
            var bmp = new Bitmap(vs.Width, vs.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(vs.Location, Point.Empty, vs.Size, CopyPixelOperation.SourceCopy);
                Debug.WriteLine("CaptureFullScreen: CopyFromScreen");
            }
            return bmp;
        }

        /// <summary>
        /// Detects the desktop background shell windows (Progman, WorkerW).
        /// </summary>
        private static bool IsDesktopWindow(IntPtr hwnd)
        {
            const int maxClass = 64;
            var sb = new StringBuilder(maxClass);
            if (GetClassName(hwnd, sb, maxClass) > 0)
            {
                string cls = sb.ToString();
                if (cls.Equals("Progman", StringComparison.OrdinalIgnoreCase) ||
                    cls.Equals("WorkerW", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
