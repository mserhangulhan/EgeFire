namespace EasyTaskStarter.Models
{
    public class QuickAccessItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public QuickAccessItem(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
