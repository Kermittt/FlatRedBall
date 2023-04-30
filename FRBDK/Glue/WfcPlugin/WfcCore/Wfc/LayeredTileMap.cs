using System.ComponentModel;
using WfcCore.Wfc;

namespace FlatRedBall.TileGraphics
{
    public partial class LayeredTileMap : IWfcMap
    {
        public double Speed { get; set; }
        public int? Seed { get; set; }
    }
}
