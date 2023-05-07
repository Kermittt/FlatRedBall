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

        public Guid AssemblyId { get; init; }
        public IPlugin Instance { get; private set; }
        public bool IsEnabled { get => Instance != null; }

        public void Start(IPlugin instance)
        {
            Instance = instance;
            Instance.StartUp();
        }

        public void Stop()
        {
            Instance.ShutDown(PluginShutDownReason.UserDisabled);
            Instance = null;
        }
    }
}
