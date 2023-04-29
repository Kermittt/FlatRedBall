using System.Diagnostics;

namespace WfcCore.Wfc
{
    [DebuggerDisplay("{TopRight}-{BottomRight}-{BottomLeft}-{TopLeft}")]
    public readonly struct WfcCorners
    {
        public WfcCorners(byte topRight, byte bottomRight, byte bottomLeft, byte topLeft)
        {
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
        }

        public byte TopRight { get; }
        public byte BottomRight { get; }
        public byte BottomLeft { get; }
        public byte TopLeft { get; }

        public bool CanConnectTo(WfcEdge edge, WfcCorners other)
        {
            return edge switch
            {
                WfcEdge.Top => TopLeft == other.BottomLeft && TopRight == other.BottomRight,
                WfcEdge.Right => TopRight == other.TopLeft && BottomRight == other.BottomLeft,
                WfcEdge.Bottom => BottomLeft == other.TopLeft && BottomRight == other.TopRight,
                WfcEdge.Left => TopLeft == other.TopRight && BottomLeft == other.BottomRight,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
