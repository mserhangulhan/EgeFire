using System;
using System.Threading;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace EasyTaskStarter;

public partial class App : System.Windows.Application
{
    private static Mutex? _mutex;
    private const string AppMutexName = "Global\\EgeFire_Mutex";

    // --autorun parametresiyle başlatılınca true olur
    public static bool StartHidden { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        this.DispatcherUnhandledException += (s, args) =>
        {
            try
            {
                string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EgeFire");
                System.IO.Directory.CreateDirectory(dir);
                string logFile = System.IO.Path.Combine(dir, "error.log");
                System.IO.File.AppendAllText(logFile, $"[{DateTime.Now}] {args.Exception}\n\n");
                WpfMessageBox.Show($"Beklenmeyen Hata: {args.Exception.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            }
            catch { }
        };

        bool createdNew;
        _mutex = new Mutex(true, AppMutexName, out createdNew);

        if (!createdNew)
        {
            Environment.Exit(0);
            return;
        }

        // --autorun varsa pencere gizli başlar
        foreach (string arg in e.Args)
        {
            if (arg.Equals("--autorun", StringComparison.OrdinalIgnoreCase))
            {
                StartHidden = true;
                break;
            }
        }

        base.OnStartup(e);

        var mainWindow = new MainWindow();
        mainWindow.Show();          // Loaded event'ini tetiklemek için önce Show
        if (StartHidden)
            mainWindow.Hide();      // Ardından hemen gizle
        else
            mainWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}