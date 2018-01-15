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
    public partial class AddNewEntryRouter : Form
    {
        NetworkManagerMain nmm;
        string nodeName;

        public AddNewEntryRouter(NetworkManagerMain nmm, string nodeName)
        {
            this.nmm = nmm;
            this.nodeName = nodeName;
            InitializeComponent();
            fillTable();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(textBox1.Text + "_" + textBox2.Text + "_" + textBox3.Text + "_" + textBox4.Text);
            if (textBox5.Text.Length > 0)
                sb.Append("_" + textBox5.Text);
            string newEntry = sb.ToString();
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
                dataGridView1.Rows[n].Cells[3].Value = split[3];
                if (split.Length == 5)
                    dataGridView1.Rows[n].Cells[4].Value = split[4];
            }
        }
    }
}
