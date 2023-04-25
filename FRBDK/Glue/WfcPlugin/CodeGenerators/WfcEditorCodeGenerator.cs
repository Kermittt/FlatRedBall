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

            // TODO : Code is being generated in GameScreen, and duplicated in Overworld screen
            // Need to check NamedObjectSave properties to determine if it is derived?

            // TODO : Need to add "using WfcPlugin.Wfc"

            // TODO : File Wfc.cs should be named Wfc.Generated.cs?
            // Should I actually be generating this and changing namespace?

            foreach (var map in maps)
            {
                codeBlock.Line($"public WfcMap {map.FieldName}_WfcMap {{ get; set; }}");
            }

            return codeBlock;
        }
    }
}
