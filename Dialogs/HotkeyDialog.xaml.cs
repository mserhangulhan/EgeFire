using System.Windows;
using WpfInput = System.Windows.Input;

namespace EasyTaskStarter.Dialogs
{
    public partial class HotkeyDialog : Window
    {
        public HotkeyDialog()
        {
            InitializeComponent();
        }

        public HotkeyDialog(string currentHotkey)
        {
            InitializeComponent();
            HotkeyTextBox.Text = currentHotkey;
        }

        public string NewHotkey => HotkeyTextBox.Text;

        private void HotkeyDialog_KeyDown(object sender, WpfInput.KeyEventArgs e)
        {
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, WpfInput.KeyEventArgs e)
        {
if (e.Key == WpfInput.Key.System) return;

            WpfInput.ModifierKeys modifiers = WpfInput.Keyboard.Modifiers;
            string combo = "";

            if (modifiers.HasFlag(WpfInput.ModifierKeys.Control))
                combo += "Ctrl+";
            if (modifiers.HasFlag(WpfInput.ModifierKeys.Alt))
                combo += "Alt+";
            if (modifiers.HasFlag(WpfInput.ModifierKeys.Shift))
                combo += "Shift+";

            if (e.Key != WpfInput.Key.None && e.Key != WpfInput.Key.LeftCtrl && e.Key != WpfInput.Key.RightCtrl &&
                e.Key != WpfInput.Key.LeftAlt && e.Key != WpfInput.Key.RightAlt &&
                e.Key != WpfInput.Key.LeftShift && e.Key != WpfInput.Key.RightShift)
            {
                combo += e.Key.ToString();
            }

            if (!string.IsNullOrEmpty(combo))
            {
                HotkeyTextBox.Text = combo.TrimEnd('+');
            }

            e.Handled = true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
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