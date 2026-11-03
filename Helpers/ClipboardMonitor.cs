using System;
using System.Collections.Generic;
using System.Windows;
using WpfClipboard = System.Windows.Clipboard;
using System.Windows.Threading;

namespace EasyTaskStarter.Helpers
{
    public class ClipboardMonitor
    {
        private static readonly List<string> _history = new();
        private static string _lastText = "";
        private static DispatcherTimer? _timer;

        public static IReadOnlyList<string> History => _history.AsReadOnly();

        public static void StartMonitoring()
        {
            if (_timer != null) return;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)
            };
            _timer.Tick += (s, e) => CheckClipboard();
            _timer.Start();
        }

        private static void CheckClipboard()
        {
            try
            {
                if (WpfClipboard.ContainsText())
                {
                    string text = WpfClipboard.GetText();
                    if (!string.IsNullOrWhiteSpace(text) && text != _lastText)
                    {
                        _lastText = text;
                        _history.Remove(text);
                        _history.Insert(0, text);
                        if (_history.Count > 15)
                        {
                            _history.RemoveAt(_history.Count - 1);
                        }
                    }
                }
            }
            catch
            {
                // Pano başka bir uygulama tarafından kilitliyse atla
            }
        }

        public static void ClearHistory()
        {
            _history.Clear();
            _lastText = "";
        }
    }
}
