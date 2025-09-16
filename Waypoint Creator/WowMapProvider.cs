using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Frm_waypoint
{
    public class WowMapProvider : IDisposable
    {
        public class MapTile
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string FilePath { get; set; }
            public SKBitmap Bitmap { get; set; }
            public bool Loaded => Bitmap != null;
        }

        public float Zoom { get; set; } = 3.0f;
        public PointF Center { get; set; } = PointF.Empty;
        public SKColor BackgroundColor { get; set; } = SKColors.LightGray;

        private readonly Dictionary<int, List<MapTile>> _mapTiles = new Dictionary<int, List<MapTile>>();
        private int _currentMapId = -1;
        private MapConfig _currentMapConfig;

        public bool IsMapLoaded => _currentMapConfig != null;

        public void LoadMap(int mapId)
        {
            if (_currentMapId == mapId) return;

            _currentMapId = mapId;
            _currentMapConfig = MapManager.GetMapConfig(mapId);

            if (!_mapTiles.ContainsKey(mapId))
            {
                _mapTiles[mapId] = LoadTilesForMap(mapId);
            }
        }

        private List<MapTile> LoadTilesForMap(int mapId)
        {
            var tiles = new List<MapTile>();
            string mapPath = MapManager.GetTilePath(mapId);

            if (!Directory.Exists(mapPath))
            {
                Console.WriteLine($"[ERROR] Map directory not found: {mapPath}");
                return tiles;
            }

            foreach (var file in Directory.GetFiles(mapPath, "*.webp"))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.StartsWith("map")) fileName = fileName.Substring(3);

                var parts = fileName.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                {
                    tiles.Add(new MapTile { X = x, Y = y, FilePath = file });
                }
            }
            return tiles;
        }

        public PointF GameToWorld(PointF gamePoint)
        {
            return new PointF(-gamePoint.Y, -gamePoint.X);
        }

        public PointF WorldToGame(PointF worldPoint)
        {
            return new PointF(-worldPoint.Y, -worldPoint.X);
        }

        public SKRect GetTileWorldRect(int tileX, int tileY)
        {
            if (!IsMapLoaded) return SKRect.Empty;

            float tileSize = _currentMapConfig.TileSize;
            float worldX = (tileX - 32) * tileSize;
            float worldY = (tileY - 32) * tileSize;

            return new SKRect(worldX, worldY, worldX + tileSize, worldY + tileSize);
        }

        public List<MapTile> GetTiles()
        {
            return IsMapLoaded ? _mapTiles[_currentMapId] : new List<MapTile>();
        }

        public SKRect GetMapBounds()
        {
            if (!IsMapLoaded) return SKRect.Empty;

            // Calculate map bounds based on tile configuration
            float tileSize = _currentMapConfig.TileSize;
            float minX = -32 * tileSize;
            float minY = -32 * tileSize;
            float maxX = 32 * tileSize;
            float maxY = 32 * tileSize;

            return new SKRect(minX, minY, maxX, maxY);
        }

        public SKBitmap LoadTile(MapTile tile)
        {
            if (tile.Loaded) return tile.Bitmap;

            try
            {
                using (var stream = File.OpenRead(tile.FilePath))
                {
                    tile.Bitmap = SKBitmap.Decode(stream);
                    return tile.Bitmap;
                }
            }
            catch
            {
                Console.WriteLine($"[ERROR] Failed to load tile: {tile.FilePath}");
                return null;
            }
        }

        public void Dispose()
        {
            foreach (var tileList in _mapTiles.Values)
            {
                foreach (var tile in tileList)
                {
                    tile.Bitmap?.Dispose();
                }
            }
        }
    }
}