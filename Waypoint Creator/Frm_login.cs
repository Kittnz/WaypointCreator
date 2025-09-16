using System;
using System.Windows.Forms;
using MySqlConnector;

namespace Frm_waypoint
{
    public partial class frm_Login : Form
    {
        public frm_Login()
        {
            InitializeComponent();
            LoadSavedSettings();
        }

        private void LoadSavedSettings()
        {
            var settings = Properties.Settings.Default;
            txt_Host.Text = settings.host;
            txt_Username.Text = settings.username;
            txt_Password.Text = settings.password;
            txt_Database.Text = settings.database;
            txt_Port.Text = settings.port;
        }

        private void Btn_OK_Click(object sender, EventArgs e)
        {
            try
            {
                using (var connection = new MySqlConnection(BuildConnectionString()))
                {
                    connection.Open();

                    if (chkBox_SaveValues.Checked)
                        SaveConnectionSettings();

                    Properties.Settings.Default.UsingDB = true;
                    Properties.Settings.Default.Save();

                    LoadMain();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuildConnectionString()
        {
            return new MySqlConnectionStringBuilder
            {
                Server = txt_Host.Text,
                Port = uint.Parse(txt_Port.Text), // Throws if invalid
                UserID = txt_Username.Text,
                Password = txt_Password.Text,
                Database = txt_Database.Text
            }.ToString();
        }

        private void SaveConnectionSettings()
        {
            var settings = Properties.Settings.Default;
            settings.host = txt_Host.Text;
            settings.username = txt_Username.Text;
            settings.password = txt_Password.Text;
            settings.database = txt_Database.Text;
            settings.port = txt_Port.Text;
            settings.Save();
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UsingDB = false;
            Properties.Settings.Default.Save();
            LoadMain();
        }

        private void LoadMain()
        {
            new Frm_Waypoint().Show();
            this.Hide();
        }
    }
}