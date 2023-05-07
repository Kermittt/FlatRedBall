using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;

namespace DynamicPluginPlugin.Models
{
    public class PluginAssembly
    {
        public PluginAssembly(string sourcePath)
        {
            Id = Guid.NewGuid();
            SourcePath = sourcePath;
        }

        public Guid Id { get; }
        public string SourcePath { get; }
        public List<Plugin> Plugins { get; } = new();

        public AssemblyLoadContext LoadContext { get; set; }
        public bool IsLoaded { get => LoadContext != null; }

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }

            LoadContext = new AssemblyLoadContext(Path.GetFileName(SourcePath), true);

            using var fs = File.OpenRead(SourcePath);
            LoadContext.LoadFromStream(fs);
        }

        public void Unload()
        {
            if (!IsLoaded)
            {
                return;
            }

            LoadContext.Unload();
            LoadContext = null;
        }
    }
}
