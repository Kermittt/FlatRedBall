namespace DynamicPluginPlugin.Models
{
    public class Plugin
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsEnabled { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
    }
}
