using FlatRedBall.Glue.FormHelpers;
using FlatRedBall.Glue.SaveClasses;

namespace WfcPlugin.Extensions
{
    public static class NamedObjectSaveExtensions
    {
        public static bool TryGetLayeredTileMap(this NamedObjectSave namedObjectSave, out NamedObjectSave map)
        {
            if (namedObjectSave?.SourceClassType == "FlatRedBall.TileGraphics.LayeredTileMap")
            {
                map = namedObjectSave;
                return true;
            }

            map = null;
            return false;
        }

        public static bool TryGetLayeredTileMap(this ITreeNode treeNode, out NamedObjectSave map)
        {
            if (treeNode?.Tag is NamedObjectSave namedObjectSave)
            {
                return TryGetLayeredTileMap(namedObjectSave, out map);
            }

            map = null;
            return false;
        }
    }
}
