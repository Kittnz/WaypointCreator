using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Frm_waypoint
{
    public partial class frm_Settings : Form
    {
        public const int boxPoint = 1;
        public const int boxLine = 2;
        public const int boxBack = 3;
        public const int boxTitle = 4;
        public const int boxConnector = 5;

        public frm_Settings()
        {
            InitializeComponent();
        }

        private void Frm_settings_Load(object sender, EventArgs e)
        {
            GetValues();
        }

        private void BtnOKClick(object sender, EventArgs e)
        {
            SaveValues();
            this.Close();
        }

        private void BtnDefault_Click(object sender, EventArgs e)
        {
            SetDefaults();
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnPointColour_Click(object sender, EventArgs e)
        {
            SetColour(boxPoint);
        }

        private void BtnLineColour_Click(object sender, EventArgs e)
        {
            SetColour(boxLine);
        }

        private void BtnBackColour_Click(object sender, EventArgs e)
        {
            SetColour(boxBack);
        }

        private void BtnTitleColour_Click(object sender, EventArgs e)
        {
            SetColour(boxTitle);
        }

        private void BtnConnectorColour_Click(object sender, EventArgs e)
        {
            SetColour(boxConnector);
        }

        private void SetColour(int item)
        {
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.AllowFullOpen = true;
            colorDlg.AnyColor = true;
            colorDlg.SolidColorOnly = false;
            colorDlg.Color = Color.Red;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                switch (item)
                {
                    case boxPoint:
                        Properties.Settings.Default.PointColour = colorDlg.Color;
                        picBoxPointColour.BackColor = colorDlg.Color;
                        break;
                    case boxLine:
                        Properties.Settings.Default.LineColour = colorDlg.Color;
                        picBoxLineColour.BackColor = colorDlg.Color;
                        break;
                    case boxBack:
                        Properties.Settings.Default.BackColour = colorDlg.Color;
                        picBoxBackColour.BackColor = colorDlg.Color;
                        break;
                    case boxTitle:
                        Properties.Settings.Default.TitleColour = colorDlg.Color;
                        picBoxTitleColour.BackColor = colorDlg.Color;
                        break;
                    case boxConnector:
                        Properties.Settings.Default.ConnectorLineColour = colorDlg.Color;
                        picBoxConnectorColour.BackColor = colorDlg.Color;
                        break;
                }
                Properties.Settings.Default.Save();
            }
        }

        private void GetValues()
        {
            // SQL Format checkboxes
            chkBoxVMaNGOS.Checked = Properties.Settings.Default.vmangos;
            chkBoxCMaNGOS.Checked = Properties.Settings.Default.cmangos;
            chkBoxTrinityCore.Checked = Properties.Settings.Default.trinitycore;
            chkBoxCPP.Checked = Properties.Settings.Default.CPP;

            // Line options
            chkBoxLine.Checked = Properties.Settings.Default.Lines;
            chkBoxSpline.Checked = Properties.Settings.Default.Splines;
            chkBoxSpline.Enabled = chkBoxLine.Checked;

            // Color boxes
            picBoxPointColour.BackColor = Properties.Settings.Default.PointColour;
            picBoxLineColour.BackColor = Properties.Settings.Default.LineColour;
            picBoxBackColour.BackColor = Properties.Settings.Default.BackColour;
            picBoxTitleColour.BackColor = Properties.Settings.Default.TitleColour;
            picBoxConnectorColour.BackColor = Properties.Settings.Default.ConnectorLineColour;
        }

        private void SaveValues()
        {
            // SQL Format
            Properties.Settings.Default.vmangos = chkBoxVMaNGOS.Checked;
            Properties.Settings.Default.cmangos = chkBoxCMaNGOS.Checked;
            Properties.Settings.Default.trinitycore = chkBoxTrinityCore.Checked;
            Properties.Settings.Default.CPP = chkBoxCPP.Checked;

            // Line options
            Properties.Settings.Default.Lines = chkBoxLine.Checked;
            Properties.Settings.Default.Splines = chkBoxSpline.Checked;

            // Colors
            Properties.Settings.Default.PointColour = picBoxPointColour.BackColor;
            Properties.Settings.Default.LineColour = picBoxLineColour.BackColor;
            Properties.Settings.Default.BackColour = picBoxBackColour.BackColor;
            Properties.Settings.Default.TitleColour = picBoxTitleColour.BackColor;
            Properties.Settings.Default.ConnectorLineColour = picBoxConnectorColour.BackColor;

            Properties.Settings.Default.Save();
        }

        private void SetDefaults()
        {
            // SQL Format defaults
            chkBoxVMaNGOS.Checked = true;
            chkBoxCMaNGOS.Checked = false;
            chkBoxTrinityCore.Checked = false;
            chkBoxCPP.Checked = false;

            // Line options defaults
            chkBoxLine.Checked = true;
            chkBoxSpline.Checked = false;

            // Color defaults
            picBoxPointColour.BackColor = Color.Blue;
            picBoxLineColour.BackColor = Color.Aqua;
            picBoxBackColour.BackColor = Color.White;
            picBoxTitleColour.BackColor = Color.Blue;
            picBoxConnectorColour.BackColor = Color.Gray;
        }

        private void SQLFormat_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox clickedCheckbox = (CheckBox)sender;

            if (clickedCheckbox.Checked)
            {
                // Uncheck all other SQL format checkboxes
                foreach (Control control in groupSQL.Controls)
                {
                    if (control is CheckBox && control != clickedCheckbox)
                    {
                        ((CheckBox)control).Checked = false;
                    }
                }
            }
            else
            {
                // Ensure at least one SQL format is checked
                if (!chkBoxVMaNGOS.Checked && !chkBoxCMaNGOS.Checked &&
                    !chkBoxTrinityCore.Checked && !chkBoxCPP.Checked)
                {
                    clickedCheckbox.Checked = true;
                }
            }
        }

        private void ChkBoxLine_CheckedChanged(object sender, EventArgs e)
        {
            chkBoxSpline.Enabled = chkBoxLine.Checked;
            if (!chkBoxLine.Checked)
            {
                chkBoxSpline.Checked = false;
            }
        }
    }
}