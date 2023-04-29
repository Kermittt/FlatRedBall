using FlatRedBall.Glue.FormHelpers;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.SaveClasses;
using FlatRedBall.Glue.VSHelpers;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using WfcPlugin.CodeGenerators;
using WfcPlugin.Controls;
using WfcPlugin.Extensions;
using WfcPlugin.ViewModels;

namespace WfcPlugin
{
    [Export(typeof(PluginBase))]
    public class MainWfcPlugin : PluginBase
    {
        private CodeBuildItemAdder _codeBuildItemAdder;
        private WfcEditorControl _control;
        private WfcEditorViewModel _viewModel;
        private PluginTab _tab;

        public override string FriendlyName => "Wave Function Collapse Map Plugin";
        public override Version Version => new(1, 0);

        public override void StartUp()
        {
            ReactToLoadedGlux += HandleGluxLoaded;
            ReactToItemSelectHandler += HandleItemSelected;

            _codeBuildItemAdder = new CodeBuildItemAdder()
            {
                AddAsGenerated = true,
                OutputFolderInProject = "Wfc"
            };
            _codeBuildItemAdder.AddFolder("WfcCore/Wfc", Assembly.GetAssembly(typeof(WfcCore.Wfc.WfcMap)));

            RegisterCodeGenerator(new WfcEditorCodeGenerator());
        }

        private void HandleGluxLoaded()
        {
            _codeBuildItemAdder.PerformAddAndSaveTask(Assembly.GetAssembly(typeof(WfcCore.Wfc.WfcMap)));
        }

        private void HandleItemSelected(ITreeNode selectedTreeNode)
        {
            if (selectedTreeNode.TryGetLayeredTileMap(out var map))
            {
                EnsureTabCreated(map);
                _viewModel.UpdateFromGlueObject();

                // TODO : Only do this if something has changed? (e.g. properties)
                // TODO : How do we ensure it is executed on existing objects without having to select them in the tree?
                GlueCommands.Self.GenerateCodeCommands.GenerateCurrentElementCode();
            }
            else
            {
                _tab?.Hide();
            }
        }

        private void EnsureTabCreated(NamedObjectSave map)
        {
            if (_tab != null)
            {
                _viewModel.GlueObject = map;
                _tab.Show();
                return;
            }

            _viewModel = new WfcEditorViewModel() { GlueObject = map };
            _control = new WfcEditorControl() { DataContext = _viewModel };
            _tab = CreateAndAddTab(_control, "WFC");
        }
    }
}