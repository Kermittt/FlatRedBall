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
        private readonly Dictionary<string, Plugin> _plugins = new();

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
                        Name = instance.FriendlyName,
                        Version = instance.Version.ToString(),
                        Path = path,
                        Type = t.UnversionedName(),
                        Instance = instance
                    };
                    _plugins.Add(plugin.Type, plugin);
                    StartPlugin(plugin);
                    return plugin;
                })
                .ToArray();

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

        public void EnablePlugin(string type)
        {
            if (!_plugins.TryGetValue(type, out var plugin))
            {
                throw new Exception($"Cannot find a plugin of type {type}");
            }

            EnablePlugin(plugin);
        }

        public void EnablePlugin(Plugin plugin)
        {
            if (plugin.Instance != null)
            {
                throw new Exception($"Plugin is already enabled for type {plugin.Type}");
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

        public void DisablePlugin(string type)
        {
            if (!_plugins.TryGetValue(type, out var plugin))
            {
                throw new Exception($"Cannot find a plugin of type {type}");
            }

            DisablePlugin(plugin);
        }

        public void DisablePlugin(Plugin plugin)
        {
            if (plugin.Instance == null)
            {
                throw new Exception($"Plugin is already disabled for type {plugin.Type}");
            }

            // Destroy the plugin
            StopPlugin(plugin);
            plugin.Instance = null;

            // If all plugins in the assembly are disabled, unload the assembly
            if (GetPluginsForAssembly(plugin.Path).All(p => p.Instance == null))
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