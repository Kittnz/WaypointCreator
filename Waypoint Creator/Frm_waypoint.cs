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
using System.Windows.Forms.DataVisualization.Charting;

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
        }

        private void Frm_waypoint_Load(object sender, EventArgs e)
        {
            chart.BackColor = Properties.Settings.Default.BackColour;
            chart.ChartAreas[0].BackColor = Properties.Settings.Default.BackColour;
        }

        private void Frm_waypoint_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private async void ToolStripButtonLoadSniff_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                toolStripStatusLabel.Text = "Loading File...";
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    await Task.Run(() => LoadSniffFileIntoDatatable(fileName));
                    toolStripTextBoxEntry.Enabled = true;
                    toolStripButtonSearch.Enabled = true;
                    toolStripStatusLabel.Text = fileName + " is selected for input.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
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
                // This code runs if the dialog was cancelled
                return;
            }
        }

        private void ToolStripButtonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string temp = "0";
                if (toolStripTextBoxEntry.Text == "" || toolStripTextBoxEntry.Text == null)
                {
                    FillListBoxWithGuids(temp);
                }
                else
                {
                    temp = toolStripTextBoxEntry.Text;
                    FillListBoxWithGuids(temp);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Please provide number or leave blank.", "Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
        }

        private void ToolStripButtonSettings_Click(object sender, EventArgs e)
        {
            // Show Settings dialog
            System.Windows.Forms.Form Frm_settings = new frm_Settings();
            Frm_settings.ShowDialog();
            GraphPath();
        }

        private void ListBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listBox.SelectedItem == null) return;

            string selectedText = listBox.SelectedItem.ToString();
            string[] parts = selectedText.Split(new[] { " - ", " (" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 3)
            {
                creature_entry = parts[0];
                creature_name = parts[1];

                // Find the full GUID that corresponds to this low GUID display
                string displayedLowGuid = parts[2].TrimEnd(')');
                creature_guid = FindFullGuidByLowGuid(displayedLowGuid, creature_entry);
            }

            FillGrid();
            GraphPath();
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
            return lowGuid; // Fallback
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Cut selected fields from grid.
            CopyCutFromGrid(true);
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Copy selected fields from grid.
            CopyCutFromGrid(false);
        }

        private void PasteAboveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Paste cut or copied data into grid above selected row.
            PasteToGrid(true);
        }

        private void PasteBelowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Paste cut or copied data into grid below selected row.
            PasteToGrid(false);
        }

        private void CreateSQLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Store the current output
            string currentOutput = txtOutput.Text;

            // Generate new SQL and append it
            if (Properties.Settings.Default.vmangos)
                currentOutput += GenerateSQL_vmangos();
            if (Properties.Settings.Default.trinitycore)
                currentOutput += GenerateSQL_trinitycore();
            if (Properties.Settings.Default.cmangos)
                currentOutput += GenerateSQL_cmangos();
            if (Properties.Settings.Default.CPP)
                currentOutput += GenerateCode_cpp();

            // Update the output
            txtOutput.Text = currentOutput;
        }
        private void MakegoXyzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = gridWaypoint.SelectedRows[0];
            string go = ".go xyz " + row.Cells[1].Value + " " + row.Cells[2].Value + " " + row.Cells[3].Value;
            Clipboard.SetText(go);
        }

        private void CopyCutFromGrid(bool cut)
        {
            // Copy selected fields from grid and cut if cut is true.
            copiedRows.Tables.Clear();
            copiedRows.Tables.Add();
            copiedRows.Tables[0].Columns.AddRange(new DataColumn[6] { new DataColumn("x", typeof(string)), new DataColumn("y", typeof(string)), new DataColumn("z", typeof(string)), new DataColumn("o", typeof(string)), new DataColumn("time", typeof(string)), new DataColumn("delay", typeof(string)) });

            foreach (DataGridViewRow row in gridWaypoint.SelectedRows)
            {
                copiedRows.Tables[0].Rows.Add(row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value, row.Cells[6].Value);
                if (cut)
                    gridWaypoint.Rows.Remove(row);
            }

            if (cut)
            {
                for (var l = 0; l < gridWaypoint.Rows.Count; l++)
                    gridWaypoint[0, l].Value = l + 1;

                GraphPath();
                Findrange();
            }
        }

        private void PasteToGrid(bool above)
        {
            // Paste copiedRows into table
            pasteTable.Tables.Clear();
            pasteTable.Tables.Add();
            pasteTable.Tables[0].Columns.AddRange(new DataColumn[6] {new DataColumn("x", typeof(string)), new DataColumn("y", typeof(string)),
                            new DataColumn("z", typeof(string)), new DataColumn("o",typeof(string)), new DataColumn("time",typeof(string)), new DataColumn("delay",typeof(string)) });

            int selected = gridWaypoint.SelectedRows[0].Index;

            // If the selected row is not the first row copy all rows above it to pasteTable
            if (selected > 0)
            {
                for (var l = 0; l < selected; l++)
                {
                    pasteTable.Tables[0].Rows.Add(gridWaypoint[1, l].Value, gridWaypoint[2, l].Value, gridWaypoint[3, l].Value, gridWaypoint[4, l].Value, gridWaypoint[5, l].Value, gridWaypoint[6, l].Value);
                }

            }

            // If pasting below selected row, add selected row to pasteTable before copiedRows
            if (!above)
                pasteTable.Tables[0].Rows.Add(gridWaypoint[1, selected].Value, gridWaypoint[2, selected].Value, gridWaypoint[3, selected].Value, gridWaypoint[4, selected].Value, gridWaypoint[5, selected].Value, gridWaypoint[6, selected].Value);

            // Add copiedRows
            for (var l = copiedRows.Tables[0].Rows.Count - 1; l > -1; l--)
            {
                pasteTable.Tables[0].Rows.Add(copiedRows.Tables[0].Rows[l].Field<string>(0), copiedRows.Tables[0].Rows[l].Field<string>(1), copiedRows.Tables[0].Rows[l].Field<string>(2), copiedRows.Tables[0].Rows[l].Field<string>(3), copiedRows.Tables[0].Rows[l].Field<string>(4), copiedRows.Tables[0].Rows[l].Field<string>(5));
            }

            // If pasting above selected row, add selected row to pasteTable after copiedRows
            if (above)
                pasteTable.Tables[0].Rows.Add(gridWaypoint[1, selected].Value, gridWaypoint[2, selected].Value, gridWaypoint[3, selected].Value, gridWaypoint[4, selected].Value, gridWaypoint[5, selected].Value, gridWaypoint[6, selected].Value);

            // Add all rows below selected row
            if (selected < gridWaypoint.Rows.Count - 1)
            {
                for (var l = selected + 1; l < gridWaypoint.Rows.Count; l++)
                {
                    pasteTable.Tables[0].Rows.Add(gridWaypoint[1, l].Value, gridWaypoint[2, l].Value, gridWaypoint[3, l].Value, gridWaypoint[4, l].Value, gridWaypoint[5, l].Value, gridWaypoint[6, l].Value);
                }

            }

            // All data now in pasteTable. Now to replace grid contents with pasteTable.
            gridWaypoint.Rows.Clear();

            for (var l = 0; l < pasteTable.Tables[0].Rows.Count; l++)
                gridWaypoint.Rows.Add(l + 1, pasteTable.Tables[0].Rows[l].Field<string>(0), pasteTable.Tables[0].Rows[l].Field<string>(1), pasteTable.Tables[0].Rows[l].Field<string>(2), pasteTable.Tables[0].Rows[l].Field<string>(3), pasteTable.Tables[0].Rows[l].Field<string>(4), pasteTable.Tables[0].Rows[l].Field<string>(5));

            GraphPath();
            Findrange();
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
                    Sniff_version_6();
                    waypoints.Clear();
                    waypoints = ParseSniffData(reader); // Process remaining data with the same reader
                    this.Text = "Waypoint Creator - Movement data loaded from SMSG_ON_MONSTER_MOVE";
                }
                else
                {
                    MessageBox.Show($"{fileName} is not a valid TrinityCore parsed sniff file.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private DataTable ParseSniffData(StreamReader reader)
        {
            DataTable dt = new DataTable("Waypoints");
            dt.Columns.AddRange(new[] { "entry", "guid", "x", "y", "z", "o", "time", "mapID" }
                .Select(c => new DataColumn(c)).ToArray());

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
                        if (line.Contains(sniff_move_1) && (line.Contains(sniff_move_2) || line.Contains(sniff_move_3)))
                        {
                            sniff.entry = packetline[move_entry];
                            sniff.guid = packetline[move_guid];
                        }
                        else if (line.Contains(sniff_move_4))
                        {
                            sniff.x = packetline[move_x];
                            sniff.y = packetline[move_y];
                            sniff.z = packetline[move_z];
                            sniff.o = "100";
                        }
                        else if (line.Contains(sniff_move_5))
                        {
                            sniff.x = packetline[move_pointx];
                            sniff.y = packetline[move_pointy];
                            sniff.z = packetline[move_pointz];
                        }
                        else if (line.Contains(sniff_move_6))
                        {
                            sniff.entry = "";
                            break;
                        }
                        else if (line.Contains(sniff_move_7))
                        {
                            sniff.o = packetline[move_o];
                        }
                    }

                    if (!string.IsNullOrEmpty(sniff.entry))
                    {
                        dt.Rows.Add(sniff.entry, sniff.guid, sniff.x, sniff.y, sniff.z, sniff.o, sniff.time, mapID);
                        sniff.entry = "";
                    }
                }
            }

            return dt;
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
                    return fullHexMatch.Groups[1].Value; // Return full hex GUID
                }
            }

            // Handle hex GUID (0x...)
            if (fullGuid.StartsWith("0x"))
            {
                return fullGuid.Split(' ')[0]; // Return just the hex value
            }

            // Fallback - return raw string
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
                    return lowPart; // Return just the low GUID number
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

            // Fallback - return all digits
            var digits = new string(fullGuid.Where(c => char.IsDigit(c)).ToArray());
            return digits.Length > 0 ? digits : "0";
        }

        public void FillGrid()
        {
            movePackets = waypoints.Clone();

            foreach (DataRow row in waypoints.Rows)
            {
                string rowFullGuid = ExtractFullGuid(row.Field<string>(1));
                if (rowFullGuid == creature_guid)
                {
                    movePackets.ImportRow(row);
                    creature_entry = row.Field<string>(0); // Ensure correct entry
                }
            }

            if (movePackets.Rows.Count > 0)
            {
                creature_entry = movePackets.Rows[0].Field<string>(0);

                gridWaypoint.Rows.Clear();

                for (var l = 0; l < movePackets.Rows.Count; l++)
                    gridWaypoint.Rows.Add(l + 1, movePackets.Rows[l].Field<string>(2),
                                         movePackets.Rows[l].Field<string>(3),
                                         movePackets.Rows[l].Field<string>(4),
                                         movePackets.Rows[l].Field<string>(5),
                                         movePackets.Rows[l].Field<string>(6), "");
            }

            Findrange();
        }

        public void GraphPath()
        {
            // Clear and set up chart basics
            chart.BackColor = Properties.Settings.Default.BackColour;
            chart.ChartAreas[0].BackColor = Properties.Settings.Default.BackColour;
            chart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chart.ChartAreas[0].AxisY.ScaleView.ZoomReset();
            chart.ChartAreas[0].AxisY.IsReversed = true;
            chart.Titles.Clear();
            chart.Series.Clear();

            // Check if we have a valid selection
            if (listBox.SelectedItem == null || gridWaypoint.RowCount == 0)
            {
                return;
            }

            // Extract creature info from the selected list item
            string selectedText = listBox.SelectedItem.ToString();
            string displayName = "Entry " + creature_entry;
            string creatureName = "Unknown";

            // Parse the formatted string "Entry - Name (GUID)"
            if (selectedText.Contains(" - ") && selectedText.Contains(" ("))
            {
                try
                {
                    string[] parts = selectedText.Split(new[] { " - ", " (" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        creatureName = parts[1];
                        displayName = $"{creatureName} (Entry: {creature_entry})";
                    }
                }
                catch
                {
                    // Fallback to basic display if parsing fails
                    displayName = $"Entry {creature_entry}";
                }
            }

            // Set chart title
            Title title = chart.Titles.Add(displayName);
            title.Font = new Font("Arial", 16, FontStyle.Bold);
            title.ForeColor = Properties.Settings.Default.TitleColour;

            chart.Series.Clear();
            chart.Series.Add("Path");
            chart.Series["Path"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;

            if (Properties.Settings.Default.Lines == true)
            {
                chart.Series.Add("Line");

                if (Properties.Settings.Default.Splines == true)
                {
                    chart.Series["Line"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                }
                else
                {
                    chart.Series["Line"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                }
            }

            // Add data points to the chart
            for (var l = 0; l < gridWaypoint.RowCount; l++)
            {
                Double xpos = Convert.ToDouble(gridWaypoint[1, l].Value);
                Double ypos = Convert.ToDouble(gridWaypoint[2, l].Value);

                chart.Series["Path"].Points.AddXY(xpos, ypos);
                chart.Series["Path"].Points[l].Color = Properties.Settings.Default.PointColour;
                chart.Series["Path"].Points[l].Label = Convert.ToString(l + 1);

                if (Properties.Settings.Default.Lines == true)
                {
                    chart.Series["Line"].Points.AddXY(xpos, ypos);
                    chart.Series["Line"].Points[l].Color = Properties.Settings.Default.LineColour;
                }
            }
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

                // Ensure values end with 'f' for float literals
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

        private void Sniff_version_6()
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
