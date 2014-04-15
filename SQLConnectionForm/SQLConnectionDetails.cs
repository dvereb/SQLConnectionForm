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
        
        /// <summary>
        /// Initialize form values to their saved state, if it exists. Then it queries the database automatically
        /// </summary>
        /// <param name="autoQueryDatabasesAvailable">Defaults to true.  If true, the form will automatically check to see which databases are available based on the saved connection string from the user's last run of the application.  It automatically selects the last used database if the connection was successfull and the database is still accessible to the user via the saved connection string.</param>
        public SQLConnectionDetailsForm(bool autoQueryDatabasesAvailable = true)
        {
            InitializeComponent();

            // TODO: Add other options besides MySQL:
            this.databaseTypeComboBox.SelectedIndex = 0;

            // Set finalConnectionString based on saved data & populate fields!
            this.hostnameIPTextBox.Text = Properties.Settings.Default.connectionStringHostnameIP;
            this.portNumericUpDown.Value = Properties.Settings.Default.connectionStringPort;
            this.usernameTextBox.Text = Properties.Settings.Default.connectionStringUsername;
            this.passwordTextBox.Text = Properties.Settings.Default.connectionStringPassword;

            if(autoQueryDatabasesAvailable)
            {
                string connectionString = MysqlHelper.generateConnectionStringFromSavedSettings(false);
                if (connectionString != "")
                    queryDatabases(connectionString, this.databaseComboBox.Items);
            }
        }

        /// <summary>
        /// Returns a new MySqlConnection object connected to the most recently saved settings (even if it was saved in a previous run of the application).  Create one SQLConnectionDetailsForm at the beginning of your application and refer to it throughout instead of making multiple copies.
        /// 
        /// Run like this:
        /// SQLConnectionDetailsForm connDetails;
        /// using(connDetails.getMySqlConnection()) { /* code that requires SQL connection */ }
        /// </summary>
        /// <returns>new MySqlConnection object</returns>
        public MySqlConnection getMySqlConnection()
        {
            return new MySqlConnection(MysqlHelper.generateConnectionStringFromSavedSettings(true));
        }

        /// <summary>
        /// Save SQL connection string settings from textboxes into Application Properties.Settings.Default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Exits the form without saving a new connection string.  Sets DialogResult to Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            // Don't overwrite connection details, perhaps the server needed to be started!
            this.Close();
        }

        /// <summary>
        /// This button tests a connection to an SQL Server based on the data filled in to the first set of input boxes (Hostname / IP, Port, Username, and Password)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            //Disable form controls
            setInputEnabled(false);

            // Clear combobox no matter what:
            items.Clear();

            // only enable if successful!  (in the loop below)
            setSaveEnabled(false); 

            // Query the server and grab new values for combo box or show error message and leave it empty:
            using(MySqlConnection conn = new MySqlConnection(connectionString))
            using(MySqlCommand command = new MySqlCommand("show databases;", conn))
            {
                try
                {
                    conn.Open();

                    MySqlDataReader reader;
                    reader = command.ExecuteReader();

                    // for all results (databases) found:
                    bool found = false;
                    for (int c = 0; reader.Read(); c++)
                    {
                        // add them to the Items list passed in:
                        string next = reader.GetString(0);
                        items.Add(next);

                        // If this is the last used database that was saved from the last application run, USE IT!
                        //  or if it wasn't, but it is the default database name, USE IT!
                        //  but always take last used OVER default.
                        if (next == Properties.Settings.Default.connectionStringDatabase
                            || (next == Properties.Settings.Default.connectionStringDefaultDatabase && !found))
                        {
                            // set as true so that it ignores the default database name if it shows up later in the list
                            found = true;
                            this.databaseComboBox.SelectedIndex = c;
                        }
                    }
                    reader.Close();

                    // enable OK button if successfully connected and have databases available:
                    setSaveEnabled(true);

                    // save these successful connection details in case you edit a box BEFORE hitting save and WITHOUT re-testing the connection:
                    this.lastSuccessfulHostnameIP = hostnameIPTextBox.Text;
                    this.lastSuccessfulPort = Convert.ToUInt16(portNumericUpDown.Value);
                    this.lastSuccessfulUsername = usernameTextBox.Text;
                    this.lastSuccessfulPassword = passwordTextBox.Text;

                    conn.Close();
                }
                catch (MySqlException ex) {
                    MessageBox.Show("MySQL Error Number " + ex.Number + ", " + ex.Message);

                    // DEBUG ONLY, otherwise you're showing the saved password to a random person:
                    //  (although it is saved on the computer somewhere and they COULD go find it)
                    //  MessageBox.Show(connectionString);
                }
            }
            
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

        /// <summary>
        /// Toggle "Enabled" property of the save button and database selection combobox.
        /// </summary>
        /// <param name="setTo">Pass true to enable and false to disable, defaults to true</param>
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
