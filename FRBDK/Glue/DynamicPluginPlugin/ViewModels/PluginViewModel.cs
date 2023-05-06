using FlatRedBall.Glue.MVVM;

namespace DynamicPluginPlugin.ViewModels
{
    public class PluginViewModel : ViewModel
    {
        public string Name
        {
            get => Get<string>();
            private set => Set(value);
        }

        public string FullPath
        {
            get => Get<string>();
            private set => Set(value);
        }

        public bool IsEnabled
        {
            get => Get<bool>();
            private set => Set(value);
        }

        public string Version
        {
            get => Get<string>();
            private set => Set(value);
        }
    }
}
