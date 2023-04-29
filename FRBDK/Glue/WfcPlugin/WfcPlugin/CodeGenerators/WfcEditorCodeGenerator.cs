using FlatRedBall.Glue.CodeGeneration;
using FlatRedBall.Glue.CodeGeneration.CodeBuilder;
using FlatRedBall.Glue.SaveClasses;
using System.Collections.Generic;
using System.Linq;
using WfcPlugin.Extensions;

namespace WfcPlugin.CodeGenerators
{
    public class WfcEditorCodeGenerator : ElementComponentCodeGenerator
    {
        public override ICodeBlock GenerateFields(ICodeBlock codeBlock, IElement element)
        {
            foreach (var map in GetMaps(element).Where(m => !m.DefinedByBase))
            {
                var access = map.SetByDerived ? "protected" : "private";
                codeBlock.Line($"{access} WfcCore.Wfc.WfcMap {map.FieldName}_WfcMap;");
            }

            return codeBlock;
        }

        public override ICodeBlock GenerateInitializeLate(ICodeBlock codeBlock, IElement element)
        {
            // TODO : Initialize Speed and Seed here?
            foreach (var map in GetMaps(element).Where(m => !m.SetByDerived))
            {
                codeBlock.Line($"{map.FieldName}_WfcMap = new WfcCore.Wfc.WfcMap({map.FieldName});");
            }

            return codeBlock;
        }

        public override ICodeBlock GenerateActivity(ICodeBlock codeBlock, IElement element)
        {
            foreach (var map in GetMaps(element).Where(m => !m.SetByDerived))
            {
                codeBlock.Line($"{map.FieldName}_WfcMap.ScrollMap();");
                codeBlock.Line($"{map.FieldName}_WfcMap.PlayerInput();");
            }

            return codeBlock;
        }

        private static List<NamedObjectSave> GetMaps(IElement element)
        {
            return element.NamedObjects
                .Where(n => n.TryGetLayeredTileMap(out var _))
                .ToList();
        }
    }
}
