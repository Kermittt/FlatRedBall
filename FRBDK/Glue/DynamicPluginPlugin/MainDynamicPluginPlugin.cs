using DynamicPluginPlugin.Models;
using DynamicPluginPlugin.ViewModels;
using DynamicPluginPlugin.Views;
using FlatRedBall.Glue.Plugins;
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
        private Dictionary<string, AssemblyLoadContext> _contexts = new();
        private Dictionary<string, Plugin> _plugins = new();

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
            var context = LoadPluginAssembly(path);
            _contexts.Add(path, context);

            return context.Assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.IsPublic && !t.IsAbstract && t.IsAssignableTo(typeof(PluginBase)))
                .Select(t =>
                {
                    var instance = (PluginBase)Activator.CreateInstance(t);
                    var plugin = new Plugin()
                    {
                        Path = path,
                        IsEnabled = true,
                        Name = instance.FriendlyName,
                        Version = instance.Version.ToString()
                        // TODO : Set Type to be transportable type name
                    };
                    _plugins.Add(plugin.Name, plugin);
                    return plugin;
                });
        }

        public void EnablePlugin()
        {
        }

        public void DisablePlugin()
        {
        }

        private static AssemblyLoadContext LoadPluginAssembly(string path)
        {
            var context = new AssemblyLoadContext(Path.GetFileName(path), true);
            context.LoadFromAssemblyPath(path);
            return context;
        }
    }
}