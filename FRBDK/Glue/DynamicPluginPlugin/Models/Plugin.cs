using FlatRedBall.Glue.Plugins.Interfaces;

namespace DynamicPluginPlugin.Models
{
    public class Plugin
    {
        public string Name { get; init; }
        public string Path { get; init; }
        public string Version { get; init; }
        public string Type { get; init; }
        public IPlugin Instance { get; set; }
    }
}
