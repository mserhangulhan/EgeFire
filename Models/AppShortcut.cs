using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace EasyTaskStarter.Models
{
    public class AppShortcut : INotifyPropertyChanged
    {
        private string _name = "";
        private string _exePath = "";
        private string _arguments = "";
        private string _hotkey = "";
        private string _appxId = "";
        private bool _autoStart = false;
        private BitmapSource? _icon;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string ExePath
        {
            get => _exePath;
            set { _exePath = value; OnPropertyChanged(); LoadIcon(); }
        }

        public string Arguments
        {
            get => _arguments;
            set { _arguments = value; OnPropertyChanged(); }
        }

        public string Hotkey
        {
            get => _hotkey;
            set { _hotkey = value; OnPropertyChanged(); }
        }

        public string AppxId
        {
            get => _appxId;
            set { _appxId = value; OnPropertyChanged(); }
        }

        public bool AutoStart
        {
            get => _autoStart;
            set { _autoStart = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsStoreApp => !string.IsNullOrEmpty(_appxId);

        [JsonIgnore]
        public bool IsUrl =>
            _exePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            _exePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            _exePath.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase);

        [JsonIgnore]
        public BitmapSource? Icon
        {
            get => _icon;
            private set { _icon = value; OnPropertyChanged(); }
        }

        public AppShortcut() { }

        public AppShortcut(string name, string exePath, string arguments = "")
        {
            _name = name;
            _exePath = exePath;
            _arguments = arguments;
            LoadIcon();
        }

        public void LoadIcon()
        {
            if (IsStoreApp)
            {
                Icon = null;
                return;
            }

            // URL ise exe ikonu çekme, null bırak (XAML'da emoji gösterilir)
            if (_exePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                _exePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                _exePath.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
            {
                Icon = null;
                return;
            }
            Icon = Helpers.IconHelper.GetIcon(_exePath);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
