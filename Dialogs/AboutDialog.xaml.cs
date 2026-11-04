using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using EasyTaskStarter.Services;

namespace EasyTaskStarter.Dialogs
{
    public partial class AboutDialog : Window
    {
        public string AppVersion { get; }

        public AboutDialog()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "3.0.0";
            AppVersion = $"v{ver}";
            InitializeComponent();
        }

        private void AboutDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
            VersionText.Text = $"Masaüstü Görev & Kısayol Yöneticisi — {AppVersion}";
        }

        private void ApplyTheme()
        {
            var config = ConfigManager.LoadConfig();
            var dict = Resources.MergedDictionaries;
            dict.Clear();

            if (config.Theme == "Light" || (config.Theme == "System" && !IsSystemDarkTheme()))
            {
                dict.Add(new ResourceDictionary { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) });
            }
            else
            {
                dict.Add(new ResourceDictionary { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) });
            }
        }

        private bool IsSystemDarkTheme()
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                return value != null && (int)value == 0;
            }
            catch
            {
                return false;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch
            {
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
