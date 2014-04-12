using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace LEAFDB
{
    public partial class MySQLConnectionDetailsForm : Form
    {
        public MySQLConnectionDetailsForm()
        {
            InitializeComponent();

            // TODO: Set finalConnectionString based on saved data & populate fields!
            this.hostnameIPTextBox.Text = Properties.Settings.Default["connectionStringHostnameIP"].ToString();
            this.portNumericUpDown.Value = Convert.ToDecimal(Properties.Settings.Default["connectionStringPort"]);
            this.usernameTextBox.Text = Properties.Settings.Default["connectionStringUsername"].ToString();
            this.passwordTextBox.Text = Properties.Settings.Default["connectionStringPassword"].ToString();
            if (hostnameIPTextBox.Text.Length > 0 && usernameTextBox.Text.Length > 0 && passwordTextBox.Text.Length > 0)
                queryDatabases(MysqlHelper.generateConnectionString(hostnameIPTextBox.Text, usernameTextBox.Text, passwordTextBox.Text, Convert.ToUInt16(portNumericUpDown.Value)));
        }

        private void oKButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            
            // Save per-user connection string setting for later application runs:
            Properties.Settings.Default["connectionStringHostnameIP"] = lastSuccessfulHostnameIP;
            Properties.Settings.Default["connectionStringPort"] = lastSuccessfulPort;
            Properties.Settings.Default["connectionStringUsername"] = lastSuccessfulUsername;
            Properties.Settings.Default["connectionStringPassword"] = lastSuccessfulPassword;
            Properties.Settings.Default["connectionStringDatabase"] = databaseComboBox.SelectedText;

            // POTENTIAL ERROR: If they have modified any of the above boxes, it'll be saving data that wasn't used to open the actual connection to the database and test for available databases!
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            // Don't overwrite connection details, perhaps the server needed to be started!
            this.Close();
        }

        private void testConnectionAndPopulateButton_Click(object sender, EventArgs e)
        {
            if (hostnameIPTextBox.Text.Length > 0 && usernameTextBox.Text.Length > 0 && passwordTextBox.Text.Length > 0)
                queryDatabases(MysqlHelper.generateConnectionString(hostnameIPTextBox.Text, usernameTextBox.Text, passwordTextBox.Text, Convert.ToUInt16(portNumericUpDown.Value)));
//                queryDatabases("Server=" + hostnameIPTextBox.Text + ";Port=" + portNumericUpDown.Value.ToString() + ";Database=" + databaseComboBox.SelectedText + ";Uid=" + usernameTextBox.Text + ";Pwd=" + passwordTextBox.Text + ";");
        }

        private void queryDatabases(string connectionString)
        {
            // TODO: Make this nicer:
            setInputEnabled(false);

            // Clear combobox no matter what:
            databaseComboBox.Items.Clear();
            this.oKButton.Enabled = false; // only enabled if successful!
            this.databaseComboBox.Enabled = false;

            // Grab new values for combo box or show error message and leave it empty:
            using(MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try {
                    MySqlCommand command = new MySqlCommand("show databases;", conn);
                    conn.Open();
                    MySqlDataReader reader;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                        databaseComboBox.Items.Add(reader.GetString(0));
                    reader.Close();

                    // enable OK button if successfully connected and have databases available:
                    this.oKButton.Enabled = true;
                    this.databaseComboBox.Enabled = true;

                    this.lastSuccessfulHostnameIP = hostnameIPTextBox.Text;
                    this.lastSuccessfulPort = Convert.ToUInt16(portNumericUpDown.Value);
                    this.lastSuccessfulUsername = usernameTextBox.Text;
                    this.lastSuccessfulPassword = passwordTextBox.Text;

                    // default to 'leaf' if available, as that's what it is by default:
                    for (int c = 0; c < this.databaseComboBox.Items.Count; c++)
                    {
                        if (this.databaseComboBox.Items[c].ToString() == "leaf")
                        {
                            this.databaseComboBox.SelectedIndex = c;
                            break;
                        }
                    }
                }
                catch (MySqlException ex) {
                    MessageBox.Show("MySQL Error Number " + ex.Number + ", " + ex.Message);

                    // DEBUG ONLY, otherwise you're showing the saved password to a random person:
                    //  (although it is saved on the computer somewhere and they COULD go find it)
                    MessageBox.Show(connectionString);
                }
            }
            
            // TODO: Make this nicer:
            setInputEnabled(true);
        }

        private void setInputEnabled(bool setTo = true)
        {
            this.hostnameIPTextBox.Enabled = setTo;
            this.portNumericUpDown.Enabled = setTo;
            this.usernameTextBox.Enabled = setTo;
            this.passwordTextBox.Enabled = setTo;
            // database combobox is handled elsewhere as special case.
            //  The only time it is enabled is NOT during the times this function's in use
        }

        string lastSuccessfulHostnameIP;
        UInt16 lastSuccessfulPort;
        string lastSuccessfulUsername;
        string lastSuccessfulPassword;
    }
}
