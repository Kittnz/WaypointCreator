using System;
using System.Drawing;
using System.Windows.Forms;

namespace Frm_waypoint
{
    public partial class frm_Settings : Form
    {
        public const int boxPoint = 1;
        public const int boxLine  = 2;
        public const int boxBack  = 3;
        public const int boxTitle = 4;

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

        private void SetColour(int item)
        {
            ColorDialog colorDlg    = new ColorDialog();
            colorDlg.AllowFullOpen  = true;
            colorDlg.AnyColor       = true;
            colorDlg.SolidColorOnly = false;
            colorDlg.Color          = Color.Red;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                switch (item)
                {
                    case boxPoint:
                        picBoxPointColour.BackColor = colorDlg.Color;
                        break;
                    case boxLine:
                        picBoxLineColour.BackColor = colorDlg.Color;
                        break;
                    case boxBack:
                        picBoxBackColour.BackColor = colorDlg.Color;
                        break;
                    case boxTitle:
                        picBoxTitleColour.BackColor = colorDlg.Color;
                        break;
                }
            }
        }

        private void GetValues()
        {
            if (Properties.Settings.Default.vmangos == true)
                chkBoxVMaNGOS.CheckState = CheckState.Checked;
            else
                chkBoxVMaNGOS.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.cmangos == true)
                chkBoxCMaNGOS.CheckState = CheckState.Checked;
            else
                chkBoxCMaNGOS.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.trinitycore == true)
                chkBoxTrinityCore.CheckState = CheckState.Checked;
            else
                chkBoxTrinityCore.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.CPP == true)
                chkBoxCPP.CheckState = CheckState.Checked;
            else
                chkBoxCPP.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.Lines == true)
                chkBoxLine.CheckState = CheckState.Checked;
            else
                chkBoxLine.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.Splines == true)
                chkBoxSpline.CheckState = CheckState.Checked;
            else
                chkBoxSpline.CheckState = CheckState.Unchecked;

            picBoxPointColour.BackColor = Properties.Settings.Default.PointColour;
            picBoxLineColour.BackColor = Properties.Settings.Default.LineColour;
            picBoxBackColour.BackColor = Properties.Settings.Default.BackColour;
            picBoxTitleColour.BackColor = Properties.Settings.Default.TitleColour;
        }

        private void SaveValues()
        {
            if (chkBoxVMaNGOS.CheckState == CheckState.Checked)
                Properties.Settings.Default.vmangos = true;
            else
                Properties.Settings.Default.vmangos = false;

            if (chkBoxCMaNGOS.CheckState == CheckState.Checked)
                Properties.Settings.Default.cmangos = true;
            else
                Properties.Settings.Default.cmangos = false;

            if (chkBoxTrinityCore.CheckState == CheckState.Checked)
                Properties.Settings.Default.trinitycore = true;
            else
                Properties.Settings.Default.trinitycore = false;

            if (chkBoxCPP.CheckState == CheckState.Checked)
                Properties.Settings.Default.CPP = true;
            else
                Properties.Settings.Default.CPP = false;

            if (chkBoxLine.CheckState == CheckState.Checked)
                Properties.Settings.Default.Lines = true;
            else
                Properties.Settings.Default.Lines = false;

            if (chkBoxSpline.CheckState == CheckState.Checked)
                Properties.Settings.Default.Splines = true;
            else
                Properties.Settings.Default.Splines = false;

            Properties.Settings.Default.PointColour = picBoxPointColour.BackColor;
            Properties.Settings.Default.LineColour = picBoxLineColour.BackColor;
            Properties.Settings.Default.BackColour = picBoxBackColour.BackColor;
            Properties.Settings.Default.TitleColour = picBoxTitleColour.BackColor;
            Properties.Settings.Default.Save();
        }

        private void SetDefaults()
        {
            chkBoxVMaNGOS.CheckState = CheckState.Checked;
            chkBoxCMaNGOS.CheckState = CheckState.Unchecked;
            chkBoxTrinityCore.CheckState = CheckState.Unchecked;
            chkBoxCPP.CheckState = CheckState.Unchecked;
            chkBoxLine.CheckState = CheckState.Checked;
            chkBoxSpline.CheckState = CheckState.Unchecked;
            picBoxPointColour.BackColor = Color.Blue;
            picBoxLineColour.BackColor = Color.Aqua;
            picBoxBackColour.BackColor = Color.White;
            picBoxTitleColour.BackColor = Color.Blue;
        }

        private void SQLFormat_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox clickedCheckbox = (CheckBox)sender;

            if (clickedCheckbox.Checked)
            {
                // Uncheck all other checkboxes
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
                // If unchecking, ensure at least one remains checked
                bool anyChecked = chkBoxVMaNGOS.Checked || chkBoxCMaNGOS.Checked ||
                                 chkBoxTrinityCore.Checked || chkBoxCPP.Checked;

                if (!anyChecked)
                {
                    clickedCheckbox.Checked = true; // Re-check the box if none are checked
                }
            }
        }

        private void ChkBoxLine_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/disable the spline checkbox based on the "Show Lines" state
            chkBoxSpline.Enabled = chkBoxLine.Checked;

            // If "Show Lines" is being unchecked, also uncheck "Linetype Spline"
            if (!chkBoxLine.Checked)
            {
                chkBoxSpline.Checked = false;
            }
        }
    }
}
