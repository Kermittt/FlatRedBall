using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WfcCore.Wfc
{
    [DebuggerDisplay("{TileId} : {Corners}")]
    public class WfcTile
    {
        private readonly WfcTerrain[] _terrains;

        public WfcTile(int tileId, string wangId, double probability, WfcTerrain[] terrains)
        {
            TileId = tileId;
            Probability = probability;
            _terrains = terrains;
            var wangColors = wangId.Split(',').Select(c => byte.Parse(c)).ToArray();
            Corners = new WfcCorners(wangColors[1], wangColors[3], wangColors[5], wangColors[7]);
        }

        public int TileId { get; }
        public WfcCorners Corners { get; }
        public double Probability { get; }
        public List<WfcTile>[] ValidNeighbours { get; } = new List<WfcTile>[] {
            new(), new(), new(), new()
        };

        public double GetTotalProbability()
        {
            return _terrains[Corners.TopRight - 1].Probability
                * _terrains[Corners.BottomRight - 1].Probability
                * _terrains[Corners.BottomLeft - 1].Probability
                * _terrains[Corners.TopLeft - 1].Probability
                * Probability;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            WfcTile other = (WfcTile)obj;
            return TileId == other.TileId;
        }

        public override int GetHashCode()
        {
            return TileId;
        }
    }
}
