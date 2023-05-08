using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;

namespace DynamicPluginPlugin.Models
{
    public class PluginAssembly
    {
        // TODO : Add file watch to for when assembly changes
        // - update plugin details
        // - unload/reload plugin

        private FileSystemWatcher _watcher;

        public PluginAssembly(string sourcePath)
        {
            Id = Guid.NewGuid();
            SourcePath = sourcePath;
        }

        public Guid Id { get; }
        public string SourcePath { get; }
        public List<Plugin> Plugins { get; } = new();

        public AssemblyLoadContext LoadContext { get; private set; }
        public bool IsLoaded { get => LoadContext != null; }

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }

            Stream assemblySymbols = null;
            var symbolsPath = $"{Path.Combine(Path.GetDirectoryName(SourcePath), Path.GetFileNameWithoutExtension(SourcePath))}.pdb";
            if (File.Exists(symbolsPath))
            {
                assemblySymbols = File.OpenRead(SourcePath);
            }

            using var assembly = File.OpenRead(SourcePath);

            LoadContext = new AssemblyLoadContext(Path.GetFileName(SourcePath), true);
            LoadContext.LoadFromStream(assembly, assemblySymbols);

            assemblySymbols?.Dispose();
        }

        public void Unload()
        {
            if (!IsLoaded)
            {
                return;
            }

            // TODO : SingleDynamicPlugin does not unload (but TutorialPlugin does)
            // Showing and hiding tab stops garbage collection

            LoadContext.Unload();
            LoadContext = null;
        }

        public void Watch()
        {
            if (_watcher != null)
            {
                return;
            }

            //_watcher = new FileSystemWatcher(Path.GetDirectoryName(SourcePath), Path.GetFileName(SourcePath));
            //_watcher.Changed += Watcher_Changed;
            //_watcher.Deleted += Watcher_Deleted;
            //_watcher.Renamed += Watcher_Renamed;
            //_watcher.EnableRaisingEvents = true;
        }

        public void Unwatch()
        {
            if (_watcher == null)
            {
                return;
            }

            //_watcher.EnableRaisingEvents = false;
            //_watcher.Changed -= Watcher_Changed;
            //_watcher.Deleted -= Watcher_Deleted;
            //_watcher.Renamed -= Watcher_Renamed;
            //_watcher.Dispose();
            //_watcher = null;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
        }
    }
}
