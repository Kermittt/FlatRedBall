using FlatRedBall.Glue.FormHelpers;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.SaveClasses;
using FlatRedBall.Glue.VSHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using WfcCore.Wfc;
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
            _codeBuildItemAdder.AddFolder("WfcCore/Wfc", Assembly.GetAssembly(typeof(WfcMap)));

            RegisterCodeGenerator(new WfcEditorCodeGenerator());
        }

        private void HandleGluxLoaded()
        {
            _codeBuildItemAdder.PerformAddAndSaveTask(Assembly.GetAssembly(typeof(WfcMap)));
        }

        private void HandleItemSelected(ITreeNode selectedTreeNode)
        {
            if (selectedTreeNode != null && selectedTreeNode.TryGetLayeredTileMap(out var map))
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

        // TODO : Why doesn't NamedObjectSave have CustomVariables to add to?

        //private bool AddVariables(NamedObjectSave map, Dictionary<string, Func<CustomVariable>> variables)
        //{

        //}

        //private bool AddVariable(NamedObjectSave map, string name, Func<CustomVariable> variable)
        //{
        //    //map.TypedMembers
        //    if (map.HasCustomVariable(name))
        //    {
        //        return false;
        //    }
        //}

        private readonly Dictionary<string, Func<CustomVariable>> IWcfMapVariables = new()
        {
            { nameof(IWfcMap.Speed), SpeedVariable },
            { nameof(IWfcMap.Seed), SeedVariable }
        };

        private static CustomVariable SpeedVariable()
        {
            return new CustomVariable()
            {
                Name = nameof(IWfcMap.Speed),
                DefaultValue = 64d,
                Type = "double",
                SetByDerived = true,
                Category = "WFC"
            };
        }

        private static CustomVariable SeedVariable()
        {
            return new CustomVariable()
            {
                Name = nameof(IWfcMap.Seed),
                DefaultValue = null,
                Type = "int?",
                SetByDerived = true,
                Category = "WFC"
            };
        }
    }
}