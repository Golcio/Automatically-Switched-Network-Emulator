using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network_Manager
{
    public partial class AddNewEntryClient : Form
    {
        NetworkManagerMain nmm;
        string nodeName;

        public AddNewEntryClient(NetworkManagerMain nmm, string nodeName)
        {
            this.nmm = nmm;
            this.nodeName = nodeName;
            InitializeComponent();
            fillTable();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string newEntry = textBox1.Text + "_" + textBox2.Text + "_" + textBox3.Text;
            nmm.tables[nodeName].Add(newEntry);
            nmm.sendClientTable(nodeName);
            fillTable();
        }

        private void fillTable()
        {
            dataGridView1.Rows.Clear();
            for (int i = 0; i < nmm.tables[nodeName].Count; i++)
            {
                int n = dataGridView1.Rows.Add();
                string[] split = nmm.tables[nodeName][i].Split('_');
                dataGridView1.Rows[n].Cells[0].Value = split[0];
                dataGridView1.Rows[n].Cells[1].Value = split[1];
                dataGridView1.Rows[n].Cells[2].Value = split[2];
            }
        }
    }
}
