using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;

namespace EasyTaskStarter.Helpers
{
    /// <summary>
    /// Win32 Shell_NotifyIcon tabanlı system tray ikonu.
    /// WPF NotifyIcon paketi gerekmez; saf P/Invoke ile çalışır.
    /// </summary>
    public class TrayIcon : IDisposable
    {
        // ── Win32 sabitler ──────────────────────────────────────────────
        private const int WM_USER        = 0x0400;
        private const int WM_TRAYICON    = WM_USER + 1;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONUP   = 0x0205;

        private const uint NIM_ADD    = 0;
        private const uint NIM_DELETE = 2;
        private const uint NIF_MESSAGE = 0x01;
        private const uint NIF_ICON    = 0x02;
        private const uint NIF_TIP     = 0x04;

        private const int MF_STRING    = 0x00;
        private const int MF_SEPARATOR = 0x800;
        private const int MF_CHECKED   = 0x08;
        private const int TPM_RETURNCMD = 0x0100;
        private const int TPM_RIGHTBUTTON = 0x0002;

        private const int IDM_OPEN    = 1001;
        private const int IDM_STARTUP = 1002;
        private const int IDM_EXIT    = 1003;
        private const int IDM_LAUNCH_GROUP = 1004;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public uint  cbSize;
            public IntPtr hWnd;
            public uint  uID;
            public uint  uFlags;
            public uint  uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

        [DllImport("user32.dll")]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string? lpNewItem);

        [DllImport("user32.dll")]
        private static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        // ── Alanlar ─────────────────────────────────────────────────────
        private readonly IntPtr  _ownerHwnd;
        private readonly string  _tooltip;
        private readonly Action  _onOpen;
        private readonly Action  _onExit;
        private NOTIFYICONDATA   _nid;
        private Icon?            _icon;
        private System.Windows.Interop.HwndSource? _msgSource;
        private bool _disposed;

        // ── Kurucu ──────────────────────────────────────────────────────
        public TrayIcon(IntPtr ownerHwnd, string tooltip, Action onOpen, Action onExit, Action onLaunchGroup = null)
        {
            _ownerHwnd = ownerHwnd;
            _tooltip   = tooltip;
            _onOpen    = onOpen;
            _onExit    = onExit;
            _onLaunchGroup = onLaunchGroup;

            CreateMessageWindow();
            LoadIcon();
            AddTrayIcon();
        }

        private readonly Action _onLaunchGroup;

        // ── Mesaj penceresi (WndProc hook) ───────────────────────────────
        private void CreateMessageWindow()
        {
            var param = new System.Windows.Interop.HwndSourceParameters("TrayMsgWnd")
            {
                Width  = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0,
                ParentWindow = _ownerHwnd,
                WindowStyle = 0
            };
            _msgSource = new System.Windows.Interop.HwndSource(param);
            _msgSource.AddHook(WndProc);
        }

        private void LoadIcon()
        {
            try
            {
                // Gömülü resource'tan yükle
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream("EasyTaskStarter.EgeFireIcon.ico");
                if (stream != null)
                {
                    _icon = new Icon(stream);
                    return;
                }
            }
            catch { }

            // Fallback: exe'nin kendi ikonu
            try
            {
                string? exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (exe != null)
                    _icon = Icon.ExtractAssociatedIcon(exe);
            }
            catch { }

            _icon ??= SystemIcons.Application;
        }

        private void AddTrayIcon()
        {
            _nid = new NOTIFYICONDATA
            {
                cbSize          = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd            = _msgSource!.Handle,
                uID             = 1,
                uFlags          = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                uCallbackMessage= WM_TRAYICON,
                hIcon           = _icon?.Handle ?? IntPtr.Zero,
                szTip           = _tooltip
            };
            Shell_NotifyIcon(NIM_ADD, ref _nid);
        }

        // ── WndProc ─────────────────────────────────────────────────────
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_TRAYICON)
            {
                int notifyMsg = lParam.ToInt32() & 0xFFFF;

                if (notifyMsg == WM_LBUTTONDBLCLK)
                {
                    handled = true;
                    _onOpen?.Invoke();
                }
                else if (notifyMsg == WM_RBUTTONUP)
                {
                    handled = true;
                    ShowContextMenu();
                }
            }
            return IntPtr.Zero;
        }

        private void ShowContextMenu()
        {
            bool startupEnabled = Services.StartupManager.IsStartupEnabled();

            IntPtr hMenu = CreatePopupMenu();
            AppendMenu(hMenu, MF_STRING,    IDM_OPEN,    "Aç");
            AppendMenu(hMenu, MF_STRING,    IDM_LAUNCH_GROUP, "Açılış Grubundaki Uygulamaları Başlat");
            AppendMenu(hMenu, MF_SEPARATOR, 0,           null);
            AppendMenu(hMenu, MF_STRING | (startupEnabled ? MF_CHECKED : 0), IDM_STARTUP, "Başlangıçta Çalış");
            AppendMenu(hMenu, MF_SEPARATOR, 0,           null);
            AppendMenu(hMenu, MF_STRING,    IDM_EXIT,    "Kapat");

            GetCursorPos(out POINT pt);
            SetForegroundWindow(_msgSource!.Handle);

            int cmd = TrackPopupMenu(hMenu, TPM_RETURNCMD | TPM_RIGHTBUTTON, pt.X, pt.Y, 0, _msgSource.Handle, IntPtr.Zero);
            DestroyMenu(hMenu);

            switch (cmd)
            {
                case IDM_OPEN:
                    _onOpen?.Invoke();
                    break;
                case IDM_LAUNCH_GROUP:
                    _onLaunchGroup?.Invoke();
                    break;
                case IDM_STARTUP:
                    Services.StartupManager.SetStartup(!startupEnabled);
                    break;
                case IDM_EXIT:
                    _onExit?.Invoke();
                    break;
            }
        }

        // ── IDisposable ──────────────────────────────────────────────────
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Shell_NotifyIcon(NIM_DELETE, ref _nid);
            _msgSource?.Dispose();
            _icon?.Dispose();
        }
    }
}
