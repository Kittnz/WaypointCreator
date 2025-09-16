using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class MapConfig
{
    public int Id { get; set; }
    public string Directory { get; set; }
    public float MapSize { get; set; } = 34133.33f;
    public float TileSize { get; set; } = 533.3333f;
}

public static class MapManager
{
    private static List<MapConfig> _maps = new List<MapConfig>();
    private static string _baseTilePath = Path.Combine(Application.StartupPath, "world", "minimaps");
    private static DateTime _lastLoadTime;
    private static string _currentCsvPath;

    public static void Initialize(string baseTilePath = null)
    {
        _baseTilePath = Path.Combine(Application.StartupPath,
            baseTilePath ?? "world\\minimaps");

        Console.WriteLine($"Tile base path: {_baseTilePath}");
    }

    public static void LoadMaps(string csvPath)
    {
        try
        {
            _currentCsvPath = csvPath;
            _lastLoadTime = File.GetLastWriteTime(csvPath);
            _maps.Clear();

            foreach (var line in File.ReadAllLines(csvPath).Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int id))
                {
                    _maps.Add(new MapConfig
                    {
                        Id = id,
                        Directory = parts[1].Trim(),
                        MapSize = 34133.33f,
                        TileSize = 533.3333f
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading maps: {ex.Message}");
        }
    }

    public static void ReloadIfChanged()
    {
        if (_currentCsvPath == null || !File.Exists(_currentCsvPath)) return;

        var lastWriteTime = File.GetLastWriteTime(_currentCsvPath);
        if (lastWriteTime > _lastLoadTime)
        {
            LoadMaps(_currentCsvPath);
        }
    }

    public static MapConfig GetMapConfig(int mapId) => _maps.FirstOrDefault(m => m.Id == mapId);
    public static string GetTilePath(int mapId) => GetMapConfig(mapId) != null ?
        Path.Combine(_baseTilePath, GetMapConfig(mapId).Directory) : null;
}