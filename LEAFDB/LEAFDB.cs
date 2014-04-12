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
    public partial class LEAFDB : Form
    {
        public LEAFDB()
        {
            InitializeComponent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Verify discarding of changes?
            // TODO: Close Database Connection
            this.Close();
        }

        private void databaseConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MySQLConnectionDetailsForm connectionDetails = new MySQLConnectionDetailsForm();

            // TODO soon: Load existing / saved data from last usage
            // TODO soon: Verify you still have access to database, but don't force save if database unavailable / no access
            
            // Don't wait for dialog result as it's handled in the dialog...
            connectionDetails.ShowDialog();

            // TODO soon: run function to (de)activate forms (after confirming connection)
        }
    }
}
