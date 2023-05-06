using DynamicPluginPlugin.ViewModels;
using DynamicPluginPlugin.Views;
using FlatRedBall.Glue.Plugins;
using System.ComponentModel.Composition;

namespace DynamicPluginPlugin
{
    [Export(typeof(PluginBase))]
    public class MainDynamicPluginPlugin : PluginBase
    {
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

            _viewModel = new MainViewModel();
            var view = new MainView() { DataContext = _viewModel };
            _tab = CreateAndAddTab(view, "Dynamic Plugins", TabLocation.Left);
        }
    }
}