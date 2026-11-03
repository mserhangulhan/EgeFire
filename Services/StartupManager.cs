using Microsoft.Win32;
using System;
using System.Reflection;

namespace EasyTaskStarter.Services
{
    public class StartupManager
    {
        private const string AppName = "EgeFire";
        private const string RegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static void SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKey, true))
                {
                    if (enable)
                    {
                        string exePath = (Environment.ProcessPath
                            ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                            ?? Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"))!;
                        key?.SetValue(AppName, $"\"{exePath}\" --autorun");
                    }
                    else
                    {
                        key?.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Başlangıç ayarı yapılamadı: {ex.Message}", "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static bool IsStartupEnabled()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKey, false))
                {
                    return key?.GetValue(AppName) != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}