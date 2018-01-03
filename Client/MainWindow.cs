using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientTSST8
{
    public partial class MainWindow : Form
    {
        private ComboBox comboBox1;
        private TextBox textBox1;
        private ComboBox comboBox2;
        private Button button1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private RichTextBox richTextBox1;
        private Label label6;
        public string destination, numberOfPackets, sendingInterval, sendingLabel;
        Sender sender;
        Reader reader;
        private Button button2;
        private Button button3;
        private Label label7;
        private ComboBox comboBox3;
        Thread senderThread;
        public MainWindow(Sender sender, Reader reader, string destinationPort)
        {
            this.sender = sender;
            this.reader = reader;
            InitializeComponent();
            string clientName = reader.getClientName();
            this.comboBox1.Items.AddRange((object[])reader.getFreeNames());
            this.comboBox2.Items.AddRange((object[])new string[] { "500", "1000" });
            richTextBox1.SelectionColor = Color.MediumOrchid;
            richTextBox1.SelectedText =("Witaj kliencie " + clientName + "\n");
            this.Text = "Client" + clientName;



        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(19, 29);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(195, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(19, 121);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(195, 20);
            this.textBox1.TabIndex = 1;
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(19, 166);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(195, 21);
            this.comboBox2.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(19, 196);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 28);
            this.button1.TabIndex = 3;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Do kogo wysyłamy?";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Ile pakietów?";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 146);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "W jakich odstępach?";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(220, 169);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "ms";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.Location = new System.Drawing.Point(284, 29);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(350, 267);
            this.richTextBox1.TabIndex = 9;
            this.richTextBox1.Text = "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(281, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Komunikaty";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(150, 196);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(64, 28);
            this.button2.TabIndex = 11;
            this.button2.Text = "Wyjście";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(507, 307);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(127, 36);
            this.button3.TabIndex = 12;
            this.button3.Text = "Wyczyść konsolę";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Z jaką etykietą?";
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(19, 73);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(195, 21);
            this.comboBox3.TabIndex = 14;
            this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            // 
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(646, 355);
            this.Controls.Add(this.comboBox3);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.comboBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            destination = this.comboBox1.Text;
            numberOfPackets = this.textBox1.Text;
            sendingInterval = this.comboBox2.Text;
            sendingLabel = this.comboBox3.Text;
            string clientName = reader.getClientName();
            //Ls status =  Agent.xd(destination);
            Agent.setDestinationPort(destination);
            if (clientName != "" && sendingLabel != "" && numberOfPackets != "" && sendingInterval != "")
            {
                senderThread = new Thread(() => this.sender.send(Agent.destinationPort, numberOfPackets, sendingInterval, reader.getChosenDefinition()[1], clientName, sendingLabel));
                senderThread.Start();
            }
            /*
            else
            {
                this.richTextBox1.SelectionColor = Color.Red;
                this.richTextBox1.SelectedText = "Nie można wysłać wiadomości, urządzenie docelowe jest nieosiągalne.\n";
                this.richTextBox1.ScrollToCaret();
            }
            */
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillLabelComboBox(comboBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void writeReceipt(string text)
        {
            this.richTextBox1.SelectionColor = Color.Green;
            this.richTextBox1.SelectedText += text;
            this.richTextBox1.ScrollToCaret();
        }
        public void writeNMSReceipt(string text)
        {
            this.richTextBox1.SelectionColor = Color.Red;
            this.richTextBox1.SelectedText += text;
            this.richTextBox1.ScrollToCaret();
        }

        public void fillLabelComboBox(string clientName)
        {
            comboBox3.Items.Clear();
            List<string> labels = Agent.getLabelList(clientName);
            for (int i = 0; i < labels.Count; i++)
            {
                comboBox3.Items.Add(labels[i]);
            }
        }
    }
}
