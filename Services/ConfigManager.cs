using System;
using System.IO;
using System.Text.Json;
using EasyTaskStarter.Models;

namespace EasyTaskStarter.Services
{
    public class ConfigManager
    {
        private static string? _configDirectory;

        private static string ConfigDirectory
        {
            get
            {
                if (_configDirectory != null) return _configDirectory;

                // Önce C:\EgeFire dene (admin gerekirse fallback'e düşer)
                string primaryPath = @"C:\EgeFire";
                string fallbackPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EgeFire");

                try
                {
                    if (!Directory.Exists(primaryPath))
                    {
                        Directory.CreateDirectory(primaryPath);
                    }
                    // Yazma testi
                    string testFile = Path.Combine(primaryPath, ".write_test");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    _configDirectory = primaryPath;
                }
                catch
                {
                    // C:\ yazılamazsa AppData'ya düş
                    if (!Directory.Exists(fallbackPath))
                    {
                        Directory.CreateDirectory(fallbackPath);
                    }
                    _configDirectory = fallbackPath;
                }

                return _configDirectory;
            }
        }

        private static string ConfigFilePath => Path.Combine(ConfigDirectory, "config.json");

        public static AppConfig LoadConfig()
        {
            try
            {
                string dir = ConfigDirectory;
                string file = ConfigFilePath;

                if (!File.Exists(file))
                {
                    var defaultConfig = CreateDefaultConfig();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(file);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? CreateDefaultConfig();
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string dir = ConfigDirectory;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Config kaydedilemedi: {ex.Message}", "Hata", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static void ExportConfig(string targetFilePath, AppConfig config)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(targetFilePath, json);
        }

        public static AppConfig ImportConfig(string sourceFilePath)
        {
            string json = File.ReadAllText(sourceFilePath);
            var imported = JsonSerializer.Deserialize<AppConfig>(json);
            if (imported == null)
                throw new InvalidOperationException("Geçersiz yedek dosyası.");
            return imported;
        }

        private static AppConfig CreateDefaultConfig()
        {
            var config = new AppConfig();
            
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            config.QuickAccess.Add(new QuickAccessItem("Bu bilgisayar", "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"));
            config.QuickAccess.Add(new QuickAccessItem("Belgeler", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            config.QuickAccess.Add(new QuickAccessItem("Masaüstü", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
            config.QuickAccess.Add(new QuickAccessItem("İndirilenler", Path.Combine(userProfile, "Downloads")));

            return config;
        }
    }
}