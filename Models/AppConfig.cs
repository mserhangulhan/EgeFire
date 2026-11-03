using System.Collections.Generic;

namespace EasyTaskStarter.Models
{
    public class AppConfig
    {
        public List<AppShortcut> Shortcuts { get; set; } = new List<AppShortcut>();
        public List<QuickAccessItem> QuickAccess { get; set; } = new List<QuickAccessItem>();
        public string Hotkey { get; set; } = "Ctrl+Alt+D";
        public double Opacity { get; set; } = 1.0;
        public bool AlwaysOnTop { get; set; } = true;
        public bool Autorun { get; set; } = false;
        public string Theme { get; set; } = "System"; // Light, Dark, System
        public string Notes { get; set; } = "";
        public bool CheckForUpdates { get; set; } = true;
        public string LastUpdateCheck { get; set; } = "";
        public string LastKnownVersion { get; set; } = "";
        public WindowPosition WindowPosition { get; set; } = new WindowPosition();
        public WindowSize WindowSize { get; set; } = new WindowSize { Width = 350, Height = 400 };
        public List<MonitorWindowPosition> MonitorPositions { get; set; } = new List<MonitorWindowPosition>();
    }

    public class WindowPosition
    {
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
    }

    public class WindowSize
    {
        public double Width { get; set; } = 350;
        public double Height { get; set; } = 400;
    }

    public class MonitorWindowPosition
    {
        public string MonitorDeviceName { get; set; } = "";
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public double Width { get; set; } = 350;
        public double Height { get; set; } = 400;
    }
}