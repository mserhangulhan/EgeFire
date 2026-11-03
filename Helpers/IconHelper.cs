using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace EasyTaskStarter.Helpers
{
    public static class IconHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, string lpIconPath, out ushort lpiIcon);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Verilen exe / lnk yolundan 16x16 BitmapSource döndürür.
        /// Hata durumunda null döner.
        /// </summary>
        public static BitmapSource? GetIcon(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Store uygulamaları için path boş olabilir — null dön
            if (!System.IO.File.Exists(path))
                return null;

            try
            {
                using var icon = Icon.ExtractAssociatedIcon(path);
                if (icon == null) return null;

                return Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                return null;
            }
        }
    }
}
