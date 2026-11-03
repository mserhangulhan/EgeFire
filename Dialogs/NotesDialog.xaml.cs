using System.Windows;
using EasyTaskStarter.Services;

namespace EasyTaskStarter.Dialogs
{
    public partial class NotesDialog : Window
    {
        public string NotesText { get; private set; } = "";

        public NotesDialog(string initialNotes)
        {
            InitializeComponent();
            NotesTextBox.Text = initialNotes;
            NotesTextBox.Focus();
            NotesTextBox.CaretIndex = NotesTextBox.Text.Length;
        }

        private void NotesDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            NotesText = NotesTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}