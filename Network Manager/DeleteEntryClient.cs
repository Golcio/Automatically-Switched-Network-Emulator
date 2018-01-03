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
    public partial class DeleteEntryClient : Form
    {
        NetworkManagerMain nmm;
        string nodeName;

        public DeleteEntryClient(NetworkManagerMain nmm, string nodeName)
        {
            this.nmm = nmm;
            this.nodeName = nodeName;
            InitializeComponent();
            fillTable();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string value1 = null;
            string value2 = null;
            string value3 = null;

            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                value1 = row.Cells[0].Value.ToString();
                value2 = row.Cells[1].Value.ToString();
                value3 = row.Cells[2].Value.ToString();
            }

            string entry = value1 + "_" + value2 + "_" + value3;

            nmm.tables[nodeName].Remove(entry);
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
