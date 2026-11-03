using System;
using System.Runtime.InteropServices;
using System.Windows;
using WpfInput = System.Windows.Input;
using System.Windows.Interop;

namespace EasyTaskStarter.Services
{
    public class HotkeyListener : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const int SHORTCUT_HOTKEY_BASE = 9100; // Per-shortcut ID'ler: 9100, 9101...
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private IntPtr _windowHandle;
        private HwndSource _source;
        private bool _isRegistered;
        private readonly System.Collections.Generic.Dictionary<int, int> _shortcutIds = new(); // id → shortcut index

        public event EventHandler<WpfInput.KeyEventArgs>? KeyPressed;
        public event EventHandler<int>? ShortcutHotkeyPressed; // shortcut index

        public HotkeyListener(Window window, string hotkey)
        {
            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.Handle;

            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotkey(hotkey);
        }

        private void RegisterHotkey(string hotkey)
        {
            if (string.IsNullOrEmpty(hotkey)) return;

            if (ParseHotkey(hotkey, out uint modifiers, out uint vk) && vk != 0)
            {
                _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, vk);
            }
        }

        public void RegisterShortcutHotkeys(System.Collections.Generic.List<Models.AppShortcut> shortcuts)
        {
            // Önce mevcut per-kısayol hotkey'leri temizle
            foreach (var id in _shortcutIds.Keys)
                UnregisterHotKey(_windowHandle, id);
            _shortcutIds.Clear();

            for (int i = 0; i < shortcuts.Count; i++)
            {
                var hk = shortcuts[i].Hotkey;
                if (string.IsNullOrWhiteSpace(hk)) continue;
                if (ParseHotkey(hk, out uint mod, out uint vk) && vk != 0)
                {
                    int id = SHORTCUT_HOTKEY_BASE + i;
                    if (RegisterHotKey(_windowHandle, id, mod, vk))
                        _shortcutIds[id] = i;
                }
            }
        }

        private static bool ParseHotkey(string hotkey, out uint modifiers, out uint vk)
        {
            modifiers = 0;
            vk = 0;
            var parts = hotkey.Split('+');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Equals("Control", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_CONTROL;
                else if (trimmed.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_ALT;
                else if (trimmed.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_SHIFT;
                else if (trimmed.Equals("Win", StringComparison.OrdinalIgnoreCase))
                    modifiers |= MOD_WIN;
                else if (Enum.TryParse<WpfInput.Key>(trimmed, out WpfInput.Key key))
                    vk = (uint)WpfInput.KeyInterop.VirtualKeyFromKey(key);
            }
            return vk != 0;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == HOTKEY_ID)
                {
                    KeyPressed?.Invoke(this, new WpfInput.KeyEventArgs(WpfInput.Keyboard.PrimaryDevice, PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow), 0, WpfInput.Key.None));
                    handled = true;
                }
                else if (_shortcutIds.TryGetValue(id, out int index))
                {
                    ShortcutHotkeyPressed?.Invoke(this, index);
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }

            foreach (var id in _shortcutIds.Keys)
                UnregisterHotKey(_windowHandle, id);
            _shortcutIds.Clear();

            _source?.RemoveHook(HwndHook);
            // Not: HwndSource, ana pencerenin kaynağıdır — burada Dispose etmiyoruz
        }
    }
}
