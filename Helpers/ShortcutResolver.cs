using System;
using System.IO;

namespace EasyTaskStarter.Helpers
{
    public static class ShortcutResolver
    {
        /// <summary>
        /// .lnk dosyasının hedef yolunu COM (WScript.Shell) kullanarak çözer.
        /// </summary>
        public static string ResolveShortcutTarget(string lnkPath)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return lnkPath;

                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null) return lnkPath;

                dynamic shortcut = shell.CreateShortcut(lnkPath);
                string targetPath = shortcut.TargetPath;
                if (!string.IsNullOrEmpty(targetPath) && (File.Exists(targetPath) || Directory.Exists(targetPath)))
                {
                    return targetPath;
                }
            }
            catch
            {
                // COM hatası olursa orijinal dosya yolunu dön
            }
            return lnkPath;
        }
    }
}
