using FlatRedBall.Glue.MVVM;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DynamicPluginPlugin.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public MainViewModel()
        {
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
