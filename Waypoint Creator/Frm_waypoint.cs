using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Frm_waypoint
{
    public partial class Frm_Waypoint : Form
    {
        static DataTable waypoints   = new DataTable();
        static DataTable guids       = new DataTable();
        static DataTable movePackets = new DataTable();
        static DataSet copiedRows = new DataSet();
        static DataSet pasteTable = new DataSet();


        string creature_guid  = "";
        string creature_entry = "";
        string creature_name  = "";
        string SQLtext        = "";
        string mapID          = "";

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
        public int object_time;
        public int object_entry;
        public int object_guid;
        public int object_pointx;
        public int object_pointy;
        public int object_pointz;
        public string sniff_state;
        public string sniff_move;
        public string sniff_move_1;
        public string sniff_move_2;
        public string sniff_move_3;
        public string sniff_move_4;
        public string sniff_move_5;
        public string sniff_move_6;
        public string sniff_move_7;
        public string sniff_object;
        public string sniff_object_1;
        public string sniff_object_2;
        public string sniff_object_3;
        public string sniff_object_4;

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

        private void ToolStripButtonLoadSniff_Click(object sender, EventArgs e)
        {
            openFileDialog.Title           = "Open File";
            openFileDialog.Filter          = "Parsed Sniff File (*.txt)|*.txt";
            openFileDialog.FileName        = "*.txt";
            openFileDialog.FilterIndex     = 1;
            openFileDialog.ShowReadOnly    = false;
            openFileDialog.Multiselect     = false;
            openFileDialog.CheckFileExists = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadSniffFileIntoDatatable(openFileDialog.FileName);
                toolStripTextBoxEntry.Enabled = true;
                toolStripButtonSearch.Enabled = true;
                toolStripStatusLabel.Text = openFileDialog.FileName + " is selected for input.";
            }
            else
            {
                // This code runs if the dialog was cancelled
                return;
            }
        }

        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.Title           = "Save File";
            saveFileDialog.Filter          = "Path Insert SQL (*.sql)|*.sql";
            saveFileDialog.FileName        = "";
            saveFileDialog.FilterIndex     = 1;
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
                if (toolStripTextBoxEntry.Text == "" || toolStripTextBoxEntry.Text == null )
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
            // On guid select fill grid and graph.
            if ((string)listBox.SelectedItem != "" && (string)listBox.SelectedItem != null)
            {
                FillGrid();
                GraphPath();
            }
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
            if (Properties.Settings.Default.DB)
                CreateSQL_DB();
            if (Properties.Settings.Default.UDB)
                CreateSQL_UDB();
            if (Properties.Settings.Default.SAI)
                CreateSQL_SAI();
            if (Properties.Settings.Default.vmangos)
                CreateSQL_vmangos();
            if (Properties.Settings.Default.CPP)
                CreateCode_cpp();
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
            // Initialize variables
            string filetype = null;
            string sniffversion = null;

            using (StreamReader file = new StreamReader(fileName))
            {
                for (int i = 0; i < 3; i++)
                {
                    string line = file.ReadLine();

                    if (line == null)
                    {
                        // Handle unexpected end of file
                        MessageBox.Show("Unexpected end of file.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        return;
                    }

                    if (i == 0)
                        filetype = line;
                    else if (i == 2)
                        sniffversion = line;
                }
            }

            if (filetype == "# TrinityCore - WowPacketParser")
            {
                // Determine sniff version
                if (sniffversion.Contains("V4") || sniffversion.Contains("V5"))
                    Sniff_version_4();
                else if (sniffversion.Contains("V1") || sniffversion.Contains("V2") || sniffversion.Contains("V3") || sniffversion.Contains("V6") || sniffversion.Contains("V7") || sniffversion.Contains("V9"))
                    Sniff_version_6();

                // Process new sniff file
                waypoints.Clear();
                waypoints = GetDataSourceFromSniffFile(fileName);

                if (Properties.Settings.Default.ObjectUpdate)
                    this.Text = "Waypoint Creator - Movement data loaded from SMSG_UPDATE_OBJECT";
                else
                    this.Text = "Waypoint Creator - Movement data loaded from SMSG_ON_MONSTER_MOVE";
            }
            else
            {
                MessageBox.Show(saveFileDialog.FileName + " is not a valid TrinityCore parsed sniff file.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public DataTable GetDataSourceFromSniffFile(string fileName)
        {
            // Clear old sniff displays
            toolStripStatusLabel.Text = "Loading File...";
            listBox.DataSource = null;
            listBox.Refresh();
            gridWaypoint.Rows.Clear();
            chart.Titles.Clear();
            chart.Series.Clear();

            // Set cursor as hourglass
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            DataTable dt = new DataTable("Waypoints");

            Packet sniff;

            sniff.entry = "";
            sniff.guid = "";
            sniff.x = "";
            sniff.y = "";
            sniff.z = "";
            sniff.o = "NULL";
            sniff.time = "";

            string[] columns = null;

            string col = "entry,guid,x,y,z,o,time,mapID";
            columns = col.Split(new char[] { ',' });
            foreach (var column in columns)
                dt.Columns.Add(column);

            try
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    string line;
                    string[] buffer = new string[512]; // Adjust the buffer size as needed

                    int bufferIndex = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains(sniff_state))
                        {
                            // A new packet has started, reset the packet information.
                            sniff.entry = "";
                            sniff.guid = "";
                            sniff.x = "";
                            sniff.y = "";
                            sniff.z = "";
                            sniff.o = "NULL";
                            sniff.time = "";

                            // Skip this line.
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line) && !Properties.Settings.Default.ObjectUpdate && line.Contains(sniff_move))
                        {
                            string[] values = line.Split(new char[] { ' ' });
                            string[] time = values[move_time].Split(new char[] { '.' });
                            sniff.time = time[0];

                            bufferIndex = 0;
                            do
                            {
                                line = reader.ReadLine();
                                if (!string.IsNullOrEmpty(line))
                                {
                                    buffer[bufferIndex++] = line;
                                    if (line.Contains(sniff_move_1))
                                    {
                                        if (line.Contains(sniff_move_2) || line.Contains(sniff_move_3))
                                        {
                                            string[] packetline = line.Split(new char[] { ' ' });
                                            sniff.entry = packetline[move_entry];
                                            sniff.guid = packetline[move_guid];
                                        }
                                    }
                                    if (line.Contains(sniff_move_4))
                                    {
                                        string[] packetline = line.Split(new char[] { ' ' });
                                        sniff.x = packetline[move_x];
                                        sniff.y = packetline[move_y];
                                        sniff.z = packetline[move_z];
                                        sniff.o = "100";
                                    }
                                    if (line.Contains(sniff_move_5))
                                    {
                                        string[] packetline = line.Split(new char[] { ' ' });
                                        sniff.x = packetline[move_pointx];
                                        sniff.y = packetline[move_pointy];
                                        sniff.z = packetline[move_pointz];
                                    }
                                    if (line.Contains(sniff_move_6))
                                    {
                                        sniff.entry = "";
                                        break;
                                    }
                                    if (line.Contains(sniff_move_7))
                                    {
                                        string[] packetline = line.Split(new char[] { ' ' });
                                        sniff.o = packetline[move_o];
                                    }
                                }
                            } while (line != "" && bufferIndex < buffer.Length);

                            if (!string.IsNullOrEmpty(sniff.entry))
                            {
                                DataRow dr = dt.NewRow();
                                dr[0] = sniff.entry;
                                dr[1] = sniff.guid;
                                dr[2] = sniff.x;
                                dr[3] = sniff.y;
                                dr[4] = sniff.z;
                                dr[5] = sniff.o;
                                dr[6] = sniff.time;
                                dr[7] = mapID;
                                dt.Rows.Add(dr);
                                sniff.entry = "";
                            }
                        }
                    }

                }
            }
            finally
            {
                // Reset cursor to default
                this.Cursor = Cursors.Default;
            }

            return dt;
        }


        public void FillListBoxWithGuids(string entry)
        {
            guids.Clear();
            guids = waypoints.DefaultView.ToTable(true, "guid", "entry");
            List<string> lst = new List<string>();

            foreach (DataRow r in guids.Rows)
            {
                if (entry != "0")
                {
                    if (entry == r["entry"].ToString())
                        lst.Add(r["guid"].ToString());
                }
                else
                {
                    if (r["guid"].ToString() != "")
                        lst.Add(r["guid"].ToString());
                }
            }

            lst.Sort();
            if (listBox.DataSource != lst)
                listBox.DataSource = lst;
            listBox.Refresh();
        }

        public void FillGrid()
        {
                creature_guid = (string)listBox.SelectedItem;
                movePackets = waypoints.Clone();

                foreach (DataRow row in waypoints.Rows)
                {
                    if (row.Field<string>(1) == creature_guid)
                        movePackets.ImportRow(row);
                }

                creature_entry = movePackets.Rows[0].Field<string>(0);

                gridWaypoint.Rows.Clear();

                for (var l = 0; l < movePackets.Rows.Count; l++)
                    gridWaypoint.Rows.Add(l + 1, movePackets.Rows[l].Field<string>(2), movePackets.Rows[l].Field<string>(3), movePackets.Rows[l].Field<string>(4), movePackets.Rows[l].Field<string>(5), movePackets.Rows[l].Field<string>(6), "");

            Findrange();
        }

        public void GraphPath()
        {
            chart.BackColor = Properties.Settings.Default.BackColour;
            chart.ChartAreas[0].BackColor = Properties.Settings.Default.BackColour;
            chart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chart.ChartAreas[0].AxisY.ScaleView.ZoomReset();
            chart.ChartAreas[0].AxisY.IsReversed = true;

            if (creature_entry == "")
            {
                return;
            }

            if (Properties.Settings.Default.UsingDB == true)
            {
                DataSet DS = new DataSet();
                string sqltext = "SELECT `name` FROM `creature_template` WHERE `entry`=" + creature_entry + ";";
                DS = (DataSet)Module.database_conn(sqltext);

                if (DS.Tables["table1"].Rows.Count > 0)
                {
                    creature_name = DS.Tables["table1"].Rows[0][0].ToString();
                    chart.Titles.Clear();
                    Title title = chart.Titles.Add(creature_name + " Entry: " + creature_entry);
                    title.Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                    title.ForeColor = Properties.Settings.Default.TitleColour;
                }
                else
                {
                    chart.Titles.Clear();
                    Title title = chart.Titles.Add("Entry " + creature_entry + " not in database");
                    title.Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                    title.ForeColor = Properties.Settings.Default.TitleColour;
                }
            }
            else
            {
                chart.Titles.Clear();
                Title title = chart.Titles.Add("Entry " + creature_entry + " database not connected");
                title.Font = new System.Drawing.Font("Arial", 16, FontStyle.Bold);
                title.ForeColor = Properties.Settings.Default.TitleColour;
            }

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

        private void CreateSQL_DB()
        {
            //Send to SQL
            SQLtext = "-- Pathing for " + creature_name + " Entry: " + creature_entry + "\r\n" + "SET @NPC := XXXXXX;" + "\r\n" + "SET @PATH := @NPC * 10;" + "\r\n";
            SQLtext = SQLtext + "UPDATE `creature` SET `wander_distance`=0,`MovementType`=2,`position_x`=" + Convert.ToString(gridWaypoint[1, 0].Value) + ",`position_y`=" + Convert.ToString(gridWaypoint[2, 0].Value) + ",`position_z`=" + Convert.ToString(gridWaypoint[3, 0].Value) + " WHERE `guid`=@NPC;" + "\r\n";
            SQLtext = SQLtext + "DELETE FROM `creature_addon` WHERE `guid`=@NPC;" + "\r\n";
            SQLtext = SQLtext + "INSERT INTO `creature_addon` (`guid`,`path_id`,`mount`,`bytes1`,`bytes2`,`emote`,`visibilityDistanceType`,`auras`) VALUES (@NPC,@PATH,0,0,1,0,0, '');" + "\r\n";
            SQLtext = SQLtext + "DELETE FROM `waypoint_data` WHERE `id`=@PATH;" + "\r\n";
            SQLtext = SQLtext + "INSERT INTO `waypoint_data` (`id`,`point`,`position_x`,`position_y`,`position_z`,`orientation`,`delay`,`move_type`,`action`,`action_chance`,`wpguid`) VALUES" + "\r\n";

            for (var l = 0; l < gridWaypoint.RowCount; l++)
            {
                string facing = Convert.ToString(gridWaypoint[4, l].Value);
                if (facing == "")
                    facing = "0";

                string time = Convert.ToString(gridWaypoint[5, l].Value);

                string waittime = Convert.ToString(gridWaypoint[6, l].Value);
                if (waittime == "")
                    waittime = "0";

                SQLtext = SQLtext + "(@PATH," + (l + 1) + ",";

                for (var ll = 1; ll < 4; ll++)
                {
                    SQLtext = SQLtext + gridWaypoint[ll, l].Value + ",";
                }

                if (l < (gridWaypoint.RowCount - 1))
                {
                    SQLtext = SQLtext + facing + "," + waittime + ",0,0,100,0)," + "\r\n";
                }
                else
                {
                    SQLtext = SQLtext + facing + "," + waittime + ",0,0,100,0);" + "\r\n";
                }
            }
                
            SQLtext = SQLtext + "-- " + (string)listBox.SelectedItem + " .go xyz " + Convert.ToString(gridWaypoint[1, 0].Value) + " " + Convert.ToString(gridWaypoint[2, 0].Value) + " " + Convert.ToString(gridWaypoint[3, 0].Value) + "\r\n";
            txtOutput.Text = txtOutput.Text + SQLtext + "\r\n";
        }

        private void CreateSQL_UDB()
        {
            //Send to SQL
            SQLtext = "-- Pathing for " + creature_name + " Entry: " + creature_entry + " 'UDB FORMAT' \r\n" + "SET @GUID := XXXXXX;" + "\r\n";
            SQLtext = SQLtext + "UPDATE `creature` SET `wander_distance`=0,`MovementType`=2,`position_x`=" + Convert.ToString(gridWaypoint[1, 0].Value) + ",`position_y`=" + Convert.ToString(gridWaypoint[2, 0].Value) + ",`position_z`=" + Convert.ToString(gridWaypoint[3, 0].Value) + " WHERE `guid`=@GUID;" + "\r\n";
            SQLtext = SQLtext + "DELETE FROM `creature_movement` WHERE `id`=@GUID;" + "\r\n";
            SQLtext = SQLtext + "INSERT INTO `creature_movement` (`id`,`point`,`position_x`,`position_y`,`position_z`,`waittime`,`script_id`,`orientation`) VALUES" + "\r\n";

            for (var l = 0; l < gridWaypoint.RowCount; l++)
            {
                string facing = Convert.ToString(gridWaypoint[4, l].Value);
                if (facing == "")
                    facing = "0";

                string waittime = Convert.ToString(gridWaypoint[6, l].Value);
                if (waittime == "")
                    waittime = "0";

                SQLtext = SQLtext + "(@GUID," + (l + 1) + ",";

                for (var ll = 1; ll < 4; ll++)
                {
                    SQLtext = SQLtext + gridWaypoint[ll, l].Value + ",";
                }

                if (l < (gridWaypoint.RowCount - 1))
                {
                    SQLtext = SQLtext + waittime + ",0," + facing + ")," + "\r\n";
                }
                else
                {
                    SQLtext = SQLtext + waittime + ",0," + facing + ");" + "\r\n";
                }
            }

            SQLtext = SQLtext + "-- " + (string)listBox.SelectedItem + " .go xyz " + Convert.ToString(gridWaypoint[1, 0].Value) + " " + Convert.ToString(gridWaypoint[2, 0].Value) + " " + Convert.ToString(gridWaypoint[3, 0].Value) + "\r\n";
            txtOutput.Text = txtOutput.Text + SQLtext + "\r\n";
        }

        private void CreateSQL_SAI()
        {
            string name = creature_name.Replace("'", "''");
            //Send to SQL
            SQLtext = "-- Pathing for " + creature_name + " Entry: " + creature_entry + " 'SAI FORMAT' \r\n" + "SET @NPC := XXXXXX;" + "\r\n";
            SQLtext = SQLtext + "DELETE FROM `waypoints` WHERE `entry`=@NPC;" + "\r\n";
            SQLtext = SQLtext + "INSERT INTO `waypoints` (`entry`,`pointid`,`position_x`,`position_y`,`position_z`,`orientation`,`delay`,`point_comment`) VALUES" + "\r\n";

            for (var l = 0; l < gridWaypoint.RowCount; l++)
            {
                string facing = Convert.ToString(gridWaypoint[4, l].Value);
                if (facing == "")
                    facing = "0";

                string time = Convert.ToString(gridWaypoint[5, l].Value);

                string waittime = Convert.ToString(gridWaypoint[6, l].Value);
                if (waittime == "")
                    waittime = "0";

                SQLtext = SQLtext + "(@NPC," + (l + 1) + ",";

                for (var ll = 1; ll < 4; ll++)
                {
                    SQLtext = SQLtext + gridWaypoint[ll, l].Value + ",";
                }

                if (l < (gridWaypoint.RowCount - 1))
                {
                    SQLtext = SQLtext + facing + "," + waittime + "'" + name + ",')," + "\r\n";
                }
                else
                {
                    SQLtext = SQLtext + facing + "," + waittime + "'" + name + ",');" + "\r\n";
                }
            }

            SQLtext = SQLtext + "-- " + (string)listBox.SelectedItem + " .go xyz " + Convert.ToString(gridWaypoint[1, 0].Value) + " " + Convert.ToString(gridWaypoint[2, 0].Value) + " " + Convert.ToString(gridWaypoint[3, 0].Value) + "\r\n";
            txtOutput.Text = txtOutput.Text + SQLtext + "\r\n";
        }

        private void CreateSQL_vmangos()
        {
            //Send to SQL
            SQLtext = "-- Pathing for " + creature_name + " Entry: " + creature_entry + "\r\n" + "SET @NPC := XXXXXX;" + "\r\n";
            SQLtext = SQLtext + "UPDATE `creature` SET `wander_distance`=0,`movement_type`=2,`position_x`=" + Convert.ToString(gridWaypoint[1, 0].Value) + ",`position_y`=" + Convert.ToString(gridWaypoint[2, 0].Value) + ",`position_z`=" + Convert.ToString(gridWaypoint[3, 0].Value) + " WHERE `guid`=@NPC;" + "\r\n";
            SQLtext = SQLtext + "DELETE FROM `creature_movement` WHERE `id`=@NPC;" + "\r\n";
            SQLtext = SQLtext + "INSERT INTO `creature_movement` (`id`,`point`,`position_x`,`position_y`,`position_z`,`orientation`,`waittime`,`wander_distance`,`script_id`) VALUES" + "\r\n";

            for (var l = 0; l < gridWaypoint.RowCount; l++)
            {
                string facing = Convert.ToString(gridWaypoint[4, l].Value);
                if (facing == "")
                    facing = "0";

                string time = Convert.ToString(gridWaypoint[5, l].Value);

                string waittime = Convert.ToString(gridWaypoint[6, l].Value);
                if (waittime == "")
                    waittime = "0";

                SQLtext = SQLtext + "(@NPC," + (l + 1) + ",";

                for (var ll = 1; ll < 4; ll++)
                {
                    SQLtext = SQLtext + gridWaypoint[ll, l].Value + ",";
                }

                if (l < (gridWaypoint.RowCount - 1))
                {
                    SQLtext = SQLtext + facing + "," + waittime + ",0,0)," + "\r\n";
                }
                else
                {
                    SQLtext = SQLtext + facing + "," + waittime + ",0,0);" + "\r\n";
                }
            }

            SQLtext = SQLtext + "-- " + (string)listBox.SelectedItem + " .go xyz " + Convert.ToString(gridWaypoint[1, 0].Value) + " " + Convert.ToString(gridWaypoint[2, 0].Value) + " " + Convert.ToString(gridWaypoint[3, 0].Value) + "\r\n";
            txtOutput.Text = txtOutput.Text + SQLtext + "\r\n";
        }

        private void CreateCode_cpp()
        {
            String Codetext = "// Position Constant for " + creature_name + " Entry: " + creature_entry + " 'C++ FORMAT' \r\n" + "Position const XXXXXX[] =" + "\r\n" + "{" + "\r\n";
            String Codeline = "";

            for (var l = 0; l < gridWaypoint.RowCount; l++)
            {
                Codeline = "    { ";
                for (var ll = 1; ll < 4; ll++)
                {
                    Codeline = Codeline + gridWaypoint[ll, l].Value + "f";
                    if (ll < 3)
                        Codeline = Codeline + ", ";
                    else
                        Codeline = Codeline + " }";
                }
                if (l < gridWaypoint.RowCount - 1)
                    Codeline = Codeline + "," + "\r\n";
                else
                    Codeline = Codeline + "\r\n";

                Codetext = Codetext + Codeline;
            }
            Codetext = Codetext + "};" + "\r\n";
            txtOutput.Text = txtOutput.Text + Codetext + "\r\n";
        }

        private void Sniff_version_4()
        {
            state_map = 2;
            move_time = 9;
            move_entry = 6;
            move_guid = 2;
            move_x = 2;
            move_y = 4;
            move_z = 6;
            move_o = 2;
            move_pointx = 3;
            move_pointy = 5;
            move_pointz = 7;
            object_time = 9;
            object_entry = 7;
            object_guid = 3;
            object_pointx = 5;
            object_pointy = 7;
            object_pointz = 9;
            sniff_state = "SMSG_INIT_WORLD_STATES";
            sniff_move = "SMSG_ON_MONSTER_MOVE";
            sniff_move_1 = "GUID: Full:";
            sniff_move_2 = "Creature";
            sniff_move_3 = "Vehicle";
            sniff_move_4 = "Position: X:";
            sniff_move_5 = " Endpoint: X:";
            sniff_move_6 = "Spline Type: 3";
            sniff_move_7 = "Facing Angle:";
            sniff_object = "SMSG_UPDATE_OBJECT";
            sniff_object_1 = "GUID: Full:";
            sniff_object_2 = "Creature";
            sniff_object_3 = "Vehicle";
            sniff_object_4 = "Spline Waypoint: X:";
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
            object_time = 9;
            object_entry = 9;
            object_guid = 3;
            object_pointx = 4;
            object_pointy = 6;
            object_pointz = 8;
            sniff_state = "SMSG_INIT_WORLD_STATES";
            sniff_move = "SMSG_ON_MONSTER_MOVE";
            sniff_move_1 = "MoverGUID: Full:";
            sniff_move_2 = "Creature/0";
            sniff_move_3 = "Vehicle/0";
            sniff_move_4 = "Position: X:";
            sniff_move_5 = " Points: X:";
            sniff_move_6 = "Face: 2";
            sniff_move_7 = "FaceDirection:";
            sniff_object = "SMSG_UPDATE_OBJECT";
            sniff_object_1 = "MoverGUID: Full:";
            sniff_object_2 = "Creature/0";
            sniff_object_3 = "Vehicle/0";
            sniff_object_4 = "Endpoint: X:";
        }

        private void Findrange()
        {
            float wanderRange = 0.0f;

            int rowCount = gridWaypoint.RowCount;

            float[] xValues = new float[rowCount];
            float[] yValues = new float[rowCount];
            float[] zValues = new float[rowCount];

            for (int i = 0; i < rowCount; i++)
            {
                xValues[i] = float.Parse(gridWaypoint[1, i].Value.ToString());
                yValues[i] = float.Parse(gridWaypoint[2, i].Value.ToString());
                zValues[i] = float.Parse(gridWaypoint[3, i].Value.ToString());
            }

            for (int i = 0; i < rowCount; i++)
            {
                float x1 = xValues[i];
                float y1 = yValues[i];
                float z1 = zValues[i];

                for (int ii = i + 1; ii < rowCount; ii++)
                {
                    float x2 = xValues[ii];
                    float y2 = yValues[ii];
                    float z2 = zValues[ii];

                    float deltaX = x2 - x1;
                    float deltaY = y2 - y1;
                    float deltaZ = z2 - z1;

                    midX = (x2 + x1) / 2;
                    midY = (y2 + y1) / 2;

                    float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

                    if (distance > wanderRange)
                        wanderRange = distance;
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
