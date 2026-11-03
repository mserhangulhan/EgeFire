using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WpfApplication = System.Windows.Application;
using WpfMessageBox = System.Windows.MessageBox;
using WpfClipboard = System.Windows.Clipboard;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfListBox = System.Windows.Controls.ListBox;
using EasyTaskStarter.Helpers;
using EasyTaskStarter.Models;
using EasyTaskStarter.Services;

namespace EasyTaskStarter
{
    public partial class MainWindow : Window
    {
        private AppConfig _config;
        private TrayIcon? _trayIcon;
        private HotkeyListener? _hotkeyListener;
        private bool _isClosing = false;

                public MainWindow()
        {
            _config = ConfigManager.LoadConfig();
            InitializeComponent();
            InitializeTheme();
            // Loaded olayı XAML'da zaten bağlı
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            _trayIcon = new TrayIcon(handle, "EgeFire",
                () => { Show(); Activate(); },
                () => { _isClosing = true; WpfApplication.Current.Shutdown(); },
                () => LaunchAutoStartGroup());

            InitializeHotkey();
            ApplyConfig();

            // Pano geçmişini izlemeyi başlat
            Helpers.ClipboardMonitor.StartMonitoring();

            // Sürükle-bırak sıralama ve dışarıdan dosya bırakma
            Helpers.DragDropHelper.Attach(ShortcutListBox, () =>
            {
                ConfigManager.SaveConfig(_config);
                ShortcutListBox.ItemsSource = null;
                ShortcutListBox.ItemsSource = _config.Shortcuts;
            }, OnFilesDroppedToShortcuts);

            Helpers.DragDropHelper.Attach(QuickAccessListBox, () =>
            {
                ConfigManager.SaveConfig(_config);
                QuickAccessListBox.ItemsSource = null;
                QuickAccessListBox.ItemsSource = _config.QuickAccess;
            }, OnFilesDroppedToQuickAccess);

            // Auto-update check
            if (_config.CheckForUpdates)
            {
                _ = CheckForUpdatesAsync();
            }
        }

        private void InitializeTheme()
        {
            // System temasını uygula
            ApplyTheme(_config.Theme);
            UpdateThemeButton();
        }

        private void ApplyTheme(string theme)
        {
            var dict = Resources.MergedDictionaries;
            dict.Clear();

            if (theme == "Light" || (theme == "System" && IsSystemDarkTheme() == false))
            {
                dict.Add(new ResourceDictionary { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) });
            }
            else if (theme == "Dark" || (theme == "System" && IsSystemDarkTheme() == true))
            {
                dict.Add(new ResourceDictionary { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) });
            }

            // Theme button icon güncelle
            if (BtnTheme != null)
            {
                BtnTheme.Content = theme == "Dark" ? "☀️" : (theme == "Light" ? "🌙" : "🌓");
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

        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            var themes = new[] { "Light", "Dark", "System" };
            var currentIndex = Array.IndexOf(themes, _config.Theme);
            var nextIndex = (currentIndex + 1) % themes.Length;
            _config.Theme = themes[nextIndex];
            ApplyTheme(_config.Theme);
            UpdateThemeButton();
            ConfigManager.SaveConfig(_config);
        }

        private void UpdateThemeButton()
        {
            if (BtnTheme != null)
            {
                BtnTheme.Content = _config.Theme == "Dark" ? "☀️" : (_config.Theme == "Light" ? "🌙" : "🌓");
                BtnTheme.ToolTip = $"Tema: {_config.Theme} (Tıkla değiştir)";
            }
        }

        private void OnFilesDroppedToShortcuts(string[] files)
        {
            bool added = false;
            foreach (var path in files)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                string resolved = path;
                if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    resolved = Helpers.ShortcutResolver.ResolveShortcutTarget(path);
                }

                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrWhiteSpace(name))
                    name = System.IO.Path.GetFileName(resolved);

                var shortcut = new Models.AppShortcut(name, resolved, "");
                _config.Shortcuts.Add(shortcut);
                added = true;
            }

            if (added)
            {
                ConfigManager.SaveConfig(_config);
                ShortcutListBox.ItemsSource = null;
                ShortcutListBox.ItemsSource = _config.Shortcuts;
            }
        }

        private void OnFilesDroppedToQuickAccess(string[] files)
        {
            bool added = false;
            foreach (var path in files)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                string target = path;
                if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    target = Helpers.ShortcutResolver.ResolveShortcutTarget(path);
                }

                string folderPath = System.IO.Directory.Exists(target)
                    ? target
                    : (System.IO.Path.GetDirectoryName(target) ?? target);

                string folderName = System.IO.Path.GetFileName(folderPath.TrimEnd('\\', '/'));
                if (string.IsNullOrWhiteSpace(folderName))
                    folderName = folderPath;

                _config.QuickAccess.Add(new Models.QuickAccessItem(folderName, folderPath));
                added = true;
            }

            if (added)
            {
                ConfigManager.SaveConfig(_config);
                QuickAccessListBox.ItemsSource = null;
                QuickAccessListBox.ItemsSource = _config.QuickAccess;
            }
        }

        private void InitializeHotkey()
        {
            try
            {
                if (!string.IsNullOrEmpty(_config.Hotkey))
                {
                    _hotkeyListener = new HotkeyListener(this, _config.Hotkey);
                    _hotkeyListener.KeyPressed += (s, ev) =>
                    {
                        if (Visibility == Visibility.Visible)
                            Hide();
                        else
                        {
                            Show();
                            Activate();
                        }
                    };
                    _hotkeyListener.ShortcutHotkeyPressed += (s, index) =>
                    {
                        if (index >= 0 && index < _config.Shortcuts.Count)
                            OpenItem(_config.Shortcuts[index]);
                    };
                    _hotkeyListener.RegisterShortcutHotkeys(_config.Shortcuts);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hotkey error: {ex.Message}");
            }
        }

        private void ApplyConfig()
        {
            Width = Math.Max(_config.WindowSize.Width, 350);
            Height = Math.Max(_config.WindowSize.Height, 400);

            // Multi-monitor pozisyonunu yükle
            LoadMonitorPosition();

            if (_config.WindowPosition.X == 0 && _config.WindowPosition.Y == 0)
            {
                Left = SystemParameters.WorkArea.Width - Width - 20;
                Top = SystemParameters.WorkArea.Height - Height - 20;
            }
            else
            {
                Left = _config.WindowPosition.X;
                Top = _config.WindowPosition.Y;
            }

            ShortcutListBox.ItemsSource = null;
                        foreach (var s in _config.Shortcuts) s.LoadIcon();
                        ShortcutListBox.ItemsSource = _config.Shortcuts;

                        QuickAccessListBox.ItemsSource = null;
                        foreach (var q in _config.QuickAccess) q.LoadIcon();
                        QuickAccessListBox.ItemsSource = _config.QuickAccess;

            WindowHelper.SetAlwaysOnTop(this, _config.AlwaysOnTop);
            UpdatePinButton();
        }

        private void LoadMonitorPosition()
        {
            try
            {
                var currentScreen = GetCurrentScreenInfo();
                if (!string.IsNullOrEmpty(currentScreen))
                {
                    var saved = _config.MonitorPositions.FirstOrDefault(m => m.MonitorDeviceName == currentScreen);
                    if (saved != null)
                    {
                        Left = saved.X;
                        Top = saved.Y;
                        Width = Math.Max(saved.Width, 350);
                        Height = Math.Max(saved.Height, 400);
                        return;
                    }
                }
            }
            catch { }
        }

        private void SaveMonitorPosition()
        {
            try
            {
                var currentScreen = GetCurrentScreenInfo();
                if (!string.IsNullOrEmpty(currentScreen))
                {
                    var existing = _config.MonitorPositions.FirstOrDefault(m => m.MonitorDeviceName == currentScreen);
                    if (existing == null)
                    {
                        _config.MonitorPositions.Add(new Models.MonitorWindowPosition
                        {
                            MonitorDeviceName = currentScreen,
                            X = Left,
                            Y = Top,
                            Width = Width,
                            Height = Height
                        });
                    }
                    else
                    {
                        existing.X = Left;
                        existing.Y = Top;
                        existing.Width = Width;
                        existing.Height = Height;
                    }
                }
            }
            catch { }
        }

        private string GetCurrentScreenInfo()
        {
            try
            {
                var screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);
                return screen.DeviceName;
            }
            catch
            {
                return "";
            }
        }

        private void UpdatePinButton()
        {
            if (BtnPin != null)
            {
                BtnPin.Opacity = _config.AlwaysOnTop ? 1.0 : 0.4;
                BtnPin.ToolTip = _config.AlwaysOnTop ? "Her Zaman Üstte (Aktif)" : "Her Zaman Üstte (Pasif)";
            }
        }

        private void BtnPin_Click(object sender, RoutedEventArgs e)
        {
            _config.AlwaysOnTop = !_config.AlwaysOnTop;
            WindowHelper.SetAlwaysOnTop(this, _config.AlwaysOnTop);
            UpdatePinButton();
            ConfigManager.SaveConfig(_config);
        }

        // Custom Title Bar Buttons
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            // Tam ekran (maximize) devre dışı — sadece boyutlandırılabilir
            // İsterseniz buraya büyük bir boyuta ayarlama eklenebilir
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // X butonuna basıldığında pencereyi kapat değil, gizle (tray behavior)
            Hide();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is WpfListBox lb)
            {
                var clickedElement = e.OriginalSource as DependencyObject;
                var listBoxItem = FindVisualAncestor<ListBoxItem>(clickedElement);

                if (listBoxItem != null && listBoxItem.DataContext != null)
                {
                    if (listBoxItem.DataContext is Models.AppShortcut shortcut)
                        Services.ProcessLauncher.LaunchApp(shortcut.ExePath, shortcut.Arguments);
                    else if (listBoxItem.DataContext is Models.QuickAccessItem qa)
                        Services.ProcessLauncher.OpenFolder(qa.Path);

                    e.Handled = true;
                }
            }
        }

        private void ListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is WpfListBox lb)
            {
                var clickedElement = e.OriginalSource as DependencyObject;
                var listBoxItem = FindVisualAncestor<ListBoxItem>(clickedElement);

                if (listBoxItem != null && listBoxItem.DataContext != null)
                {
                    var item = listBoxItem.DataContext;
                    var menu = new System.Windows.Controls.ContextMenu();

                    var openItem = new System.Windows.Controls.MenuItem { Header = "Aç" };
                    openItem.Click += (s, args) => OpenItem(item);
                    menu.Items.Add(openItem);

                    if (item is Models.AppShortcut shortcutItem && !shortcutItem.IsStoreApp)
                    {
                        var adminItem = new System.Windows.Controls.MenuItem { Header = "Yönetici olarak aç" };
                        adminItem.Click += (s, args) =>
                        {
                            Services.ProcessLauncher.LaunchAppAsAdmin(shortcutItem.ExePath, shortcutItem.Arguments);
                        };
                        menu.Items.Add(adminItem);
                        menu.Items.Add(new System.Windows.Controls.Separator());
                    }

                    if (item is Models.AppShortcut)
                    {
                        var hotkeyItem = new System.Windows.Controls.MenuItem { Header = "Hotkey Ata..." };
                        hotkeyItem.Click += (s, args) =>
                        {
                            if (item is Models.AppShortcut sc)
                            {
                                var dlg = new Dialogs.HotkeyDialog(sc.Hotkey ?? "");
                                dlg.Owner = this;
                                if (dlg.ShowDialog() == true)
                                {
                                    sc.Hotkey = dlg.NewHotkey;
                                    ConfigManager.SaveConfig(_config);
                                    _hotkeyListener?.RegisterShortcutHotkeys(_config.Shortcuts);
                                }
                            }
                        };
                        menu.Items.Add(hotkeyItem);
                    }

                    var editItem = new System.Windows.Controls.MenuItem { Header = "Düzenle" };
                    editItem.Click += (s, args) => EditItem(item);
                    menu.Items.Add(editItem);

                    if (item is Models.AppShortcut fileSc && !fileSc.IsUrl && !fileSc.IsStoreApp && !string.IsNullOrEmpty(fileSc.ExePath))
                    {
                        var locItem = new System.Windows.Controls.MenuItem { Header = "📁 Dosya Konumunu Aç" };
                        locItem.Click += (s, args) => Services.ProcessLauncher.OpenFileLocation(fileSc.ExePath);
                        menu.Items.Add(locItem);
                    }
                    else if (item is Models.QuickAccessItem qaLoc && !string.IsNullOrEmpty(qaLoc.Path))
                    {
                        var locItem = new System.Windows.Controls.MenuItem { Header = "📁 Klasör Konumunu Aç" };
                        locItem.Click += (s, args) => Services.ProcessLauncher.OpenFolder(qaLoc.Path);
                        menu.Items.Add(locItem);
                    }

                    if (item is Models.AppShortcut autoSc)
                    {
                        var autoItem = new System.Windows.Controls.MenuItem
                        {
                            Header = autoSc.AutoStart ? "🚀 Açılış Grubundan Çıkar" : "🚀 Açılış Grubuna Dahil Et"
                        };
                        autoItem.Click += (s, args) =>
                        {
                            autoSc.AutoStart = !autoSc.AutoStart;
                            ConfigManager.SaveConfig(_config);
                            ShortcutListBox.ItemsSource = null;
                            ShortcutListBox.ItemsSource = _config.Shortcuts;
                        };
                        menu.Items.Add(autoItem);
                    }

                    menu.Items.Add(new System.Windows.Controls.Separator());

                    var deleteItem = new System.Windows.Controls.MenuItem { Header = "Sil" };
                    deleteItem.Click += (s, args) =>
                    {
                        if (item is Models.AppShortcut shortcut)
                        {
                            _config.Shortcuts.Remove(shortcut);
                            ConfigManager.SaveConfig(_config);
                            ShortcutListBox.ItemsSource = null;
                            ShortcutListBox.ItemsSource = _config.Shortcuts;
                        }
                        else if (item is Models.QuickAccessItem qaItem)
                        {
                            _config.QuickAccess.Remove(qaItem);
                            ConfigManager.SaveConfig(_config);
                            QuickAccessListBox.ItemsSource = null;
                            QuickAccessListBox.ItemsSource = _config.QuickAccess;
                        }
                    };
                    menu.Items.Add(deleteItem);

                    menu.IsOpen = true;
                    e.Handled = true;
                }
            }
        }

        private void OpenItem(object item)
        {
            if (item is Models.AppShortcut shortcut)
            {
                if (shortcut.IsStoreApp)
                    Services.ProcessLauncher.LaunchStoreApp(shortcut.AppxId);
                else
                    Services.ProcessLauncher.LaunchApp(shortcut.ExePath, shortcut.Arguments);
            }
            else if (item is Models.QuickAccessItem qa)
                Services.ProcessLauncher.OpenFolder(qa.Path);
        }

        private void EditItem(object item)
        {
            if (item is Models.AppShortcut shortcut)
            {
                var dialog = new Dialogs.AddAppDialog(shortcut.Name, shortcut.ExePath, shortcut.Arguments, shortcut.AppxId);
                if (dialog.ShowDialog() == true)
                {
                    shortcut.Name = dialog.AppName;
                    shortcut.ExePath = dialog.ExePath;
                    shortcut.Arguments = dialog.Arguments;
                    shortcut.AppxId = dialog.AppxId;
                    ConfigManager.SaveConfig(_config);
                    ShortcutListBox.ItemsSource = null;
                    ShortcutListBox.ItemsSource = _config.Shortcuts;
                }
            }
            else if (item is Models.QuickAccessItem qa)
            {
                var dialog = new Dialogs.EditQuickAccessDialog(qa.Name, qa.Path);
                if (dialog.ShowDialog() == true)
                {
                    qa.Name = dialog.ItemName;
                    qa.Path = dialog.ItemPath;
                    ConfigManager.SaveConfig(_config);
                    QuickAccessListBox.ItemsSource = null;
                    QuickAccessListBox.ItemsSource = _config.QuickAccess;
                }
            }
        }

        private void BtnAddShortcut_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.AddAppDialog();
            if (dialog.ShowDialog() == true)
            {
                var shortcut = new Models.AppShortcut(dialog.AppName, dialog.ExePath, dialog.Arguments)
                {
                    AppxId = dialog.AppxId
                };
                _config.Shortcuts.Add(shortcut);
                ConfigManager.SaveConfig(_config);
                ShortcutListBox.ItemsSource = null;
                ShortcutListBox.ItemsSource = _config.Shortcuts;
            }
        }

        private void BtnAddQuickAccess_Click(object sender, RoutedEventArgs e)
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
                string? selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    string folderName = System.IO.Path.GetFileName(selectedPath.TrimEnd('\\'));
                    if (string.IsNullOrEmpty(folderName))
                        folderName = selectedPath;

                    _config.QuickAccess.Add(new Models.QuickAccessItem(folderName, selectedPath));
                    ConfigManager.SaveConfig(_config);
                    QuickAccessListBox.ItemsSource = null;
                    QuickAccessListBox.ItemsSource = _config.QuickAccess;
                }
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var text = SearchBox.Text;

            SearchPlaceholder.Visibility = string.IsNullOrEmpty(text)
                ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(text))
            {
                ShortcutListBox.ItemsSource = _config.Shortcuts;
            }
            else
            {
                var filtered = _config.Shortcuts.FindAll(s =>
                    s.Name.Contains(text, System.StringComparison.OrdinalIgnoreCase));
                ShortcutListBox.ItemsSource = filtered;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                QuickAccessListBox.ItemsSource = _config.QuickAccess;
            }
            else
            {
                var filtered = _config.QuickAccess.FindAll(q =>
                    q.Name.Contains(text, System.StringComparison.OrdinalIgnoreCase));
                QuickAccessListBox.ItemsSource = filtered;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                if (ShortcutListBox.Items.Count > 0)
                {
                    ShortcutListBox.SelectedIndex = 0;
                    QuickAccessListBox.SelectedIndex = -1;
                }
                else if (QuickAccessListBox.Items.Count > 0)
                {
                    QuickAccessListBox.SelectedIndex = 0;
                    ShortcutListBox.SelectedIndex = -1;
                }
            }
        }

        private void SearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Web search shortcuts: g: Google, yt: YouTube, gh: GitHub, so: StackOverflow, wk: Wikipedia
            if (e.Key == Key.Enter)
            {
                var text = SearchBox.Text.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    string url = null;
                    string searchQuery = text;

                    if (text.StartsWith("g ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
                    {
                        searchQuery = text.Substring(2).Trim();
                        url = $"https://www.google.com/search?q={Uri.EscapeDataString(searchQuery)}";
                    }
                    else if (text.StartsWith("yt ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("yt:", StringComparison.OrdinalIgnoreCase))
                    {
                        searchQuery = text.Substring(3).Trim();
                        url = $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(searchQuery)}";
                    }
                    else if (text.StartsWith("gh ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("gh:", StringComparison.OrdinalIgnoreCase))
                    {
                        searchQuery = text.Substring(3).Trim();
                        url = $"https://github.com/search?q={Uri.EscapeDataString(searchQuery)}";
                    }
                    else if (text.StartsWith("so ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("so:", StringComparison.OrdinalIgnoreCase))
                    {
                        searchQuery = text.Substring(3).Trim();
                        url = $"https://stackoverflow.com/search?q={Uri.EscapeDataString(searchQuery)}";
                    }
                    else if (text.StartsWith("wk ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("wk:", StringComparison.OrdinalIgnoreCase))
                    {
                        searchQuery = text.Substring(3).Trim();
                        url = $"https://en.wikipedia.org/w/index.php?search={Uri.EscapeDataString(searchQuery)}";
                    }

                    if (url != null)
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        e.Handled = true;
                        return;
                    }
                }

                // Normal item opening
                object? itemToOpen = ShortcutListBox.SelectedItem ?? QuickAccessListBox.SelectedItem;
                if (itemToOpen == null)
                {
                    if (ShortcutListBox.Items.Count > 0)
                        itemToOpen = ShortcutListBox.Items[0];
                    else if (QuickAccessListBox.Items.Count > 0)
                        itemToOpen = QuickAccessListBox.Items[0];
                }

                if (itemToOpen != null)
                {
                    OpenItem(itemToOpen);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (ShortcutListBox.Items.Count > 0)
                {
                    int nextIdx = ShortcutListBox.SelectedIndex + 1;
                    if (nextIdx < ShortcutListBox.Items.Count)
                    {
                        ShortcutListBox.SelectedIndex = nextIdx;
                        ShortcutListBox.ScrollIntoView(ShortcutListBox.SelectedItem);
                    }
                    QuickAccessListBox.SelectedIndex = -1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (ShortcutListBox.Items.Count > 0)
                {
                    int prevIdx = ShortcutListBox.SelectedIndex - 1;
                    if (prevIdx >= 0)
                    {
                        ShortcutListBox.SelectedIndex = prevIdx;
                        ShortcutListBox.ScrollIntoView(ShortcutListBox.SelectedItem);
                    }
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (!string.IsNullOrEmpty(SearchBox.Text))
                {
                    SearchBox.Text = "";
                    e.Handled = true;
                }
            }
        }

        private void BtnHotkeySettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.HotkeyDialog(_config.Hotkey);
            if (dialog.ShowDialog() == true)
            {
                _config.Hotkey = dialog.NewHotkey;
                ConfigManager.SaveConfig(_config);
                _hotkeyListener?.Dispose();
                InitializeHotkey();
            }
        }

        private void BtnQuickTools_Click(object sender, RoutedEventArgs e)
        {
            var menu = new System.Windows.Controls.ContextMenu();

            void AddTool(string title, Action action)
            {
                var item = new System.Windows.Controls.MenuItem { Header = title };
                item.Click += (s, ev) => action();
                menu.Items.Add(item);
            }

            AddTool("⚙️  Görev Yöneticisi", () => Services.ProcessLauncher.LaunchApp("taskmgr.exe"));
            AddTool("🧮  Hesap Makinesi", () => Services.ProcessLauncher.LaunchApp("calc.exe"));
            AddTool("📝  Not Defteri", () => Services.ProcessLauncher.LaunchApp("notepad.exe"));
            AddTool("✂️  Ekran Alıntısı Aracı", () => Services.ProcessLauncher.LaunchApp("snippingtool.exe"));
            AddTool("💻  Komut İstemi (CMD)", () => Services.ProcessLauncher.LaunchApp("cmd.exe"));
            AddTool("⚡  PowerShell", () => Services.ProcessLauncher.LaunchApp("powershell.exe"));
            menu.Items.Add(new System.Windows.Controls.Separator());
            AddTool("🔒  Bilgisayarı Kilitle", () => Services.ProcessLauncher.LaunchApp("rundll32.exe", "user32.dll,LockWorkStation"));
            AddTool("🛑  Bilgisayarı Kapat", () => Services.ProcessLauncher.LaunchApp("shutdown.exe", "/s /t 0"));

            // Zamanlayıcı Alt Menüsü
            var timerMenu = new System.Windows.Controls.MenuItem { Header = "⏱️  Kapatma / Uyku Sayacı" };
            timerMenu.Items.Add(CreateTimerItem("15 dakika sonra Kapat", "shutdown.exe", "/s /t 900"));
            timerMenu.Items.Add(CreateTimerItem("30 dakika sonra Kapat", "shutdown.exe", "/s /t 1800"));
            timerMenu.Items.Add(CreateTimerItem("60 dakika sonra Kapat", "shutdown.exe", "/s /t 3600"));
            timerMenu.Items.Add(CreateTimerItem("120 dakika sonra Kapat", "shutdown.exe", "/s /t 7200"));
            timerMenu.Items.Add(new System.Windows.Controls.Separator());
            timerMenu.Items.Add(CreateTimerItem("15 dakika sonra Uyku", "rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0"));
            timerMenu.Items.Add(CreateTimerItem("30 dakika sonra Uyku", "rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0"));
            timerMenu.Items.Add(new System.Windows.Controls.Separator());
            var cancelTimerItem = new System.Windows.Controls.MenuItem { Header = "❌ Kapatma Sayacını İptal Et" };
            cancelTimerItem.Click += (s, ev) =>
            {
                Services.ProcessLauncher.LaunchApp("shutdown.exe", "/a");
                WpfMessageBox.Show("Zamanlanmış kapatma işlemi iptal edildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            timerMenu.Items.Add(cancelTimerItem);

            menu.Items.Add(timerMenu);

            menu.PlacementTarget = BtnQuickTools;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            menu.IsOpen = true;
        }

        private System.Windows.Controls.MenuItem CreateTimerItem(string header, string cmd, string args)
        {
            var item = new System.Windows.Controls.MenuItem { Header = header };
            item.Click += (s, ev) =>
            {
                Services.ProcessLauncher.LaunchApp(cmd, args);
                WpfMessageBox.Show($"{header} zamanlandı. İstediğin zaman sayaç menüsünden iptal edebilirsin.", "Sayaç Başlatıldı", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            return item;
        }

        private void BtnConfigBackup_Click(object sender, RoutedEventArgs e)
        {
            var menu = new System.Windows.Controls.ContextMenu();

            var exportItem = new System.Windows.Controls.MenuItem { Header = "📤 Ayarları Dışa Aktar (Yedekle)" };
            exportItem.Click += (s, ev) =>
            {
                var saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Ayarları Yedekle",
                    Filter = "JSON Dosyası (*.json)|*.json",
                    FileName = $"EgeFire_Backup_{DateTime.Now:yyyyMMdd_HHmm}.json"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    try
                    {
                        ConfigManager.ExportConfig(saveDlg.FileName, _config);
                        WpfMessageBox.Show("Ayarlar başarıyla dışa aktarıldı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        WpfMessageBox.Show($"Dışa aktarma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };
            menu.Items.Add(exportItem);

            var importItem = new System.Windows.Controls.MenuItem { Header = "📥 Ayarları İçe Aktar (Geri Yükle)" };
            importItem.Click += (s, ev) =>
            {
                var openDlg = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Yedek Dosyası Seç",
                    Filter = "JSON Dosyası (*.json)|*.json"
                };

                if (openDlg.ShowDialog() == true)
                {
                    try
                    {
                        var importedConfig = ConfigManager.ImportConfig(openDlg.FileName);
                        _config = importedConfig;
                        ConfigManager.SaveConfig(_config);
                        ApplyConfig();
                        _hotkeyListener?.Dispose();
                        InitializeHotkey();
                        WpfMessageBox.Show("Ayarlar başarıyla içe aktarıldı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        WpfMessageBox.Show($"İçe aktarma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };
            menu.Items.Add(importItem);

            menu.PlacementTarget = BtnConfigBackup;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            menu.IsOpen = true;
        }

        private void BtnLaunchAll_Click(object sender, RoutedEventArgs e)
        {
            var autoList = _config.Shortcuts.FindAll(s => s.AutoStart);
            if (autoList.Count == 0)
            {
                WpfMessageBox.Show("Açılış grubunda kayıtlı uygulama bulunamadı.\nBir uygulamanın üzerine sağ tıklayıp '🚀 Açılış Grubuna Dahil Et' diyerek ekleyebilirsiniz.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int count = 0;
            foreach (var shortcut in autoList)
            {
                OpenItem(shortcut);
                count++;
            }

            WpfMessageBox.Show($"{count} adet açılış grubu uygulaması başlatıldı!", "Başlatıldı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LaunchAutoStartGroup()
        {
            var autoList = _config.Shortcuts.FindAll(s => s.AutoStart);
            if (autoList.Count == 0) return;

            foreach (var shortcut in autoList)
            {
                OpenItem(shortcut);
            }
        }

        private void BtnClipboard_Click(object sender, RoutedEventArgs e)
        {
            var menu = new System.Windows.Controls.ContextMenu();
            var history = Helpers.ClipboardMonitor.History;

            if (history.Count == 0)
            {
                var emptyItem = new System.Windows.Controls.MenuItem { Header = "(Henüz panoda metin kopyalanmadı)", IsEnabled = false };
                menu.Items.Add(emptyItem);
            }
            else
            {
                var headerItem = new System.Windows.Controls.MenuItem { Header = "📋 Son Kopyalananlar (Tıkla ve Kopyala):", IsEnabled = false, FontWeight = FontWeights.Bold };
                menu.Items.Add(headerItem);
                menu.Items.Add(new System.Windows.Controls.Separator());

                foreach (var text in history)
                {
                    string preview = text.Trim().Replace("\r", " ").Replace("\n", " ");
                    if (preview.Length > 45) preview = preview.Substring(0, 42) + "...";

                    var item = new System.Windows.Controls.MenuItem { Header = preview, ToolTip = text };
                    string capturedText = text;
                    item.Click += (s, ev) =>
                    {
                        try
                        {
                            WpfClipboard.SetText(capturedText);
                            WpfMessageBox.Show("Metin panoya kopyalandı!", "Pano", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch
                        {
                        }
                    };
                    menu.Items.Add(item);
                }

                menu.Items.Add(new System.Windows.Controls.Separator());
                var clearItem = new System.Windows.Controls.MenuItem { Header = "🗑️ Geçmişi Temizle" };
                clearItem.Click += (s, ev) => Helpers.ClipboardMonitor.ClearHistory();
                menu.Items.Add(clearItem);
            }

            menu.PlacementTarget = BtnClipboard;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            menu.IsOpen = true;
        }

        private void BtnNotes_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Dialogs.NotesDialog(_config.Notes);
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                _config.Notes = dlg.NotesText;
                ConfigManager.SaveConfig(_config);
            }
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new Dialogs.AboutDialog();
                dlg.Owner = this;
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Hakkında penceresi açılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version?.ToString(3) ?? "3.0.0";

                var (hasUpdate, latestVersion, releaseNotes, downloadUrl) = await Services.UpdateChecker.CheckForUpdatesAsync(version);

                if (hasUpdate)
                {
                    var result = WpfMessageBox.Show(
                        $"Yeni sürüm mevcut: v{latestVersion}\n\n{releaseNotes}\n\nŞimdi indirmek ister misiniz?",
                        "Güncelleme Bulundu",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(downloadUrl))
                    {
                        Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
                    }
                }

                _config.LastUpdateCheck = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                _config.LastKnownVersion = latestVersion;
                ConfigManager.SaveConfig(_config);
            }
            catch
            {
                // Sessizce hata yut - güncelleme kontrolü kullanıcıyı rahatsız etmemeli
            }
        }

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                _trayIcon?.Dispose();
                _hotkeyListener?.Dispose();
                SaveWindowPosition();
                SaveMonitorPosition();
            }
        }

        private void SaveWindowPosition()
        {
            _config.WindowPosition = new Models.WindowPosition { X = Left, Y = Top };
            _config.WindowSize = new Models.WindowSize { Width = Width, Height = Height };
            ConfigManager.SaveConfig(_config);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    BtnMaximize_Click(null, null);
                }
                else
                {
                    DragMove();
                }
            }
        }

        private static T? FindVisualAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                    return ancestor;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}