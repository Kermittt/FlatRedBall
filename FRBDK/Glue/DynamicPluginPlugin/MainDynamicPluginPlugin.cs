using DynamicPluginPlugin.Extensions;
using DynamicPluginPlugin.Models;
using DynamicPluginPlugin.ViewModels;
using DynamicPluginPlugin.Views;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace DynamicPluginPlugin
{
    [Export(typeof(PluginBase))]
    public class MainDynamicPluginPlugin : PluginBase
    {
        private readonly Dictionary<Guid, PluginAssembly> _pluginAssemblies = new();
        private readonly Dictionary<Guid, Plugin> _plugins = new();

        private DirectoryInfo _cacheDirectory;
        private PluginTab _tab;
        private MainViewModel _viewModel;

        public override string FriendlyName => "Dynamic Plugin Plugin";

        /// <summary>
        /// Set to <c>true</c> to force garbage collection whenever an assembly is unloaded.
        /// </summary>
        public bool ForceGC { get; set; } = true;

        public override void StartUp()
        {
            _cacheDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PluginCache"));
            if (!_cacheDirectory.Exists)
            {
                _cacheDirectory.Create();
            }

            EnsureTabCreated();
            _tab.Show();
        }

        private void EnsureTabCreated()
        {
            if (_tab != null)
            {
                return;
            }

            _viewModel = new MainViewModel(this);
            var view = new MainView() { DataContext = _viewModel };
            _tab = CreateAndAddTab(view, "Dynamic Plugins", TabLocation.Left);
        }

        public PluginAssembly AddPluginAssembly(string path)
        {
            // TODO : Need to make sure plugin is not already loaded by the main plugin system

            if (_pluginAssemblies.Values.Any(a => a.Path == path))
            {
                throw new Exception("Plugin assembly already loaded");
            }

            // Instantiate and create all plugins in the assembly
            var pluginAssembly = new PluginAssembly()
            {
                Path = path
            };
            pluginAssembly.Load();
            var plugins = pluginAssembly.LoadContext.Assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.IsPublic && !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IPlugin)))
                .Select(t =>
                {
                    var instance = (IPlugin)Activator.CreateInstance(t);
                    var plugin = new Plugin()
                    {
                        Id = t.GUID,
                        AssemblyId = pluginAssembly.Id,
                        Name = instance.FriendlyName,
                        Version = instance.Version.ToString(),
                        Path = path,
                        Type = t.UnversionedName()
                    };

                    pluginAssembly.Plugins.Add(plugin);
                    _plugins.Add(plugin.Id, plugin);

                    plugin.Start(instance);
                    return plugin;
                })
                .ToArray();

            // Unload the assembly immediately if it didn't contain any plugins
            if (plugins.Length == 0)
            {
                pluginAssembly.Unload();
                DoGC();

                return null;
            }

            _pluginAssemblies.Add(pluginAssembly.Id, pluginAssembly);
            return pluginAssembly;
        }

        public void RemovePluginAssembly(Guid id)
        {
            // Disable and remove all plugins in the assembly
            foreach (var plugin in _pluginAssemblies[id].Plugins)
            {
                if (plugin.IsEnabled)
                {
                    DisablePlugin(plugin);
                }
                _plugins.Remove(plugin.Id);
            }

            // Remove the assembly
            _pluginAssemblies.Remove(id);
        }

        public void EnablePlugin(Guid id)
        {
            if (!_plugins.TryGetValue(id, out var plugin))
            {
                throw new Exception($"Cannot find a plugin with id {id}");
            }

            EnablePlugin(plugin);
        }

        public void EnablePlugin(Plugin plugin)
        {
            if (plugin.IsEnabled)
            {
                throw new Exception($"Plugin is already enabled for id {plugin.Id}");
            }

            // If all plugins in the assembly are disabled, load the assembly
            var pluginAssembly = _pluginAssemblies[plugin.AssemblyId];
            if (!pluginAssembly.IsLoaded)
            {
                pluginAssembly.Load();
            }

            // Instantiate the plugin
            var type = Type.GetType(plugin.Type, (assemblyName) => pluginAssembly.LoadContext.LoadFromAssemblyName(assemblyName), null, true);
            plugin.Start((IPlugin)Activator.CreateInstance(type));
        }

        public void DisablePlugin(Guid id)
        {
            if (!_plugins.TryGetValue(id, out var plugin))
            {
                throw new Exception($"Cannot find a plugin with id {id}");
            }

            DisablePlugin(plugin);
        }

        public void DisablePlugin(Plugin plugin)
        {
            if (!plugin.IsEnabled)
            {
                throw new Exception($"Plugin is already disabled with id {plugin.Id}");
            }

            // Destroy the plugin
            plugin.Stop();

            // If all plugins in the assembly are disabled, unload the assembly
            var pluginAssembly = _pluginAssemblies[plugin.AssemblyId];
            if (pluginAssembly.Plugins.All(p => !p.IsEnabled))
            {
                pluginAssembly.Unload();
                DoGC();
            }
        }

        private void DoGC()
        {
            if (!ForceGC)
            {
                return;
            }

            GC.Collect();
        }
    }
}