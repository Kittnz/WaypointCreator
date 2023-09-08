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
            if (Properties.Settings.Default.DB == true)
                chkBoxDB.CheckState = CheckState.Checked;
            else
                chkBoxDB.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.UDB == true)
                chkBoxUDB.CheckState = CheckState.Checked;
            else
                chkBoxUDB.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.SAI == true)
                chkBoxSAI.CheckState = CheckState.Checked;
            else
                chkBoxSAI.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.CPP == true)
                chkBoxCPP.CheckState = CheckState.Checked;
            else
                chkBoxCPP.CheckState = CheckState.Unchecked;
            if (Properties.Settings.Default.vmangos == true)
                chkBoxvmangos.CheckState = CheckState.Checked;
            else
                chkBoxvmangos.CheckState = CheckState.Unchecked;
            if (Properties.Settings.Default.Lines == true)
                chkBoxLine.CheckState = CheckState.Checked;
            else
                chkBoxLine.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.Splines == true)
                chkBoxSpline.CheckState = CheckState.Checked;
            else
                chkBoxSpline.CheckState = CheckState.Unchecked;

            if (Properties.Settings.Default.ObjectUpdate == true)
                chkBoxObject.CheckState = CheckState.Checked;
            else
                chkBoxObject.CheckState = CheckState.Unchecked;

            picBoxPointColour.BackColor = Properties.Settings.Default.PointColour;
            picBoxLineColour.BackColor = Properties.Settings.Default.LineColour;
            picBoxBackColour.BackColor = Properties.Settings.Default.BackColour;
            picBoxTitleColour.BackColor = Properties.Settings.Default.TitleColour;
        }

        private void SaveValues()
        {
            if (chkBoxDB.CheckState == CheckState.Checked)
                Properties.Settings.Default.DB = true;
            else
                Properties.Settings.Default.DB = false;

            if (chkBoxUDB.CheckState == CheckState.Checked)
                Properties.Settings.Default.UDB = true;
            else
                Properties.Settings.Default.UDB = false;

            if (chkBoxSAI.CheckState == CheckState.Checked)
                Properties.Settings.Default.SAI = true;
            else
                Properties.Settings.Default.SAI = false;

            if (chkBoxCPP.CheckState == CheckState.Checked)
                Properties.Settings.Default.CPP = true;
            else
                Properties.Settings.Default.CPP = false;

            if (chkBoxvmangos.CheckState == CheckState.Checked)
                Properties.Settings.Default.vmangos = true;
            else
                Properties.Settings.Default.vmangos = false;

            if (chkBoxLine.CheckState == CheckState.Checked)
                Properties.Settings.Default.Lines = true;
            else
                Properties.Settings.Default.Lines = false;

            if (chkBoxSpline.CheckState == CheckState.Checked)
                Properties.Settings.Default.Splines = true;
            else
                Properties.Settings.Default.Splines = false;

            if (chkBoxObject.CheckState == CheckState.Checked)
                Properties.Settings.Default.ObjectUpdate = true;
            else
                Properties.Settings.Default.ObjectUpdate = false;

            Properties.Settings.Default.PointColour = picBoxPointColour.BackColor;
            Properties.Settings.Default.LineColour = picBoxLineColour.BackColor;
            Properties.Settings.Default.BackColour = picBoxBackColour.BackColor;
            Properties.Settings.Default.TitleColour = picBoxTitleColour.BackColor;
            Properties.Settings.Default.Save();
        }

        private void SetDefaults()
        {
            chkBoxDB.CheckState = CheckState.Checked;
            chkBoxUDB.CheckState = CheckState.Unchecked;
            chkBoxSAI.CheckState = CheckState.Unchecked;
            chkBoxCPP.CheckState = CheckState.Unchecked;
            chkBoxvmangos.CheckState = CheckState.Unchecked;
            chkBoxLine.CheckState = CheckState.Checked;
            chkBoxSpline.CheckState = CheckState.Unchecked;
            picBoxPointColour.BackColor = Color.Blue;
            picBoxLineColour.BackColor = Color.Aqua;
            picBoxBackColour.BackColor = Color.White;
            picBoxTitleColour.BackColor = Color.Blue;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ChkBoxLine_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
