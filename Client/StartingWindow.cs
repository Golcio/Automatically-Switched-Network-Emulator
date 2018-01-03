using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientTSST8
{
    public partial class StartingWindow : Form
    {
        public string chosenName;
        Reader reader;
        //Agent agent;
        public StartingWindow(string path, Reader reader)
        {
            this.reader = reader;
            InitializeComponent();
            string[] names = reader.getClientNames();
            this.comboBox1.Items.AddRange((object[])names);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            chosenName = this.comboBox1.Text;
            reader.chooseDefinition(chosenName);
            //agent.agentSender(chosenName);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        private void StartingWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Application.Exit();
        }
    }
}
