using System;
using System.Collections.Generic;
using System.Runtime.Loader;

namespace DynamicPluginPlugin.Models
{
    public class PluginAssembly
    {
        public Guid Id { get; init; }
        public string Path { get; init; }
        public List<Plugin> Plugins { get; } = new();

        public AssemblyLoadContext LoadContext { get; set; }
        public bool IsLoaded { get => LoadContext != null; }

        public void Load()
        {
            LoadContext = new AssemblyLoadContext(System.IO.Path.GetFileName(Path), true);
            LoadContext.LoadFromAssemblyPath(Path);
        }

        public void Unload()
        {
            LoadContext.Unload();
            LoadContext = null;
        }
    }
}
