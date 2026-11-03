using System.Windows;

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