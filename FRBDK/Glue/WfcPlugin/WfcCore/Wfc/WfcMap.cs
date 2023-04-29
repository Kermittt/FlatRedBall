using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.TileGraphics;
using FlatRedBall.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WfcCore.Wfc
{
    public class WfcMap
    {
        private readonly LayeredTileMap _layeredTileMap;
        private readonly Xbox360GamePad _gamepad = InputManager.Xbox360GamePads[0];
        private readonly WfcEdge[] _edges = Enum.GetValues<WfcEdge>();

        private GameRandom _random;
        private int? _seed;
        private List<WfcTile>[] _wfcMap;
        private int _collapseRemain;
        private double _lastTileTime;
        private float _mapOffsetY;

        public WfcMap(LayeredTileMap layeredTileMap, int? seed = null)
        {
            _layeredTileMap = layeredTileMap;
            _seed = seed;
            _random = _seed != null ? new GameRandom(_seed.Value) : new GameRandom();

            Width = _layeredTileMap.NumberTilesWide.Value;
            Height = _layeredTileMap.NumberTilesTall.Value;
            TilePixelWidth = (int)_layeredTileMap.WidthPerTile.Value;
            TilePixelHeight = (int)_layeredTileMap.HeightPerTile.Value;
            PixelWidth = Width * TilePixelWidth;
            PixelHeight = Height * TilePixelHeight;

            GenerateTiles();
            ResetMap();

            for (var y = 0; y < Height; y++)
            {
                PopulateMapRow(y);
            }

            _mapOffsetY = TilePixelHeight;
            Camera.Main.Position = new Vector3(PixelWidth / 2, -PixelHeight / 2, Camera.Main.Z);
        }

        public WfcTerrain[] Terrains { get; private set; }
        public WfcTile[] Tiles { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TilePixelWidth { get; private set; }
        public int TilePixelHeight { get; private set; }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public int EmptyTileId { get; set; }
        public double Speed { get; set; } = 64;

        public int? Seed
        {
            get { return _seed; }
            set
            {
                if (_seed == value)
                {
                    return;
                }
                _seed = value;
                _random = _seed != null ? new GameRandom(_seed.Value) : new GameRandom();
            }
        }

        public void ScrollMap()
        {
            var tileSeconds = TimeManager.SecondsSince(_lastTileTime);
            var secondsPerTile = TilePixelHeight / Speed;
            while (tileSeconds > secondsPerTile)
            {
                tileSeconds -= secondsPerTile;
                _lastTileTime = TimeManager.CurrentScreenTime - tileSeconds;

                ShiftTilesForward(1);
                PopulateMapRow(Height - 1);
                PaintMap();
            }

            _layeredTileMap.Y = -(float)(tileSeconds * Speed) + _mapOffsetY;
        }

        public void PlayerInput()
        {
            Speed += _gamepad.RightStick.Vertical.Value;
            //System.Diagnostics.Debug.WriteLine(_pixelsPerSecond);
        }

        private void GenerateTiles()
        {
            Terrains = _layeredTileMap.Tilesets[0].wangsets[0].WangColors.Select(w =>
                new WfcTerrain(w.name, w.color, w.probability)).ToArray();
            Tiles = _layeredTileMap.Tilesets[0].wangsets[0].WangTiles.Select(w =>
            {
                var tile = _layeredTileMap.Tilesets[0].Tiles.First(t => t.id == w.tileid);
                if (tile.PropertyDictionary.TryGetValue("Empty", out var value)
                    && bool.TryParse(value, out var isEmpty)
                    && isEmpty)
                {
                    EmptyTileId = tile.id;
                }

                return new WfcTile(w.tileid, w.wangid, tile.Probability, Terrains);
            }).ToArray();

            foreach (var tile in Tiles)
            {
                foreach (var otherTile in Tiles)
                {
                    foreach (var edge in _edges)
                    {
                        if (tile.Corners.CanConnectTo(edge, otherTile.Corners))
                        {
                            tile.ValidNeighbours[(int)edge].Add(otherTile);
                        }
                    }
                }
            }
        }

        public void ResetMap()
        {
            _collapseRemain = Width * Height;
            _wfcMap = Enumerable.Range(0, _collapseRemain).Select(_ => Tiles.ToList()).ToArray();
        }

        public void PopulateMap()
        {
            while (!IsCollapsed())
            {
                Iterate();
            }
        }

        public void PopulateMapRow(int y)
        {
            var row = y * Width;
            var right = row + (Width / 2);
            var left = right - 1;
            while (left >= row)
            {
                CollapseAt(left);
                Propagate(left);

                CollapseAt(right);
                Propagate(right);

                left--;
                right++;
            }
        }

        public void Iterate()
        {
            var index = GetMinEntropyIndex();
            if (index == -1)
            {
                _collapseRemain = 0;
                return;
            }

            CollapseAt(index);
            Propagate(index);
        }

        public void CollapseAt(int index)
        {
            var list = _wfcMap[index];
            var tile = _random.InWeighted(list, t => t.GetTotalProbability());
            list.Clear();

            if (tile == null)
            {
                return;
            }

            list.Add(tile);
            PaintAt(index);
            _collapseRemain--;
        }

        private void PaintAt(int index)
        {
            var list = _wfcMap[index];
            var tileId = list.Count == 1 ? list[0].TileId : EmptyTileId;
            _layeredTileMap.MapLayers[0].PaintTile(index, tileId);
        }

        public void Propagate(int index)
        {
            var stack = new Stack<int>();

            do
            {
                foreach (var edge in _edges)
                {
                    if (!ValidEdge(index, edge) || _wfcMap[index].Count == 0)
                    {
                        continue;
                    }

                    var validNeighbours = _wfcMap[index].SelectMany(t => t.ValidNeighbours[(int)edge]).Distinct().ToArray();

                    var otherIndex = ShiftIndex(index, edge);
                    var otherNeighbours = _wfcMap[otherIndex];

                    if (otherNeighbours.Count == 1)
                    {
                        continue;
                    }

                    var modified = false;
                    for (var i = otherNeighbours.Count - 1; i > -1; i--)
                    {
                        if (!validNeighbours.Contains(otherNeighbours[i]))
                        {
                            otherNeighbours.RemoveAt(i);
                            modified = true;
                        }
                    }

                    if (otherNeighbours.Count == 1)
                    {
                        PaintAt(otherIndex);
                        _collapseRemain--;
                    }
                    if (modified && otherNeighbours.Count > 1 && !stack.Contains(otherIndex))
                    {
                        stack.Push(otherIndex);
                    }
                }
            } while (stack.TryPop(out index));
        }

        private bool ValidEdge(int index, WfcEdge edge)
        {
            return edge switch
            {
                WfcEdge.Top => index < (Width * Height) - Width,
                WfcEdge.Right => index % Width < Width - 1,
                WfcEdge.Bottom => index >= Width,
                WfcEdge.Left => index % Width > 0,
                _ => throw new NotImplementedException(),
            };
        }

        private int ShiftIndex(int index, WfcEdge edge)
        {
            return edge switch
            {
                WfcEdge.Top => index + Width,
                WfcEdge.Right => index + 1,
                WfcEdge.Bottom => index - Width,
                WfcEdge.Left => index - 1,
                _ => throw new NotImplementedException(),
            };
        }

        private int GetMinEntropyIndex()
        {
            // TODO : Should take into account tile (and terrain) weights
            var min = _wfcMap.Where(m => m.Count > 1).Min(m => m.Count);
            var allMins = _wfcMap.Select((m, i) => (m, i)).Where(t => t.m.Count == min).ToList();
            return _random.In(allMins).i;
        }

        public bool IsCollapsed()
        {
            return _collapseRemain == 0;
        }

        public void ShiftTilesForward(int y)
        {
            var startShiftIndex = y * Width;
            var endShiftIndex = _wfcMap.Length - startShiftIndex;
            Array.Copy(_wfcMap, startShiftIndex, _wfcMap, 0, endShiftIndex);

            for (var i = _wfcMap.Length - 1; i >= endShiftIndex; i--)
            {
                _wfcMap[i] = Tiles.ToList();
            }
            for (var i = endShiftIndex - 1; i > endShiftIndex - Width; i--)
            {
                Propagate(i);
            }
        }

        public void PaintMap()
        {
            for (var i = 0; i < _wfcMap.Length; i++)
            {
                PaintAt(i);
            }
        }
    }
}
