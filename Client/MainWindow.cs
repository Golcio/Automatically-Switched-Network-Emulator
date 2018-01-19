using Client;
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
        private Button button4;
        private Button button5;
        private Label label7;
        private Label label8;
        private TextBox textBox3;
        private Label label10;
        Thread senderThread;

        CallingPartyCallController callingPartyCallController;
        public string label = "0";
        public bool connected = false;
        public string connectedID = null;
        public string myName = null;
        private Label label9;
        public string nccport = null;

        public MainWindow(Sender sender, Reader reader, string cpccinput, string nccport, string myid)
        {
            this.sender = sender;
            this.reader = reader;
            this.nccport = nccport;
            string clientName = reader.getClientName();
            myName = clientName;
            InitializeComponent();
            if (myName.Equals("A"))
            {
                this.comboBox1.Items.AddRange(new object[] {
                "clientB",
                "clientC"});
            }
            else if (myName.Equals("B"))
            {
                this.comboBox1.Items.AddRange(new object[] {
                "clientA",
                "clientC"});
            }
            else if (myName.Equals("C"))
            {
                this.comboBox1.Items.AddRange(new object[] {
                "clientA",
                "clientB"});
            }
            this.comboBox2.Items.AddRange((object[])new string[] { "500", "1000" });
            richTextBox1.SelectionColor = Color.MediumOrchid;
            richTextBox1.SelectedText = ("Witaj kliencie " + clientName + "\n");
            this.Text = "Client" + clientName;
            callingPartyCallController = new CallingPartyCallController(cpccinput, nccport, myid, this);
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
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(19, 29);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(200, 24);
            this.comboBox1.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(19, 275);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(195, 22);
            this.textBox1.TabIndex = 1;
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(19, 320);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(195, 24);
            this.comboBox2.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(19, 361);
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
            this.label1.Size = new System.Drawing.Size(0, 17);
            this.label1.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Dostępni użytkownicy";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 255);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Ile pakietów?";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 300);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "W jakich odstępach?";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(220, 323);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "ms";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.Location = new System.Drawing.Point(284, 29);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(350, 360);
            this.richTextBox1.TabIndex = 9;
            this.richTextBox1.Text = "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(281, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 17);
            this.label6.TabIndex = 10;
            this.label6.Text = "Komunikaty";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(130, 361);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(84, 28);
            this.button2.TabIndex = 11;
            this.button2.Text = "Wyjście";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(507, 395);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(127, 36);
            this.button3.TabIndex = 12;
            this.button3.Text = "Wyczyść konsolę";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(19, 104);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(103, 28);
            this.button4.TabIndex = 13;
            this.button4.Text = "Połącz";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Enabled = false;
            this.button5.Location = new System.Drawing.Point(128, 104);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(91, 28);
            this.button5.TabIndex = 14;
            this.button5.Text = "Rozłącz";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 146);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 17);
            this.label7.TabIndex = 15;
            this.label7.Text = "Połączono z:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(111, 146);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(108, 17);
            this.label8.TabIndex = 16;
            this.label8.Text = "brak połączenia";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(19, 76);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(152, 22);
            this.textBox3.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 56);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(177, 17);
            this.label10.TabIndex = 20;
            this.label10.Text = "Wymagana przepustowość";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(177, 79);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 17);
            this.label9.TabIndex = 21;
            this.label9.Text = "Mbps";
            // 
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(646, 443);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
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
            sendingLabel = label;
            string clientName = reader.getClientName();
            //Ls status =  Agent.xd(destination);
            //Agent.setDestinationPort(destination);
            if (clientName != "" && sendingLabel != "" && numberOfPackets != "" && sendingInterval != "")
            {
                senderThread = new Thread(() => this.sender.send(nccport, numberOfPackets, sendingInterval, reader.getChosenDefinition()[1], clientName, sendingLabel));
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

        private void button3_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string destinationid = this.comboBox1.Text;
            string capacity = this.textBox3.Text;
            if (destinationid.Length > 0 && capacity.Length > 0)
            {
                callingPartyCallController.CallRequest(destinationid, capacity);
                string text = "Łączę z " + destinationid + "...\n";
                this.richTextBox1.SelectedText += text;
                this.richTextBox1.ScrollToCaret();
            }
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

        private void button5_Click(object sender, EventArgs e)
        {
            callingPartyCallController.CallTeardown(connectedID);
        }

        public void writeNMSReceipt(string text)
        {
            this.richTextBox1.SelectionColor = Color.Red;
            this.richTextBox1.SelectedText += text;
            this.richTextBox1.ScrollToCaret();
        }

        public void connectedProcedure(string conID)
        {
            if (connected == false)
            {
                this.connectedID = conID;
                this.connected = true;
                string text = "Połączono z " + connectedID + ".\n";
                MethodInvoker inv = delegate
                {
                    this.comboBox1.Enabled = false;
                    this.textBox3.Enabled = false;
                    this.button4.Enabled = false;
                    this.button5.Enabled = true;
                    this.button1.Enabled = true;
                    this.label8.Text = connectedID;
                    this.richTextBox1.SelectedText += text;
                    this.richTextBox1.ScrollToCaret();
                };
                this.Invoke(inv);
            }
        }

        public void disconnectedProcedure()
        {
            if (connected == true)
            {
                MethodInvoker inv = delegate
                {
                    this.comboBox1.Enabled = true;
                    this.textBox3.Enabled = true;
                    this.button4.Enabled = true;
                    this.button5.Enabled = false;
                    this.button1.Enabled = false;
                    this.label8.Text = "brak połączenia";
                };
                this.Invoke(inv);
            }
        }

        public void writeToConsole(string text)
        {
            string text2 = text + ".\n";
            MethodInvoker inv = delegate
            {
                this.comboBox1.Enabled = false;
                this.textBox3.Enabled = false;
                this.button4.Enabled = false;
                this.button5.Enabled = true;
                this.button1.Enabled = true;
                this.label8.Text = connectedID;
                this.richTextBox1.SelectedText += text;
                this.richTextBox1.ScrollToCaret();
            };
            this.Invoke(inv);
        }
    }
}
