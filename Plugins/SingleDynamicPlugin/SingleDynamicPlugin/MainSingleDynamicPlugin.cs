using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.Interfaces;
using SingleDynamicPlugin.ViewModels;
using SingleDynamicPlugin.Views;

namespace SingleDynamicPlugin
{
    public class MainSingleDynamicPlugin : PluginBase
    {
        private PluginTab _tab;
        private MainViewModel _viewModel;

        public override string FriendlyName => "Single Dynamic Plugin";

        public override void StartUp()
        {
            EnsureTabCreated();
            _tab.Show();
        }

        public override bool ShutDown(PluginShutDownReason shutDownReason)
        {
            _tab.Hide();

            return true;
        }

        private void EnsureTabCreated()
        {
            if (_tab != null)
            {
                return;
            }

            _viewModel = new MainViewModel();
            var view = new MainView() { DataContext = _viewModel };
            _tab = CreateAndAddTab(view, "Single Dynamic Plugin", TabLocation.Center);
        }
    }
}