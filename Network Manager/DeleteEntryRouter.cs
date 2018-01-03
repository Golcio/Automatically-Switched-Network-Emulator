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
    public partial class DeleteEntryRouter : Form
    {
        NetworkManagerMain nmm;
        string nodeName;

        public DeleteEntryRouter(NetworkManagerMain nmm, string nodeName)
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
            string value4 = null;
            string value5 = null;

            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                value1 = row.Cells[0].Value.ToString();
                value2 = row.Cells[1].Value.ToString();
                value3 = row.Cells[2].Value.ToString();
                value4 = row.Cells[3].Value.ToString();
                if (row.Cells[4].Value != null)
                    value5 = row.Cells[4].Value.ToString();
            }
            string entry;
            if (value5 != null)
                entry = value1 + "_" + value2 + "_" + value3 + "_" + value4 + "_" + value5;
            else
                entry = value1 + "_" + value2 + "_" + value3 + "_" + value4;

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
                dataGridView1.Rows[n].Cells[3].Value = split[3];
                if (split.Length == 5)
                    dataGridView1.Rows[n].Cells[4].Value = split[4];
            }
        }
    }
}
