using FlatRedBall.Glue.MVVM;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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
                if (plugins.Any())
                {
                    foreach (var plugin in plugins)
                    {
                        var viewModel = new PluginViewModel(plugin);
                        viewModel.PropertyChanged += Plugin_PropertyChanged;
                        Plugins.Add(viewModel);
                    }
                }
                else
                {
                    MessageBox.Show("No plugins found in assembly.", "Dynamic Plugins");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading plugin assembly.\n\n{ex.Message}", "Dynamic Plugins");
            }
        }

        private void RemoveExecute()
        {

        }

        private bool RemoveCanExecute()
        {
            return SelectedPlugin != null;
        }

        private void Plugin_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginViewModel.IsEnabled))
            {
                try
                {
                    var viewModel = (PluginViewModel)sender;
                    if (viewModel.IsEnabled)
                    {
                        _main.EnablePlugin(viewModel.Type);
                    }
                    else
                    {
                        _main.DisablePlugin(viewModel.Type);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating plugin.\n\n{ex.Message}", "Dynamic Plugins");
                }
            }
        }
    }
}
