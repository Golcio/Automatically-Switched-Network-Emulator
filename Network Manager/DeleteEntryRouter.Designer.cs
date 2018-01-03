using System.Drawing;

namespace Network_Manager
{
    partial class DeleteEntryRouter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.InputPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InputLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OutputPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OutputLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TunelStart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.InputPort,
            this.InputLabel,
            this.OutputPort,
            this.OutputLabel,
            this.TunelStart});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(543, 447);
            this.dataGridView1.TabIndex = 1;
            // 
            // InputPort
            // 
            this.InputPort.HeaderText = "Port we.";
            this.InputPort.Name = "InputPort";
            // 
            // InputLabel
            // 
            this.InputLabel.HeaderText = "Etykieta we.";
            this.InputLabel.Name = "InputLabel";
            // 
            // OutputPort
            // 
            this.OutputPort.HeaderText = "Port wy.";
            this.OutputPort.Name = "OutputPort";
            // 
            // OutputLabel
            // 
            this.OutputLabel.HeaderText = "Etykieta wy.";
            this.OutputLabel.Name = "OutputLabel";
            // 
            // TunelStart
            // 
            this.TunelStart.HeaderText = "Pocz. tunelu";
            this.TunelStart.Name = "TunelStart";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(184, 465);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 38);
            this.button1.TabIndex = 3;
            this.button1.Text = "Usuń wybrany wiersz";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // DeleteEntryRouter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 515);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "DeleteEntryRouter";
            this.Text = "Usuwanie wpisu z tablicy";
            this.Icon = new Icon("manager2.ico");
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn InputPort;
        private System.Windows.Forms.DataGridViewTextBoxColumn InputLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn OutputPort;
        private System.Windows.Forms.DataGridViewTextBoxColumn OutputLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn TunelStart;
        private System.Windows.Forms.Button button1;
    }
}