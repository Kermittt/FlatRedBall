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
using System.Runtime.Loader;

namespace DynamicPluginPlugin
{
    [Export(typeof(PluginBase))]
    public class MainDynamicPluginPlugin : PluginBase
    {
        private readonly Dictionary<string, AssemblyLoadContext> _contexts = new();
        private readonly Dictionary<Guid, Plugin> _plugins = new();

        private PluginTab _tab;
        private MainViewModel _viewModel;

        public override string FriendlyName => "Dynamic Plugin Plugin";

        public override void StartUp()
        {
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

        public IEnumerable<Plugin> AddPluginAssembly(string path)
        {
            // TODO : Need to make sure plugin is not already loaded by the main plugin system

            if (_contexts.ContainsKey(path))
            {
                throw new Exception("Plugin assembly already loaded");
            }

            // Instantiate and create all plugins in the assembly
            var context = LoadPluginAssembly(path);
            var plugins = context.Assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.IsPublic && !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IPlugin)))
                .Select(t =>
                {
                    var instance = (IPlugin)Activator.CreateInstance(t);
                    var plugin = new Plugin()
                    {
                        Id = t.GUID,
                        Name = instance.FriendlyName,
                        Version = instance.Version.ToString(),
                        Path = path,
                        Type = t.UnversionedName(),
                        Instance = instance
                    };
                    _plugins.Add(plugin.Id, plugin);
                    StartPlugin(plugin);
                    return plugin;
                })
                .ToArray();

            // Add the assembly, or unload it immediately if it didn't contain any plugins
            if (plugins.Length > 0)
            {
                _contexts.Add(path, context);
            }
            else
            {
                context.Unload();
            }

            return plugins;
        }

        public void RemovePluginAssembly(string path)
        {
            // Disable and remove all plugins in the assembly
            foreach (var plugin in GetPluginsForAssembly(path))
            {
                if (plugin.IsEnabled)
                {
                    DisablePlugin(plugin);
                }
                _plugins.Remove(plugin.Id);
            }

            // Remove the assembly
            _contexts.Remove(path);
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
            if (_contexts[plugin.Path] == null)
            {
                _contexts[plugin.Path] = LoadPluginAssembly(plugin.Path);
            }

            // Instantiate the plugin
            var type = Type.GetType(plugin.Type, (assemblyName) => _contexts[plugin.Path].LoadFromAssemblyName(assemblyName), null, true);
            plugin.Instance = (IPlugin)Activator.CreateInstance(type);
            StartPlugin(plugin);
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
            StopPlugin(plugin);
            plugin.Instance = null;

            // If all plugins in the assembly are disabled, unload the assembly
            if (GetPluginsForAssembly(plugin.Path).All(p => !p.IsEnabled))
            {
                UnloadPluginAssembly(plugin.Path);
            }
        }

        public IEnumerable<Plugin> GetPluginsForAssembly(string path)
        {
            return _plugins.Values.Where(p => p.Path == path);
        }

        private static void StartPlugin(Plugin plugin)
        {
            plugin.Instance.StartUp();
        }

        private static void StopPlugin(Plugin plugin)
        {
            plugin.Instance.ShutDown(PluginShutDownReason.UserDisabled);
        }

        private void UnloadPluginAssembly(string path)
        {
            _contexts[path].Unload();
            _contexts[path] = null;
        }

        private static AssemblyLoadContext LoadPluginAssembly(string path)
        {
            var context = new AssemblyLoadContext(Path.GetFileName(path), true);
            context.LoadFromAssemblyPath(path);
            return context;
        }
    }
}