using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace dvereb.SQLConnectionForm
{
    public partial class SQLConnectionDetailsForm : Form
    {
        public SQLConnectionDetailsForm()
        {
            InitializeComponent();

            // TODO: Add other options besides MySQL:
            this.databaseTypeComboBox.SelectedIndex = 0;

            // Set finalConnectionString based on saved data & populate fields!
            this.hostnameIPTextBox.Text = Properties.Settings.Default.connectionStringHostnameIP;
            this.portNumericUpDown.Value = Properties.Settings.Default.connectionStringPort;
            this.usernameTextBox.Text = Properties.Settings.Default.connectionStringUsername;
            this.passwordTextBox.Text = Properties.Settings.Default.connectionStringPassword;

            {
                string connectionString = MysqlHelper.generateConnectionStringFromSavedSettings(false);
                if (connectionString != "")
                    queryDatabases(connectionString, this.databaseComboBox.Items);
            }
        }

        private void oKButton_Click(object sender, EventArgs e)
        {
            // verify no changes since db test
            if (hostnameIPTextBox.Text != lastSuccessfulHostnameIP
                || Convert.ToUInt16(portNumericUpDown.Value) != lastSuccessfulPort
                || usernameTextBox.Text != lastSuccessfulUsername
                || passwordTextBox.Text != lastSuccessfulPassword)
            {
                setSaveEnabled(false);
                MessageBox.Show("Please double check database connection as you've modified the connection string details since your last verification.");
                return;
            }

            // Save per-user connection string setting for later application runs:
            Properties.Settings.Default.connectionStringHostnameIP = lastSuccessfulHostnameIP;
            Properties.Settings.Default.connectionStringPort = lastSuccessfulPort;
            Properties.Settings.Default.connectionStringUsername = lastSuccessfulUsername;
            Properties.Settings.Default.connectionStringPassword = lastSuccessfulPassword;
            Properties.Settings.Default.connectionStringDatabase = databaseComboBox.SelectedItem.ToString();
            Properties.Settings.Default.Save();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
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
                queryDatabases(MysqlHelper.generateConnectionString(hostnameIPTextBox.Text, usernameTextBox.Text, passwordTextBox.Text, Convert.ToUInt16(portNumericUpDown.Value)), this.databaseComboBox.Items);
            else
                MessageBox.Show("You must enter a value in all four boxes: Hostname / IP, Port, Username, and Password.", "Error");
        }

        /// <summary>
        /// Uses "connectionString" parameter to connect to a MySQL Database & populates a combobox's items with a list of available database names
        /// </summary>
        /// <param name="connectionString">MySQL Connection String including Hostname/IP (Server=), Port (Port=), UserID (Uid=), and Password (Pwd=)</param>
        /// <param name="items">ComboBox.Items to populate with database names</param>
        private void queryDatabases(string connectionString, ComboBox.ObjectCollection items)
        {
            // TODO: Make this nicer:
            setInputEnabled(false);

            // Clear combobox no matter what:
            items.Clear();
            setSaveEnabled(false); // only enable if successful!

            // Grab new values for combo box or show error message and leave it empty:
            using(MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    MySqlCommand command = new MySqlCommand("show databases;", conn);
                    conn.Open();
                    MySqlDataReader reader;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                        items.Add(reader.GetString(0));
                    reader.Close();

                    // enable OK button if successfully connected and have databases available:
                    setSaveEnabled(true);

                    this.lastSuccessfulHostnameIP = hostnameIPTextBox.Text;
                    this.lastSuccessfulPort = Convert.ToUInt16(portNumericUpDown.Value);
                    this.lastSuccessfulUsername = usernameTextBox.Text;
                    this.lastSuccessfulPassword = passwordTextBox.Text;

                    if(items.Count > 0)
                    {
                        // Default to currently-being-used database, if available...
                        //  If not, default to connectionStringDefaultDatabase if available:
                        bool found = false;
                        for (int c = 0; c < this.databaseComboBox.Items.Count; c++)
                        {
                            if (items[c].ToString() == Properties.Settings.Default.connectionStringDatabase)
                            {
                                this.databaseComboBox.SelectedIndex = c;
                                found = true;
                                break;
                            }
                            else if (items[c].ToString() == Properties.Settings.Default.connectionStringDefaultDatabase)
                            {
                                this.databaseComboBox.SelectedIndex = c;
                                found = true;
                                // don't break as we may find the LAST SELECTED database that was saved!
                            }
                        }
                        if (!found)
                            this.databaseComboBox.SelectedIndex = 0;
                    }
                }
                catch (MySqlException ex) {
                    MessageBox.Show("MySQL Error Number " + ex.Number + ", " + ex.Message);

                    // DEBUG ONLY, otherwise you're showing the saved password to a random person:
                    //  (although it is saved on the computer somewhere and they COULD go find it)
                    //  MessageBox.Show(connectionString);
                }
            }
            
            // TODO: Make this nicer:
            setInputEnabled(true);
        }

        /// <summary>
        /// Toggle "Enabled" property of the top four input boxes (Hostname/IP, Port, Username, and Password)
        /// </summary>
        /// <param name="setTo">Pass true to enable and false to disable, defaults to true</param>
        private void setInputEnabled(bool setTo = true)
        {
            this.hostnameIPTextBox.Enabled = setTo;
            this.portNumericUpDown.Enabled = setTo;
            this.usernameTextBox.Enabled = setTo;
            this.passwordTextBox.Enabled = setTo;
            // database combobox & OK button are handled elsewhere as special cases.
            //  The only time they are enabled is NOT during the times this function is in use
        }

        private void setSaveEnabled(bool setTo = true)
        {
            this.saveButton.Enabled = setTo;
            this.databaseComboBox.Enabled = setTo;
        }

        // used to verify no changes upon confirmation:
        string lastSuccessfulHostnameIP;
        UInt16 lastSuccessfulPort;
        string lastSuccessfulUsername;
        string lastSuccessfulPassword;
    }
}
