using FlatRedBall.Glue.Plugins.Interfaces;
using System;

namespace DynamicPluginPlugin.Models
{
    public class Plugin
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Path { get; init; }
        public string Version { get; init; }
        public string Type { get; init; }
        public IPlugin Instance { get; set; }

        public bool IsEnabled { get => Instance != null; }
    }
}
