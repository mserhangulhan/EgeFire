using System;
using System.Diagnostics;

namespace EasyTaskStarter.Services
{
    public class ProcessLauncher
    {
        public static void LaunchApp(string exePath, string arguments = "")
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Uygulama başlatılamadı: {ex.Message}", "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static void LaunchAppAsAdmin(string exePath, string arguments = "")
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Yönetici olarak başlatılamadı: {ex.Message}", "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static void LaunchStoreApp(string appxId)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $@"shell:AppsFolder\{appxId}",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Store uygulaması başlatılamadı: {ex.Message}", "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static void OpenFolder(string folderPath)
        {
            try
            {
                Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Klasör açılamadı: {ex.Message}", "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static void OpenFileLocation(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
                else if (System.IO.Directory.Exists(filePath))
                {
                    Process.Start("explorer.exe", filePath);
                }
                else
                {
                    // Fallback: Klasörünü açmayı dene
                    string? dir = System.IO.Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
                    {
                        Process.Start("explorer.exe", dir);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Dosya konumu açılamadı: {ex.Message}", "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
