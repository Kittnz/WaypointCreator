using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Frm_waypoint
{
    public partial class Frm_Waypoint : Form
    {
        static DataTable waypoints = new DataTable();
        static DataTable guids = new DataTable();
        static DataTable movePackets = new DataTable();
        static DataSet copiedRows = new DataSet();
        static DataSet pasteTable = new DataSet();


        string creature_guid = "";
        string creature_entry = "";
        string creature_name = "";
        string mapID = "";

        float midX = 0;
        float midY = 0;

        private SKGLControl _mapControl;

        private readonly WowMapProvider _mapProvider = new WowMapProvider();

        private bool _isDragging = false;
        private Point _lastMousePosition;
        private float _zoomSensitivity = 0.1f;
        private float _minZoom = 0.1f;
        private float _maxZoom = 10f;
        private SKColor _backgroundColor = SKColors.LightGray;
        private List<string[]> _clip = new List<string[]>();
        private List<string> _originalListBoxItems = new List<string>();

        struct Packet
        {
            public string time;
            public string x;
            public string y;
            public string z;
            public string o;
            public string entry;
            public string guid;
        };

        // Sniff Varibles for sniffs
        public int state_map;
        public int move_time;
        public int move_entry;
        public int move_guid;
        public int move_x;
        public int move_y;
        public int move_z;
        public int move_o;
        public int move_pointx;
        public int move_pointy;
        public int move_pointz;
        public string sniff_state;
        public string sniff_move;
        public string sniff_move_1;
        public string sniff_move_2;
        public string sniff_move_3;
        public string sniff_move_4;
        public string sniff_move_5;
        public string sniff_move_6;
        public string sniff_move_7;

        public Frm_Waypoint()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            InitializeComponent();

            MapManager.Initialize(Path.Combine("world", "minimaps"));
            MapManager.LoadMaps(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.csv"));

            _mapControl = skiaMapControl;
            _mapControl.PaintSurface += MapControl_PaintSurface;

            _mapProvider = new WowMapProvider();

            // Set initial map
            _mapProvider.LoadMap(-1);

            _mapControl.MouseDown += MapControl_MouseDown;
            _mapControl.MouseMove += MapControl_MouseMove;
            _mapControl.MouseUp += MapControl_MouseUp;
            _mapControl.MouseWheel += MapControl_MouseWheel;

            toolStripTextBoxEntry.KeyDown += ToolStripTextBoxEntry_KeyDown;
        }

        private void MapControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastMousePosition = e.Location;
                _mapControl.Cursor = Cursors.Hand;
            }
        }

        private void MapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var delta = new Point(e.Location.X - _lastMousePosition.X, e.Location.Y - _lastMousePosition.Y);

                // Convert screen delta to world delta
                var worldDelta = new PointF(
                    delta.X / _mapProvider.Zoom,
                    delta.Y / _mapProvider.Zoom
                );

                // Calculate new center position
                var newCenter = new PointF(
                    _mapProvider.Center.X - worldDelta.X,
                    _mapProvider.Center.Y - worldDelta.Y
                );

                // Constrain to map bounds
                var mapBounds = _mapProvider.GetMapBounds();
                var visibleRect = GetVisibleWorldRect();

                // Calculate maximum allowed center position
                float maxCenterX = mapBounds.Right - (visibleRect.Width / 2);
                float minCenterX = mapBounds.Left + (visibleRect.Width / 2);
                float maxCenterY = mapBounds.Bottom - (visibleRect.Height / 2);
                float minCenterY = mapBounds.Top + (visibleRect.Height / 2);

                newCenter.X = Math.Max(minCenterX, Math.Min(maxCenterX, newCenter.X));
                newCenter.Y = Math.Max(minCenterY, Math.Min(maxCenterY, newCenter.Y));

                _mapProvider.Center = newCenter;
                _lastMousePosition = e.Location;
                _mapControl.Invalidate();
            }
        }

        private void MapControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                _mapControl.Cursor = Cursors.Default;
            }
        }

        private void MapControl_MouseWheel(object sender, MouseEventArgs e)
        {
            // Get mouse position relative to control
            var mousePos = e.Location;

            // Get world position before zoom
            var worldPos = ScreenToWorld(mousePos);

            float zoomDelta = e.Delta > 0 ? _zoomSensitivity : -_zoomSensitivity;
            float newZoom = _mapProvider.Zoom + zoomDelta;
            _mapProvider.Zoom = Math.Max(_minZoom, Math.Min(newZoom, _maxZoom));

            // Get world position after zoom
            var newWorldPos = ScreenToWorld(mousePos);

            // Adjust center to keep mouse position stable
            _mapProvider.Center = new PointF(
                _mapProvider.Center.X + (worldPos.X - newWorldPos.X),
                _mapProvider.Center.Y + (worldPos.Y - newWorldPos.Y)
            );

            _mapControl.Invalidate();
        }

        private void MapControl_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            _backgroundColor = new SKColor(
                Properties.Settings.Default.BackColour.R,
                Properties.Settings.Default.BackColour.G,
                Properties.Settings.Default.BackColour.B
                );

            canvas.Clear(_backgroundColor);

            // Get map bounds in world coordinates
            var mapBounds = _mapProvider.GetMapBounds();

            // Apply clipping to prevent rendering outside map bounds
            using (new SKAutoCanvasRestore(canvas))
            {
                ApplyViewTransform(canvas);

                // Clip to map bounds
                canvas.ClipRect(mapBounds);

                DrawTiles(canvas);
                DrawPaths(canvas);
                DrawMarkers(canvas);
            }
        }

        private PointF ScreenToWorld(Point screenPoint)
        {
            int vw = _mapControl.Width;
            int vh = _mapControl.Height;

            // Reverse transformation pipeline
            float worldX = (screenPoint.X - vw / 2f) / _mapProvider.Zoom;
            float worldY = -(screenPoint.Y - vh / 2f) / _mapProvider.Zoom;

            return new PointF(worldX, worldY);
        }

        private void Frm_waypoint_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private async void ToolStripButtonLoadSniff_Click(object sender, EventArgs e)
        {
            openFileDialog.Title = "Open File";
            openFileDialog.Filter = "Parsed Sniff File (*.txt)|*.txt";
            openFileDialog.FileName = "*.txt";
            openFileDialog.FilterIndex = 1;
            openFileDialog.ShowReadOnly = false;
            openFileDialog.Multiselect = true;
            openFileDialog.CheckFileExists = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] fileNames = openFileDialog.FileNames;
                toolStripStatusLabel.Text = "Loading Files...";
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    waypoints.Clear();

                    await Task.Run(() =>
                    {
                        foreach (string fileName in fileNames)
                        {
                            LoadSniffFileIntoDatatable(fileName);
                        }
                    });

                    toolStripTextBoxEntry.Enabled = true;
                    toolStripButtonSearch.Enabled = true;
                    toolStripStatusLabel.Text = $"{fileNames.Length} files selected for input.";

                    FillListBoxWithGuids("0");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading files: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private readonly List<PathData> _paths = new List<PathData>();
        private readonly List<MarkerData> _markers = new List<MarkerData>();

        private class PathData
        {
            public SKPath Path { get; set; }
            public SKPaint Paint { get; set; }
        }

        private class MarkerData
        {
            public SKPoint Position { get; set; }
            public SKPaint Paint { get; set; }
            public string Label { get; set; }
        }

        private void ApplyViewTransform(SKCanvas canvas)
        {
            int vw = _mapControl.Width;
            int vh = _mapControl.Height;

            canvas.Translate(vw / 2f, vh / 2f);  // Center viewport
            canvas.Scale(_mapProvider.Zoom);
            canvas.Scale(1, 1);
            canvas.Translate(-_mapProvider.Center.X, -_mapProvider.Center.Y);
        }

        private void DrawTiles(SKCanvas canvas)
        {
            if (!_mapProvider.IsMapLoaded) return;

            // Get visible area in world coordinates
            var visibleRect = GetVisibleWorldRect();

            foreach (var tile in _mapProvider.GetTiles())
            {
                var tileRect = _mapProvider.GetTileWorldRect(tile.X, tile.Y);

                // Only draw tiles that intersect with visible area
                if (tileRect.IntersectsWith(visibleRect))
                {
                    var bitmap = _mapProvider.LoadTile(tile);
                    if (bitmap != null)
                    {
                        canvas.DrawBitmap(bitmap, tileRect);
                    }
                }
            }
        }

        private SKRect GetVisibleWorldRect()
        {
            int vw = _mapControl.Width;
            int vh = _mapControl.Height;

            // Calculate visible area in world coordinates
            float halfWidth = (vw / 2f) / _mapProvider.Zoom;
            float halfHeight = (vh / 2f) / _mapProvider.Zoom;

            return new SKRect(
                _mapProvider.Center.X - halfWidth,
                _mapProvider.Center.Y - halfHeight,
                _mapProvider.Center.X + halfWidth,
                _mapProvider.Center.Y + halfHeight
            );
        }

        private void DrawPaths(SKCanvas canvas)
        {
            var pathPaint = new SKPaint
            {
                Color = new SKColor(
                    Properties.Settings.Default.LineColour.R,
                    Properties.Settings.Default.LineColour.G,
                    Properties.Settings.Default.LineColour.B
                ),
                StrokeWidth = 2,
                IsStroke = true,
                IsAntialias = true
            };

            foreach (var pathData in _paths)
            {
                pathData.Paint = pathPaint;
                canvas.DrawPath(pathData.Path, pathData.Paint);
            }
        }

        private void DrawMarkers(SKCanvas canvas)
        {
            if (_mapProvider.Zoom < 0.3f || _markers.Count == 0)
                return;

            var markerPaint = new SKPaint
            {
                Color = new SKColor(
                    Properties.Settings.Default.PointColour.R,
                    Properties.Settings.Default.PointColour.G,
                    Properties.Settings.Default.PointColour.B
                ),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            var linePaint = new SKPaint
            {
                Color = new SKColor(
                    Properties.Settings.Default.LineColour.R,
                    Properties.Settings.Default.LineColour.G,
                    Properties.Settings.Default.LineColour.B
                ),
                StrokeWidth = 2,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            var fontStyle = new SKFontStyle(
                SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright
            );

            var textFont = new SKFont(SKTypeface.FromFamilyName("Arial", fontStyle), 8);

            var textLinePaint = new SKPaint
            {
                Color = new SKColor(
                    Properties.Settings.Default.ConnectorLineColour.R,
                    Properties.Settings.Default.ConnectorLineColour.G,
                    Properties.Settings.Default.ConnectorLineColour.B
                ),
                StrokeWidth = 0.75f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                PathEffect = SKPathEffect.CreateDash(new float[] { 2f, 2f }, 0)
            };

            const float markerRadius = 2.5f;
            const float baseSpacing = 20f;
            const int maxLabelsToShow = 5;
            const float minAngleIncrement = 60f * (float)Math.PI / 180f;

            // Draw connecting lines between waypoints
            if (_markers.Count > 1)
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(_markers[0].Position);
                    for (int i = 1; i < _markers.Count; i++)
                    {
                        path.LineTo(_markers[i].Position);
                    }
                    canvas.DrawPath(path, linePaint);
                }
            }

            var positionGroups = _markers
                .Select((m, i) => new { Marker = m, Index = i + 1 })
                .GroupBy(x => new { X = (int)x.Marker.Position.X, Y = (int)x.Marker.Position.Y })
                .ToList();

            foreach (var group in positionGroups)
            {
                var items = group.ToList();
                var count = items.Count;
                var center = items[0].Marker.Position;

                // Draw markers
                foreach (var item in items)
                {
                    canvas.DrawCircle(center, markerRadius, markerPaint);
                }

                // Draw labels
                if (count == 1)
                {
                    DrawLabel(canvas, items[0].Index.ToString(), center, textFont, textLinePaint, baseSpacing);
                }
                else if (count <= maxLabelsToShow)
                {
                    float radius = baseSpacing * (1 + count * 0.15f);
                    float angleIncrement = Math.Max(minAngleIncrement, (2 * (float)Math.PI) / count);
                    float currentAngle = (float)Math.PI / 2;

                    for (int i = 0; i < count; i++)
                    {
                        var labelPos = new SKPoint(
                            center.X + radius * (float)Math.Cos(currentAngle),
                            center.Y - radius * (float)Math.Sin(currentAngle)
                        );

                        canvas.DrawLine(center.X, center.Y, labelPos.X, labelPos.Y, textLinePaint);
                        DrawTextWithShadow(canvas, items[i].Index.ToString(), labelPos, textFont);
                        currentAngle += angleIncrement;
                    }
                }
            }
        }

        private void DrawLabel(SKCanvas canvas, string label, SKPoint center,
                              SKFont textFont, SKPaint linePaint, float offset)
        {
            var labelPos = new SKPoint(center.X, center.Y - offset);
            canvas.DrawLine(center.X, center.Y, labelPos.X, labelPos.Y, linePaint);
            DrawTextWithShadow(canvas, label, labelPos, textFont);
        }

        private void DrawTextWithShadow(SKCanvas canvas, string text, SKPoint position, SKFont font)
        {
            var textColor = new SKColor(
                Properties.Settings.Default.TitleColour.R,
                Properties.Settings.Default.TitleColour.G,
                Properties.Settings.Default.TitleColour.B
            );

            using (var textPaint = new SKPaint { Color = textColor, IsAntialias = true })
            using (var shadowPaint = new SKPaint { Color = SKColors.Black.WithAlpha(0xAA), IsAntialias = true })
            {
                // Draw shadow with center alignment
                canvas.DrawText(text, position.X + 1, position.Y + 1, SKTextAlign.Center, font, shadowPaint);

                // Draw main text with center alignment
                canvas.DrawText(text, position.X, position.Y, SKTextAlign.Center, font, textPaint);
            }
        }

        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.Title = "Save File";
            saveFileDialog.Filter = "Path Insert SQL (*.sql)|*.sql";
            saveFileDialog.FileName = "";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.CheckFileExists = false;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (string line in txtOutput.Lines)
                            sw.Write(line + sw.NewLine);

                        sw.Close();
                        MessageBox.Show("SQL Pathing Inserts written to file " + saveFileDialog.FileName, "SQL Written to file", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void ToolStripButtonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = toolStripTextBoxEntry.Text?.Trim() ?? "";

                if (string.IsNullOrEmpty(searchTerm))
                {
                    // If search is empty, restore the original list
                    if (_originalListBoxItems.Count > 0)
                    {
                        listBox.DataSource = _originalListBoxItems;
                        listBox.Refresh();
                    }
                }
                else
                {
                    // Store the current list as original if this is the first search
                    if (_originalListBoxItems.Count == 0 && listBox.Items.Count > 0)
                    {
                        _originalListBoxItems = listBox.Items.Cast<string>().ToList();
                    }

                    // Search through the original list for entry or name matches
                    string searchTermLower = searchTerm.ToLowerInvariant();
                    var filteredItems = new List<string>();

                    foreach (string item in _originalListBoxItems)
                    {
                        // Extract entry and name parts (format: "Entry - Name (LowGuid)")
                        string[] parts = item.Split(new[] { " - ", " (" }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length >= 2)
                        {
                            string entry = parts[0].Trim();
                            string name = parts[1].Trim();

                            // Check if either entry OR name matches
                            if (entry.ToLowerInvariant() == searchTermLower ||
                                name.ToLowerInvariant().Contains(searchTermLower))
                            {
                                filteredItems.Add(item);
                            }
                        }
                    }

                    if (filteredItems.Count == 0)
                    {
                        MessageBox.Show($"No matches found for '{searchTerm}'",
                            "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Keep current items (the original list)
                        listBox.DataSource = _originalListBoxItems;
                        listBox.Refresh();
                    }
                    else
                    {
                        listBox.DataSource = filteredItems;
                        listBox.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during search: " + ex.Message,
                    "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ToolStripTextBoxEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ToolStripButtonSearch_Click(sender, e);
            }
        }

        private void ToolStripButtonSettings_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Form Frm_settings = new frm_Settings();
            Frm_settings.ShowDialog();
            PlotCurrentWaypoints();
        }

        private void ListBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Console.WriteLine("ListBox selection changed!");
            if (listBox.SelectedItem == null)
            {
                Console.WriteLine("No item selected");
                return;
            }

            string selectedText = listBox.SelectedItem.ToString();
            Console.WriteLine($"Selected: {selectedText}");

            string[] parts = selectedText.Split(new[] { " - ", " (" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 3)
            {
                creature_entry = parts[0];
                creature_name = parts[1];
                string displayedLowGuid = parts[2].TrimEnd(')');

                Console.WriteLine($"Parsed - Entry: {creature_entry}, Name: {creature_name}, LowGuid: {displayedLowGuid}");

                creature_guid = FindFullGuidByLowGuid(displayedLowGuid, creature_entry);
                Console.WriteLine($"Full GUID: {creature_guid}");
            }

            FillGrid();
            PlotCurrentWaypoints();
        }

        private string FindFullGuidByLowGuid(string lowGuid, string entry)
        {
            foreach (DataRow row in waypoints.Rows)
            {
                string rowEntry = row.Field<string>(0);
                string rowLowGuid = ExtractLowGuid(row.Field<string>(1));

                if (rowEntry == entry && rowLowGuid == lowGuid)
                {
                    return ExtractFullGuid(row.Field<string>(1));
                }
            }
            return lowGuid;
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySelection();
            DeleteSelection();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
            => CopySelection();

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
            => PasteSelection();

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
            => DeleteSelection();

        private void CreateSQLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string currentOutput = txtOutput.Text;

            if (Properties.Settings.Default.vmangos)
                currentOutput += GenerateSQL_vmangos();
            if (Properties.Settings.Default.trinitycore)
                currentOutput += GenerateSQL_trinitycore();
            if (Properties.Settings.Default.cmangos)
                currentOutput += GenerateSQL_cmangos();
            if (Properties.Settings.Default.CPP)
                currentOutput += GenerateCode_cpp();

            txtOutput.Text = currentOutput;
        }
        private void MakegoXyzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = gridWaypoint.SelectedRows[0];
            string go = ".go xyz " + row.Cells[1].Value + " " + row.Cells[2].Value + " " + row.Cells[3].Value;
            Clipboard.SetText(go);
        }

        private void PlotCurrentWaypoints()
        {
            if (gridWaypoint.RowCount == 0)
            {
                Console.WriteLine("[DEBUG] PlotCurrentWaypoints: grid is empty");
                return;
            }

            Console.WriteLine($"[DEBUG] PlotCurrentWaypoints: Current mapID={mapID}");

            if (!int.TryParse(mapID, out int mapId))
            {
                Console.WriteLine($"[WARN] Invalid mapID '{mapID}', using fallback map 0");
                mapId = 0;
            }

            _mapProvider.LoadMap(mapId);

            if (!_mapProvider.IsMapLoaded)
            {
                MessageBox.Show($"Map {mapId} could not be loaded", "Map Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Collect waypoints in game coordinates
            var gameWaypoints = new List<PointF>();
            for (int i = 0; i < gridWaypoint.RowCount; i++)
            {
                try
                {
                    float x = float.Parse(gridWaypoint[1, i].Value.ToString());
                    float y = float.Parse(gridWaypoint[2, i].Value.ToString());
                    gameWaypoints.Add(new PointF(x, y));
                    Console.WriteLine($"[DEBUG] Game waypoint {i}: X={x}, Y={y}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to parse waypoint {i}: {ex.Message}");
                }
            }

            LoadNpcPath(mapId, gameWaypoints);
        }

        public PointF TileToWorldCoords(int tileX, int tileY)
        {
            const float tileSize = 533.3333f;
            const float mapSize = 34133.33f;
            float worldX = (tileX * tileSize) - (mapSize / 2) + (tileSize / 2);
            float worldY = (tileY * tileSize) - (mapSize / 2) + (tileSize / 2);
            Console.WriteLine($"TileToWorldCoords: Input tile ({tileX},{tileY}) -> World coords ({worldX:F3},{worldY:F3})");
            return new PointF(worldX, worldY);
        }

        public void LoadNpcPath(int mapId, List<PointF> gameWaypoints)
        {
            try
            {
                _mapProvider.LoadMap(mapId);
                if (!_mapProvider.IsMapLoaded)
                {
                    MessageBox.Show($"Map {mapId} could not be loaded", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _paths.Clear();
                _markers.Clear();

                // Convert all waypoints to world coordinates
                var worldWaypoints = new List<PointF>();
                foreach (var p in gameWaypoints)
                {
                    var worldPoint = _mapProvider.GameToWorld(p);
                    worldWaypoints.Add(worldPoint);
                    Console.WriteLine($"Waypoint: Game({p.X:F1},{p.Y:F1}) -> World({worldPoint.X:F1},{worldPoint.Y:F1})");
                }

                // Build path in world coordinates
                var path = new SKPath();
                if (worldWaypoints.Count > 0)
                {
                    path.MoveTo(worldWaypoints[0].X, worldWaypoints[0].Y);
                    for (int i = 1; i < worldWaypoints.Count; i++)
                    {
                        path.LineTo(worldWaypoints[i].X, worldWaypoints[i].Y);
                    }
                }

                var pathPaint = new SKPaint
                {
                    Color = new SKColor(
                        Properties.Settings.Default.LineColour.R,
                        Properties.Settings.Default.LineColour.G,
                        Properties.Settings.Default.LineColour.B
                    ),
                    StrokeWidth = 2,
                    IsStroke = true,
                    IsAntialias = true
                };

                // Add markers in world coordinates
                for (int i = 0; i < worldWaypoints.Count; i++)
                {
                    _markers.Add(new MarkerData
                    {
                        Position = new SKPoint(worldWaypoints[i].X, worldWaypoints[i].Y),
                        Paint = new SKPaint
                        {
                            Color = SKColors.Red,
                            IsStroke = false,
                            IsAntialias = true
                        },
                        Label = (i + 1).ToString()
                    });
                }

                // Center view
                if (worldWaypoints.Count > 0)
                {
                    float centerX = worldWaypoints.Average(p => p.X);
                    float centerY = worldWaypoints.Average(p => p.Y);
                    _mapProvider.Center = new PointF(centerX, centerY);
                    Console.WriteLine($"Centering at: World({centerX:F1},{centerY:F1})");

                    // Calculate optimal zoom
                    if (worldWaypoints.Count > 1)
                    {
                        var minX = worldWaypoints.Min(p => p.X);
                        var maxX = worldWaypoints.Max(p => p.X);
                        var minY = worldWaypoints.Min(p => p.Y);
                        var maxY = worldWaypoints.Max(p => p.Y);

                        float spanX = (maxX - minX) * 1.2f;
                        float spanY = (maxY - minY) * 1.2f;

                        float zoomX = _mapControl.Width / spanX;
                        float zoomY = _mapControl.Height / spanY;

                        _mapProvider.Zoom = Math.Min(zoomX, zoomY) * 0.9f;
                        _mapProvider.Zoom = Math.Max(0.5f, Math.Min(_mapProvider.Zoom, 1.5f));
                        Console.WriteLine($"Calculated Zoom: {_mapProvider.Zoom:F2} (X:{zoomX:F2}, Y:{zoomY:F2})");
                    }
                    else
                    {
                        _mapProvider.Zoom = 1.0f;
                    }
                }

                _mapControl.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading NPC path: {ex}");
                MessageBox.Show($"Error loading path: {ex.Message}");
            }
        }

        public void LoadSniffFileIntoDatatable(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8, true, 8192))
            {
                string line;
                int lineNumber = 0;
                string filetype = null;
                string sniffversion = null;

                // Read header lines
                while ((line = reader.ReadLine()) != null && lineNumber < 3)
                {
                    if (lineNumber == 0) filetype = line;
                    else if (lineNumber == 2) sniffversion = line;
                    lineNumber++;
                }

                if (filetype == "# TrinityCore - WowPacketParser")
                {
                    Sniff_version();
                    ParseSniffData(reader);

                    this.Invoke((MethodInvoker)delegate {
                        this.Text = "Waypoint Creator - Movement data loaded from SMSG_ON_MONSTER_MOVE";
                    });
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate {
                        MessageBox.Show($"{fileName} is not a valid TrinityCore parsed sniff file.",
                            "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            }
        }

        private void ParseSniffData(StreamReader reader)
        {
            if (waypoints.Columns.Count == 0)
            {
                waypoints.Columns.AddRange(new[] { "entry", "guid", "x", "y", "z", "o", "time", "mapID" }
                    .Select(c => new DataColumn(c)).ToArray());
            }

            Packet sniff = new Packet { o = "NULL" };
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains(sniff_state))
                {
                    sniff = new Packet { o = "NULL" };
                    continue;
                }

                if (!string.IsNullOrEmpty(line) && line.Contains(sniff_move))
                {
                    string[] values = line.Split(' ');
                    sniff.time = values[move_time].Split('.')[0];

                    while ((line = reader.ReadLine()) != null && !string.IsNullOrEmpty(line))
                    {
                        string[] packetline = line.Split(' ');

                        // Extract creature info and mapID from MoverGUID line
                        if (line.Contains(sniff_move_1) && (line.Contains(sniff_move_2) || line.Contains(sniff_move_3)))
                        {
                            sniff.entry = packetline[move_entry];
                            sniff.guid = packetline[move_guid];

                            // Find and extract mapID from the "Map: X" part
                            for (int i = 0; i < packetline.Length; i++)
                            {
                                if (packetline[i] == "Map:")
                                {
                                    mapID = packetline[i + 1];
                                    Console.WriteLine($"Extracted mapID: {mapID}");
                                    break;
                                }
                            }
                        }
                        else if (line.Contains(sniff_move_4))
                        {
                            // Position coordinates
                            sniff.x = packetline[move_x];
                            sniff.y = packetline[move_y];
                            sniff.z = packetline[move_z];
                            sniff.o = "100";
                        }
                        else if (line.Contains(sniff_move_5))
                        {
                            // Movement points
                            sniff.x = packetline[move_pointx];
                            sniff.y = packetline[move_pointy];
                            sniff.z = packetline[move_pointz];
                        }
                        else if (line.Contains(sniff_move_6))
                        {
                            // End of movement data
                            sniff.entry = "";
                            break;
                        }
                        else if (line.Contains(sniff_move_7))
                        {
                            // Orientation
                            sniff.o = packetline[move_o];
                        }
                    }

                    if (!string.IsNullOrEmpty(sniff.entry))
                    {
                        waypoints.Rows.Add(sniff.entry, sniff.guid, sniff.x, sniff.y, sniff.z, sniff.o, sniff.time, mapID);
                        Console.WriteLine($"Added waypoint: Entry={sniff.entry}, Map={mapID}, X={sniff.x}, Y={sniff.y}");
                        sniff.entry = "";
                    }
                }
            }
        }

        private static readonly Dictionary<string, string> CreatureNameCache = new Dictionary<string, string>();

        public void FillListBoxWithGuids(string entry)
        {
            guids.Clear();
            guids = waypoints.DefaultView.ToTable(true, "guid", "entry");
            List<string> lst = new List<string>();

            if (Properties.Settings.Default.UsingDB)
            {
                var neededEntries = guids.AsEnumerable().Select(r => r["entry"].ToString()).Distinct();
                var missingEntries = neededEntries.Where(e => !CreatureNameCache.ContainsKey(e)).ToList();

                if (missingEntries.Count > 0)
                {
                    string sqltext = "SELECT `entry`, `name` FROM `creature_template` WHERE `entry` IN (" +
                                     string.Join(",", missingEntries) + ");";
                    DataSet DS = (DataSet)Module.database_conn(sqltext);

                    foreach (DataRow row in DS.Tables["table1"].Rows)
                    {
                        CreatureNameCache[row["entry"].ToString()] = row["name"].ToString();
                    }
                }
            }

            foreach (DataRow r in guids.Rows)
            {
                string creatureEntry = r["entry"].ToString();
                string fullGuid = ExtractFullGuid(r["guid"].ToString());
                string lowGuid = ExtractLowGuid(r["guid"].ToString());
                string creatureName = CreatureNameCache.TryGetValue(creatureEntry, out string name)
                                    ? name
                                    : "Unknown";

                if (entry != "0")
                {
                    if (entry == creatureEntry)
                    {
                        lst.Add($"{creatureEntry} - {creatureName} ({lowGuid})");
                    }
                }
                else if (!string.IsNullOrEmpty(r["guid"].ToString()))
                {
                    lst.Add($"{creatureEntry} - {creatureName} ({lowGuid})");
                }
            }

            lst.Sort();
            listBox.DataSource = lst;
            listBox.Refresh();

            // Store this as the original list for search functionality
            _originalListBoxItems = new List<string>(lst);
        }

        private string ExtractFullGuid(string fullGuid)
        {
            if (string.IsNullOrEmpty(fullGuid))
                return "0";

            // Handle full TrinityCore GUID format
            if (fullGuid.Contains("Full:") && fullGuid.Contains("Low:"))
            {
                var fullHexMatch = System.Text.RegularExpressions.Regex.Match(fullGuid, @"Full:\s*(0x[0-9A-F]+)");
                if (fullHexMatch.Success)
                {
                    return fullHexMatch.Groups[1].Value;
                }
            }

            if (fullGuid.StartsWith("0x"))
            {
                return fullGuid.Split(' ')[0];
            }

            return fullGuid;
        }

        private string ExtractLowGuid(string fullGuid)
        {
            if (string.IsNullOrEmpty(fullGuid))
                return "0";

            // Handle TrinityCore format with Low: marker
            if (fullGuid.Contains("Low:"))
            {
                var parts = fullGuid.Split(new[] { "Low:" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var lowPart = parts[1].Trim().Split(' ')[0];
                    return lowPart;
                }
            }

            // Handle hex GUID (0x...) - extract last 8 digits as low GUID
            if (fullGuid.StartsWith("0x"))
            {
                string hex = fullGuid.Substring(2);
                if (hex.Length >= 8)
                {
                    return long.Parse(hex.Substring(hex.Length - 8),
                           System.Globalization.NumberStyles.HexNumber).ToString();
                }
                return long.Parse(hex, System.Globalization.NumberStyles.HexNumber).ToString();
            }

            var digits = new string(fullGuid.Where(c => char.IsDigit(c)).ToArray());
            return digits.Length > 0 ? digits : "0";
        }

        public void FillGrid()
        {
            Console.WriteLine("\nFilling grid...");
            Console.WriteLine($"Current creature_guid: {creature_guid}");

            movePackets = waypoints.Clone();
            int matches = 0;

            foreach (DataRow row in waypoints.Rows)
            {
                string rowFullGuid = ExtractFullGuid(row.Field<string>(1));
                if (rowFullGuid == creature_guid)
                {
                    movePackets.ImportRow(row);
                    creature_entry = row.Field<string>(0);
                    matches++;
                }
            }

            Console.WriteLine($"Found {matches} matching waypoints");

            if (movePackets.Rows.Count > 0)
            {
                // Get mapID from the first waypoint
                mapID = movePackets.Rows[0].Field<string>(7);
                Console.WriteLine($"Using mapID {mapID} from waypoint data");

                gridWaypoint.Rows.Clear();

                for (var l = 0; l < movePackets.Rows.Count; l++)
                {
                    gridWaypoint.Rows.Add(
                        l + 1,
                        movePackets.Rows[l].Field<string>(2), // x
                        movePackets.Rows[l].Field<string>(3), // y
                        movePackets.Rows[l].Field<string>(4), // z
                        movePackets.Rows[l].Field<string>(5), // o
                        movePackets.Rows[l].Field<string>(6), // time
                        "" // delay
                    );
                }
            }

            Findrange();
            PlotCurrentWaypoints();
        }

        private void CopySelection()
        {
            if (gridWaypoint.SelectedRows.Count == 0)
                return;

            var sb = new StringBuilder();

            sb.AppendLine("Point,X,Y,Z,Orientation,Time,Delay");

            var sortedRows = gridWaypoint.SelectedRows.Cast<DataGridViewRow>()
                .OrderBy(row => Convert.ToInt32(row.Cells[0].Value ?? "0"))
                .ToList();

            foreach (DataGridViewRow row in sortedRows)
            {
                sb.AppendLine(string.Join(",",
                    (row.Cells[0].Value ?? "").ToString(),
                    (row.Cells[1].Value ?? "").ToString(),
                    (row.Cells[2].Value ?? "").ToString(),
                    (row.Cells[3].Value ?? "").ToString(),
                    (row.Cells[4].Value ?? "").ToString(),
                    (row.Cells[5].Value ?? "").ToString(),
                    (row.Cells[6].Value ?? "").ToString()
                ));
            }

            _clip.Clear();

            // Store in both CSV format and internal format
            DataObject data = new DataObject();
            data.SetData(DataFormats.CommaSeparatedValue, sb.ToString());
            data.SetData(DataFormats.Text, sb.ToString());

            foreach (DataGridViewRow row in sortedRows)
            {
                _clip.Add(new[]
                {
            row.Cells[1].Value?.ToString(),
            row.Cells[2].Value?.ToString(),
            row.Cells[3].Value?.ToString(),
            row.Cells[4].Value?.ToString(),
            row.Cells[5].Value?.ToString(),
            row.Cells[6].Value?.ToString()
        });
            }
            data.SetData("WaypointCreatorInternalFormat", _clip);

            Clipboard.SetDataObject(data, true);
        }

        private void PasteSelection()
        {
            _clip.Clear();

            if (Clipboard.ContainsData("WaypointCreatorInternalFormat"))
            {
                _clip = (List<string[]>)Clipboard.GetData("WaypointCreatorInternalFormat");
            }
            else if (Clipboard.ContainsText())
            {
                string[] lines = Clipboard.GetText().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return;

                bool hasHeader = false;

                // Check for header
                foreach (string raw in lines)
                {
                    string[] v = raw.Split(',');
                    if (v.Length >= 4 &&
                        v[0].Equals("Point", StringComparison.OrdinalIgnoreCase) &&
                        v[1].Equals("X", StringComparison.OrdinalIgnoreCase) &&
                        v[2].Equals("Y", StringComparison.OrdinalIgnoreCase) &&
                        v[3].Equals("Z", StringComparison.OrdinalIgnoreCase))
                    {
                        hasHeader = true;
                        break;
                    }
                }

                // If no header found, skip processing
                if (!hasHeader) return;

                // Process data rows
                foreach (string raw in lines)
                {
                    string[] v = raw.Split(',');

                    // Skip header lines
                    if (v.Length >= 4 &&
                        v[0].Equals("Point", StringComparison.OrdinalIgnoreCase) &&
                        v[1].Equals("X", StringComparison.OrdinalIgnoreCase) &&
                        v[2].Equals("Y", StringComparison.OrdinalIgnoreCase) &&
                        v[3].Equals("Z", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Process data rows: require Point,X,Y,Z and valid numbers for X,Y,Z
                    if (v.Length >= 4 &&
                        int.TryParse(v[0], out _) &&
                        double.TryParse(v[1], out _) &&
                        double.TryParse(v[2], out _) &&
                        double.TryParse(v[3], out _))
                    {
                        _clip.Add(new[]
                        {
                    v[1], // X
                    v[2], // Y
                    v[3], // Z
                    v.Length > 4 ? v[4] : "100", // Orientation
                    v.Length > 5 ? v[5] : "0", // Time
                    v.Length > 6 ? v[6] : "0"  // Delay
                });
                    }
                }
            }

            if (_clip.Count == 0) return;

            // Insert at the selected row's index (below it) or at the end if no selection
            int insertIndex = gridWaypoint.SelectedRows.Count > 0 ? gridWaypoint.SelectedRows[0].Index + 1 : gridWaypoint.Rows.Count;

            foreach (var wp in _clip)
            {
                gridWaypoint.Rows.Insert(insertIndex, new object[] { null, wp[0], wp[1], wp[2], wp[3], wp[4], wp[5] });
                // Highlight the new row in green
                gridWaypoint.Rows[insertIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                insertIndex++;
            }

            Renumber();
            PlotCurrentWaypoints();
            Findrange();
        }

        private void DeleteSelection()
        {
            foreach (DataGridViewRow r in gridWaypoint.SelectedRows)
                gridWaypoint.Rows.Remove(r);
            Renumber(); PlotCurrentWaypoints(); Findrange();
        }

        private void Renumber()
        {
            for (int i = 0; i < gridWaypoint.Rows.Count; i++)
                gridWaypoint[0, i].Value = i + 1;
        }

        private string GenerateSQL_vmangos()
        {
            string lowGuid = ExtractLowGuid(creature_guid);
            var sb = new StringBuilder();

            sb.AppendLine($"-- Pathing for {creature_name} Entry: {creature_entry}");
            sb.AppendLine("SET @NPC := XXXXXX;");
            sb.AppendLine($"UPDATE `creature` SET `wander_distance`=0, `movement_type`=2, `position_x`={gridWaypoint[1, 0].Value}, `position_y`={gridWaypoint[2, 0].Value}, `position_z`={gridWaypoint[3, 0].Value} WHERE `guid`=@NPC;");
            sb.AppendLine("DELETE FROM `creature_movement` WHERE `id`=@NPC;");
            sb.AppendLine("INSERT INTO `creature_movement` (`id`,`point`,`position_x`,`position_y`,`position_z`,`orientation`,`waittime`,`wander_distance`,`script_id`) VALUES");

            for (int i = 0; i < gridWaypoint.RowCount; i++)
            {
                string facing = gridWaypoint[4, i].Value?.ToString() ?? "100";
                string waittime = string.IsNullOrEmpty(gridWaypoint[6, i].Value?.ToString()) ? "0" : gridWaypoint[6, i].Value.ToString();
                sb.Append($"(@NPC, {i + 1}, {gridWaypoint[1, i].Value}, {gridWaypoint[2, i].Value}, {gridWaypoint[3, i].Value}, {facing}, {waittime}, 0, 0)");
                sb.AppendLine(i < gridWaypoint.RowCount - 1 ? "," : ";");
            }

            sb.AppendLine($"-- Full: {creature_guid} | Low: {lowGuid}");
            sb.AppendLine($"-- .go xyz {gridWaypoint[1, 0].Value} {gridWaypoint[2, 0].Value} {gridWaypoint[3, 0].Value}\r\n");

            return sb.ToString();
        }

        private string GenerateSQL_cmangos()
        {
            string lowGuid = ExtractLowGuid(creature_guid);
            var sb = new StringBuilder();

            sb.AppendLine($"-- Pathing for {creature_name} Entry: {creature_entry}");
            sb.AppendLine("SET @NPC := XXXXXX;");
            sb.AppendLine($"UPDATE `creature` SET `MovementType`=2, `spawndist`=0, `position_x`={gridWaypoint[1, 0].Value}, `position_y`={gridWaypoint[2, 0].Value}, `position_z`={gridWaypoint[3, 0].Value}, `orientation`={gridWaypoint[4, 0].Value ?? "100"} WHERE `guid`=@NPC;");
            sb.AppendLine("DELETE FROM `creature_movement` WHERE `Id`=@NPC;");
            sb.AppendLine("INSERT INTO `creature_movement` (`Id`, `Point`, `PositionX`, `PositionY`, `PositionZ`, `Orientation`, `WaitTime`, `ScriptId`) VALUES");

            for (int i = 0; i < gridWaypoint.RowCount; i++)
            {
                string facing = gridWaypoint[4, i].Value?.ToString() ?? "100";
                string waittime = string.IsNullOrEmpty(gridWaypoint[6, i].Value?.ToString()) ? "0" : gridWaypoint[6, i].Value.ToString();
                sb.Append($"(@NPC, {i + 1}, {gridWaypoint[1, i].Value}, {gridWaypoint[2, i].Value}, {gridWaypoint[3, i].Value}, {facing}, {waittime}, 0)");
                sb.AppendLine(i < gridWaypoint.RowCount - 1 ? "," : ";");
            }

            sb.AppendLine($"-- Full: {creature_guid} | Low: {lowGuid}");
            sb.AppendLine($"-- .go xyz {gridWaypoint[1, 0].Value} {gridWaypoint[2, 0].Value} {gridWaypoint[3, 0].Value}\r\n");

            return sb.ToString();
        }

        private string GenerateSQL_trinitycore()
        {
            string lowGuid = ExtractLowGuid(creature_guid);
            var sb = new StringBuilder();

            sb.AppendLine($"-- {creature_name} (Entry: {creature_entry})");
            sb.AppendLine($"SET @ENTRY := {creature_entry};");
            sb.AppendLine($"SET @PATH := @ENTRY * 100;");
            sb.AppendLine($"DELETE FROM `waypoint_path` WHERE `PathId` = @PATH;");
            sb.AppendLine($"INSERT INTO `waypoint_path` VALUES (@PATH, 1, 0, '{creature_name.Replace("'", "''")}');");
            sb.AppendLine();
            sb.AppendLine($"DELETE FROM `waypoint_path_node` WHERE `PathId` = @PATH;");
            sb.AppendLine($"INSERT INTO `waypoint_path_node` VALUES");

            for (int i = 0; i < gridWaypoint.RowCount; i++)
            {
                string facing = gridWaypoint[4, i].Value?.ToString() ?? "NULL";
                string waittime = string.IsNullOrEmpty(gridWaypoint[6, i].Value?.ToString()) ? "0" : gridWaypoint[6, i].Value.ToString();
                sb.Append($"(@PATH, {i}, {gridWaypoint[1, i].Value}, {gridWaypoint[2, i].Value}, {gridWaypoint[3, i].Value}, {facing}, {waittime})");
                sb.AppendLine(i < gridWaypoint.RowCount - 1 ? "," : ";");
            }

            sb.AppendLine();
            sb.AppendLine($"-- Full: {creature_guid} | Low: {lowGuid}");
            sb.AppendLine($"-- .go xyz {gridWaypoint[1, 0].Value} {gridWaypoint[2, 0].Value} {gridWaypoint[3, 0].Value}\r\n");

            return sb.ToString();
        }

        private string GenerateCode_cpp()
        {
            string code = $"// Position Constant for {creature_name} (Entry: {creature_entry})\r\n" +
                                 $"Position const {creature_name.Replace(" ", "")}Waypoints[] =\r\n" +
                                 "{\r\n";

            for (int i = 0; i < gridWaypoint.RowCount; i++)
            {
                string x = gridWaypoint[1, i].Value?.ToString() ?? "0.0f";
                string y = gridWaypoint[2, i].Value?.ToString() ?? "0.0f";
                string z = gridWaypoint[3, i].Value?.ToString() ?? "0.0f";
                string o = gridWaypoint[4, i].Value?.ToString() ?? "0.0f";

                if (!x.EndsWith("f")) x += "f";
                if (!y.EndsWith("f")) y += "f";
                if (!z.EndsWith("f")) z += "f";
                if (!o.EndsWith("f")) o += "f";

                string Codeline = $"    {{ {x}, {y}, {z}, {o} }}";

                if (i < gridWaypoint.RowCount - 1)
                    Codeline += ",";

                Codeline += $"  // Point {i + 1}\r\n";
                code += Codeline;
            }

            code += "};\r\n\r\n" +
                   $"// .go xyz {gridWaypoint[1, 0].Value} {gridWaypoint[2, 0].Value} {gridWaypoint[3, 0].Value}\r\n";

            return code;
        }

        private void Sniff_version()
        {
            state_map = 1;
            move_time = 9;
            move_entry = 8;
            move_guid = 2;
            move_x = 2;
            move_y = 4;
            move_z = 6;
            move_o = 3;
            move_pointx = 5;
            move_pointy = 7;
            move_pointz = 9;
            sniff_state = "SMSG_INIT_WORLD_STATES";
            sniff_move = "SMSG_ON_MONSTER_MOVE";
            sniff_move_1 = "MoverGUID: Full:";
            sniff_move_2 = "Creature/0";
            sniff_move_3 = "Vehicle/0";
            sniff_move_4 = "Position: X:";
            sniff_move_5 = " Points: X:";
            sniff_move_6 = "Face: 2";
            sniff_move_7 = "FaceDirection:";
        }

        private void Findrange()
        {
            float wanderRange = 0.0f;
            int rowCount = gridWaypoint.RowCount;

            var points = new (float x, float y, float z)[rowCount];
            for (int i = 0; i < rowCount; i++)
                points[i] = (float.Parse(gridWaypoint[1, i].Value.ToString()),
                             float.Parse(gridWaypoint[2, i].Value.ToString()),
                             float.Parse(gridWaypoint[3, i].Value.ToString()));

            for (int i = 0; i < rowCount - 1; i++)
            {
                for (int j = i + 1; j < rowCount; j++)
                {
                    float dx = points[j].x - points[i].x;
                    float dy = points[j].y - points[i].y;
                    float dz = points[j].z - points[i].z;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (distance > wanderRange) wanderRange = distance;

                    midX = (points[j].x + points[i].x) / 2;
                    midY = (points[j].y + points[i].y) / 2;
                }
            }

            toolStripLabelRange.Text = "Wander Range: " + (wanderRange / 2).ToString();
        }

        private void MakegoMidpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mid = ".go xy " + midX.ToString() + " " + midY.ToString();
            Clipboard.SetText(mid);
        }
    }
}
