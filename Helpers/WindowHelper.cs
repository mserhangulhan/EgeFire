using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace EasyTaskStarter.Helpers
{
    public static class WindowHelper
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern nint SetWindowLong(IntPtr hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern nint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int LWA_ALPHA = 0x00000002;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        public static void ApplyOpacity(Window window, double opacity)
        {
            if (window == null) return;
            
            window.Opacity = opacity;
        }

        public static void SetAlwaysOnTop(Window window, bool alwaysOnTop)
        {
            if (alwaysOnTop)
            {
                window.Topmost = true;
            }
            else
            {
                window.Topmost = false;
            }
        }

        public static void SetTransparentClick(Window window)
        {
            if (window == null) return;

            HwndSource source = (HwndSource)PresentationSource.FromVisual(window);
            if (source != null)
            {
                var hwnd = source.Handle;
                nint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            }
        }
    }
}