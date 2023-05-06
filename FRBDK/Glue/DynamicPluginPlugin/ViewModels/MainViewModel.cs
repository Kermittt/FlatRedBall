using FlatRedBall.Glue.MVVM;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DynamicPluginPlugin.ViewModels
{
    public class MainViewModel : ViewModel
    {
        private readonly MainDynamicPluginPlugin _main;

        public MainViewModel()
        {
            Plugins.Add(new PluginViewModel() { Name = "Sample Plugin 1" });
            Plugins.Add(new PluginViewModel() { Name = "Sample Plugin 2" });
        }

        public MainViewModel(MainDynamicPluginPlugin main)
        {
            _main = main;

            AddCommand = new CommandBase(AddExecute);
            RemoveCommand = new CommandBase(RemoveExecute, RemoveCanExecute);
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ObservableCollection<PluginViewModel> Plugins { get; } = new();

        public PluginViewModel SelectedPlugin
        {
            get => Get<PluginViewModel>();
            set => Set(value);
        }

        private void AddExecute()
        {
            var ofd = new OpenFileDialog()
            {
                Title = "Open plugin assembly",
                DefaultExt = ".dll",
                Filter = "Plugin Assemblies (.dll)|*.dll"
            };
            if (ofd.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var plugins = _main.AddPluginAssembly(ofd.FileName);
                foreach (var plugin in plugins)
                {
                    Plugins.Add(new PluginViewModel(plugin));
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void RemoveExecute()
        {

        }

        private bool RemoveCanExecute()
        {
            return SelectedPlugin != null;
        }
    }
}
