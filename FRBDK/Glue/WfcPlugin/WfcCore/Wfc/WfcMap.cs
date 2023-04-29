using FlatRedBall.TileGraphics;

namespace WfcCore.Wfc
{
    public class WfcMap
    {
        private readonly LayeredTileMap _layeredTileMap;

        public WfcMap(LayeredTileMap layeredTileMap)
        {
            _layeredTileMap = layeredTileMap;
            System.Diagnostics.Debug.WriteLine("WfcMap");
        }
    }
}
