using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;

namespace DynamicPluginPlugin.Models
{
    public class PluginAssembly
    {
        public PluginAssembly(string sourcePath, string cacheDirectory)
        {
            Id = Guid.NewGuid();
            SourcePath = sourcePath;
            CachePath = Path.Combine(cacheDirectory, Id.ToString("D"), Path.GetFileName(sourcePath));

            if (!Directory.Exists(Path.GetDirectoryName(CachePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
            }

            File.Copy(SourcePath, CachePath, true);
        }

        public Guid Id { get; }
        public string SourcePath { get; }
        public string CachePath { get; }
        public List<Plugin> Plugins { get; } = new();

        public AssemblyLoadContext LoadContext { get; set; }
        public bool IsLoaded { get => LoadContext != null; }

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }

            LoadContext = new AssemblyLoadContext(Path.GetFileName(CachePath), true);

            using var fs = File.OpenRead(CachePath);
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

        public void Remove()
        {
            Unload();

            if (!Directory.Exists(Path.GetDirectoryName(CachePath)))
            {
                return;
            }

            Directory.Delete(Path.GetDirectoryName(CachePath), true);
        }
    }
}
