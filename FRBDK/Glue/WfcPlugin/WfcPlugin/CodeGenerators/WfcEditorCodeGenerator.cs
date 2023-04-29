using FlatRedBall.Glue.CodeGeneration;
using FlatRedBall.Glue.CodeGeneration.CodeBuilder;
using FlatRedBall.Glue.SaveClasses;
using System.Linq;
using WfcPlugin.Extensions;

namespace WfcPlugin.CodeGenerators
{
    public class WfcEditorCodeGenerator : ElementComponentCodeGenerator
    {
        public override ICodeBlock GenerateFields(ICodeBlock codeBlock, IElement element)
        {
            var maps = element.NamedObjects.Where(n => n.TryGetLayeredTileMap(out var _)).ToList();
            if (maps.Count == 0)
            {
                return codeBlock;
            }

            foreach (var map in maps.Where(m => !m.DefinedByBase))
            {
                codeBlock.Line($"public WfcCore.Wfc.WfcMap {map.FieldName}_WfcMap {{ get; set; }}");
            }

            return codeBlock;
        }
    }
}
