using FlatRedBall.Glue.FormHelpers;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.SaveClasses;
using System;
using System.ComponentModel.Composition;
using WfcPlugin.Controls;
using WfcPlugin.ViewModels;

namespace WfcPlugin
{
    [Export(typeof(PluginBase))]
    public class MainWfcPlugin : PluginBase
    {
        private WfcEditorControl _control;
        private WfcEditorViewModel _viewModel;
        private PluginTab _tab;

        public override string FriendlyName => "WFC Plugin";
        public override Version Version => new(1, 0);

        public override void StartUp()
        {
            ReactToItemSelectHandler += HandleItemSelected;
        }

        private void HandleItemSelected(ITreeNode selectedTreeNode)
        {
            if (selectedTreeNode.Tag is NamedObjectSave namedObjectSave
                && namedObjectSave.InstanceType == "FlatRedBall.TileGraphics.LayeredTileMap")
            {
                EnsureTabCreated(namedObjectSave);
                _viewModel.UpdateFromGlueObject();
            }
            else
            {
                _tab?.Hide();
            }
        }

        private void EnsureTabCreated(NamedObjectSave namedObjectSave)
        {
            if (_tab != null)
            {
                _viewModel.GlueObject = namedObjectSave;
                _tab.Show();
                return;
            }

            _viewModel = new WfcEditorViewModel() { GlueObject = namedObjectSave };
            _control = new WfcEditorControl() { DataContext = _viewModel };
            _tab = CreateAndAddTab(_control, "WFC");
        }
    }
}