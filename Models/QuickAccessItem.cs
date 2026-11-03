using System.Windows.Media.Imaging;

namespace EasyTaskStarter.Models
{
    public class QuickAccessItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public BitmapSource? Icon { get; private set; }

        public QuickAccessItem(string name, string path)
        {
            Name = name;
            Path = path;
            LoadIcon();
        }

        public void LoadIcon()
        {
            try
            {
                // "Bu bilgisayar" için sistem ikonu
                if (Path == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}")
                {
                    Icon = Helpers.IconHelper.GetIcon("shell32.dll");
                    return;
                }

                if (System.IO.Directory.Exists(Path) || System.IO.File.Exists(Path))
                {
                    Icon = Helpers.IconHelper.GetIcon(Path);
                }
            }
            catch
            {
                Icon = null;
            }
        }
    }
}
