using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiggerSimulator
{
    public partial class Form1 : Form
    {
        string inputPort = null;
        List<String[]> connectionsToBreak = new List<string[]>();
        List<String[]> connectionsBroken = new List<string[]>();
        Dictionary<String, int> sendingPorts = new Dictionary<String, int>();
        public Form1()
        {
            InitializeComponent();
            parseConfigFile();
            fillConnectionsCombo();
            fillBrokenConnectionsCombo();
            Thread inputSocketThread = new Thread(() => initializeInputSocket());
            inputSocketThread.Start();
        }

        public void initializeInputSocket()
        {
            bool done = false;
            UdpClient listener = new UdpClient(Int32.Parse(inputPort));
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Int32.Parse(inputPort));
            string received_data;
            byte[] receive_byte_array;

            try
            {
                while (!done)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    string[] splitArray = received_data.Split('_');
                    if (splitArray[0].Equals("BreakConnection"))
                    {
                        string[] connection = new string[2];
                        connection[0] = splitArray[1];
                        connection[1] = splitArray[2];
                        for (int i = 0; i < connectionsToBreak.Count; i++)
                        {
                            if (connectionsToBreak[i][0].Equals(connection[0]))
                            {
                                if (connectionsToBreak[i][1].Equals(connection[1]))
                                {
                                    connectionsToBreak.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        connectionsBroken.Add(connection);
                        MethodInvoker inv = delegate
                        {
                            fillConnectionsCombo();
                            fillBrokenConnectionsCombo();
                        };
                        this.Invoke(inv);
                        writeToConsole("Łącze " + splitArray[1] + "-" + splitArray[2] + " zostało zniszczone.");
                    }
                    else if (splitArray[0].Equals("ConnectionRestored"))
                    {
                        string[] connection = new string[2];
                        connection[0] = splitArray[1];
                        connection[1] = splitArray[2];
                        for (int i = 0; i < connectionsBroken.Count; i++)
                        {
                            if (connectionsBroken[i][0].Equals(connection[0]))
                            {
                                if (connectionsBroken[i][1].Equals(connection[1]))
                                {
                                    connectionsBroken.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        connectionsToBreak.Add(connection);
                        MethodInvoker inv = delegate
                        {
                            fillConnectionsCombo();
                            fillBrokenConnectionsCombo();
                        };
                        this.Invoke(inv);
                        writeToConsole("Łącze " + splitArray[1] + "-" + splitArray[2] + " zostało naprawione.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
        }

        public void fillConnectionsCombo()
        {
            comboBox1.Items.Clear();
            foreach (String[] connection in connectionsToBreak)
            {
                string text = connection[0] + "-" + connection[1];
                comboBox1.Items.Add(text);
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        public void fillBrokenConnectionsCombo()
        {
            comboBox2.Items.Clear();
            foreach (String[] connection in connectionsBroken)
            {
                string text = connection[0] + "-" + connection[1];
                comboBox2.Items.Add(text);
            }
            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;
        }

        public void parseConfigFile()
        {
            var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\connections.txt";
            try
            {
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    string currentParsing = null;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Equals("configuration"))
                            currentParsing = "configuration";
                        else if (line.Equals("nodes"))
                            currentParsing = "nodes";
                        else if (line.Equals("connections"))
                            currentParsing = "connections";
                        else if (line.Equals("inputPort"))
                            currentParsing = "inputPort";
                        else
                        {
                            if (currentParsing.Equals("nodes"))
                            {
                                string[] splitArray = line.Split('_');
                                int sendingPort = Int32.Parse(splitArray[1]);
                                sendingPorts.Add(splitArray[0], sendingPort);
                            }
                            else if (currentParsing.Equals("connections"))
                            {
                                string[] splitArray = line.Split('_');
                                connectionsToBreak.Add(splitArray);
                            }
                            else if (currentParsing.Equals("inputPort"))
                            {
                                inputPort = line;
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) { 
                writeToConsole("Nie wybrano połączenia.");
                return;
            }
            string connection = comboBox1.SelectedItem.ToString();
            string[] split = connection.Split('-');
            string output = "BreakConnection_" + split[0] + "_" + split[1];
            string[] split2 = split[0].Split('.');
            string node = split2[0];
            string destination = sendingPorts[node].ToString();
            Send(output, destination);
            writeToConsole("Wysyłam żądanie zniszczenia połączenia " + connection + "...");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                writeToConsole("Nie wybrano połączenia.");
                return;
            }
            string connection = comboBox2.SelectedItem.ToString();
            string[] split = connection.Split('-');
            string output = "RestoreConnection_" + split[0] + "_" + split[1];
            string[] split2 = split[0].Split('.');
            string node = split2[0];
            string destination = sendingPorts[node].ToString();
            Send(output, destination);
            writeToConsole("Wysyłam żądanie naprawy połączenia " + connection + "...");
        }

        public void Send(String message, String destination)
        {
            Boolean exception_thrown = false;
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Int32.Parse(destination));
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);

            try
            {
                sending_socket.SendTo(send_buffer, sending_end_point);
            }
            catch (Exception send_exception)
            {
                exception_thrown = true;
            }
        }

        public void writeToConsole(string text)
        {
            MethodInvoker inv = delegate
            {
                this.richTextBox1.SelectedText += text + "\n";
                this.richTextBox1.ScrollToCaret();
            };
            this.Invoke(inv);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
