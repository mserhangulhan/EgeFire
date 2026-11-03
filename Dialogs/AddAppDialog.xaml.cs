using Microsoft.Win32;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Windows;

namespace EasyTaskStarter.Dialogs
{
    public partial class AddAppDialog : Window
    {
        public AddAppDialog()
        {
            InitializeComponent();
        }

        public AddAppDialog(string name, string exePath, string arguments, string appxId = "")
        {
            InitializeComponent();
            Title = "Kısayolu Düzenle";
            BtnAddOrSave.Content = "Kaydet";
            AppNameTextBox.Text = name;
            ExePathTextBox.Text = exePath;
            ArgumentsTextBox.Text = arguments;
            AppxId = appxId;
        }

        public string AppName => AppNameTextBox.Text;
        public string ExePath => ExePathTextBox.Text;
        public string Arguments => ArgumentsTextBox.Text;
        public string AppxId { get; private set; } = "";

        private bool _urlMode = false;

        private void BtnWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            _urlMode = false;
            AppNameTextBox.Text = "WhatsApp";
            ExePathTextBox.IsReadOnly = true;
            ExePathTextBox.Text = "shell:AppsFolder\\5319275A.WhatsAppDesktop_cv1g1gvanyjgm!App";
            AppxId = "5319275A.WhatsAppDesktop_cv1g1gvanyjgm!App";
            PathLabel.Text = "Store Uygulaması:";
            BtnBrowse.IsEnabled = false;
            BtnUrlMode.Content = "🌐 URL";
        }

        private void BtnUrlMode_Click(object sender, RoutedEventArgs e)
        {
            AppxId = "";
            _urlMode = !_urlMode;
            if (_urlMode)
            {
                ExePathTextBox.IsReadOnly = false;
                ExePathTextBox.Text = ExePathTextBox.Text.Length == 0 ? "https://" : ExePathTextBox.Text;
                ExePathTextBox.Focus();
                ExePathTextBox.CaretIndex = ExePathTextBox.Text.Length;
                PathLabel.Text = "Web adresi (URL):";
                BtnBrowse.IsEnabled = false;
                BtnUrlMode.Content = "📁 Exe";
                BtnUrlMode.ToolTip = "Uygulama (exe) ekle moduna geç";
            }
            else
            {
                ExePathTextBox.IsReadOnly = true;
                ExePathTextBox.Text = "";
                PathLabel.Text = "Uygulama yolu:";
                BtnBrowse.IsEnabled = true;
                BtnUrlMode.Content = "🌐 URL";
                BtnUrlMode.ToolTip = "Web adresi (URL) ekle";
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            AppxId = "";
            PathLabel.Text = "Uygulama yolu:";
            WpfOpenFileDialog dialog = new WpfOpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                ExePathTextBox.Text = dialog.FileName;
                if (string.IsNullOrWhiteSpace(AppNameTextBox.Text))
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    AppNameTextBox.Text = fileName;
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AppName) || string.IsNullOrWhiteSpace(ExePath))
            {
                System.Windows.MessageBox.Show("Uygulama adı ve yol zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_urlMode)
            {
                var url = ExePath.Trim();
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                {
                    System.Windows.MessageBox.Show("Geçerli bir URL girin (http:// veya https:// ile başlamalı).", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

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