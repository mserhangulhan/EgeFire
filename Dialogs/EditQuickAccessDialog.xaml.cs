using Microsoft.Win32;
using System.Windows;

namespace EasyTaskStarter.Dialogs
{
    public partial class EditQuickAccessDialog : Window
    {
        public EditQuickAccessDialog(string name, string path)
        {
            InitializeComponent();
            NameTextBox.Text = name;
            PathTextBox.Text = path;
        }

        public string ItemName => NameTextBox.Text;
        public string ItemPath => PathTextBox.Text;

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Klasör seçmek için buraya tıklayın"
            };

            if (dialog.ShowDialog() == true)
            {
                string? dir = System.IO.Path.GetDirectoryName(dialog.FileName);
                if (dir != null)
                {
                    PathTextBox.Text = dir;
                    var folderName = System.IO.Path.GetFileName(dir.TrimEnd('\\'));
                    if (string.IsNullOrEmpty(folderName)) folderName = dir;
                    NameTextBox.Text = folderName;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                System.Windows.MessageBox.Show("Klasör adı ve yolu zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
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