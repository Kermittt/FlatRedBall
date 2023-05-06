using DynamicPluginPlugin.Models;
using FlatRedBall.Glue.MVVM;

namespace DynamicPluginPlugin.ViewModels
{
    public class PluginViewModel : ViewModel
    {
        public PluginViewModel()
        {
        }

        public PluginViewModel(Plugin model)
        {
            Name = model.Name;
            Path = model.Path;
            IsEnabled = model.IsEnabled;
            Version = model.Version;
        }

        public string Name
        {
            get => Get<string>();
            init => Set(value);
        }

        public string Path
        {
            get => Get<string>();
            init => Set(value);
        }

        public bool IsEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string Version
        {
            get => Get<string>();
            init => Set(value);
        }

        public string Type
        {
            get => Get<string>();
            init => Set(value);
        }
    }
}
