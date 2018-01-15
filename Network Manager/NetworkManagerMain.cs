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

namespace Network_Manager
{
    public partial class NetworkManagerMain : Form
    {
        public List<string> activeNodes = new List<string>();
        public Dictionary<string, List<string>> tables = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> clientTables = new Dictionary<string, List<string>>();
        public Dictionary<string, int> nodes = new Dictionary<string, int>();
        public Dictionary<String, int> listeners = new Dictionary<String, int>();
        public Dictionary<String, IPEndPoint> outputIPEndPoints = new Dictionary<String, IPEndPoint>();
        public Dictionary<String, Socket> inputSockets = new Dictionary<String, Socket>();
        public Dictionary<String, Socket> outputSockets = new Dictionary<String, Socket>();
        static Dictionary<String, bool> initiallyConnected = new Dictionary<string, bool>();


        public string chosenNode;
        public string chosenAction;
        public NetworkManagerMain()
        {
            InitializeComponent();
            this.comboBox2.Items.AddRange((object[])new string[] { "Dodaj wpis w tablicy", "Usuń wpis w tablicy" });
            Thread.Sleep(2000);
            cb1fill();
            nmsMain();
        }
        public void goodNews(string text)
        {
            this.richTextBox1.SelectionColor = Color.Green;
            this.richTextBox1.SelectedText += text;
            this.richTextBox1.ScrollToCaret();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            chosenNode = this.comboBox1.Text;
            chosenAction = this.comboBox2.Text;
            if ((!chosenNode.Equals("")) && (!chosenAction.Equals("")))
            {
                if (chosenAction == "Dodaj wpis w tablicy")
                {
                    if (chosenNode.Contains("client"))
                    {
                        AddNewEntryClient newEntry = new AddNewEntryClient(this, chosenNode);
                        newEntry.Show();
                    }
                    else if (chosenNode.Contains("networkNode"))
                    {
                        AddNewEntryRouter newEntry = new AddNewEntryRouter(this, chosenNode);
                        newEntry.Show();
                    }
                }
                else if (chosenAction == "Usuń wpis w tablicy")
                {
                    if (chosenNode.Contains("client"))
                    {
                        DeleteEntryClient deleteEntry = new DeleteEntryClient(this, chosenNode);
                        deleteEntry.Show();
                    }
                    else if (chosenNode.Contains("networkNode"))
                    {
                        DeleteEntryRouter deleteEntry = new DeleteEntryRouter(this, chosenNode);
                        deleteEntry.Show();
                    }

                }
            }
        }

        public void nmsMain()
        {
            parseConfigFile();
            parseTables();
            createOutputSockets();
            createInputSockets();
        }

        public void parseConfigFile()
        {
            var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\config/NetworkManager.txt";
            try
            {
                rtb1SetColor(Color.Black);
                rtb1SetText("Parsowanie pliku konfiguracyjnego systemu zarządzania...");
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    string currentParsing = null;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Equals("nodes"))
                            currentParsing = "nodes";
                        else
                        {
                            if (currentParsing.Equals("nodes"))
                            {
                                string[] splitArray = line.Split('_');
                                int nodeport = Int32.Parse(splitArray[1]);
                                nodes.Add(splitArray[0], nodeport);
                                initiallyConnected.Add(splitArray[0], false);
                                outputIPEndPoints.Add(splitArray[0], new IPEndPoint(IPAddress.Parse("127.0.0.1"), nodeport));
                                outputSockets.Add(splitArray[0], new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                                int listener = Int32.Parse(splitArray[2]);
                                inputSockets.Add(splitArray[0], new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                                listeners.Add(splitArray[0], listener);
                            }
                        }

                    }
                }
                rtb1SetColor(Color.Black);
                rtb1SetText("Parsowanie pliku zakończone.");
            }
            catch (Exception e)
            {
                rtb1SetColor(Color.Black);
                rtb1SetText(e.ToString());
            }
        }

        public void parseTables()
        {
            foreach (KeyValuePair<string, int> kvp in nodes)
            {
                if (kvp.Key.Contains("client"))
                {
                    parseTable(kvp.Key);
                }
                else if (kvp.Key.Contains("networkNode"))
                {
                    parseTable(kvp.Key);
                }
            }
        }

        public void parseTable(string nodeName)
        {
            List<string> table = new List<string>();
            var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\config/" + nodeName + ".txt";
            try
            {
                rtb1SetColor(Color.Black);
                rtb1SetText("Parsowanie pliku węzła " + nodeName + "...");
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        table.Add(line);
                    }
                }
                if (tables.ContainsKey(nodeName))
                    tables.Remove(nodeName);

                tables.Add(nodeName, table);

                rtb1SetColor(Color.Black);
                rtb1SetText("Parsowanie pliku zakończone.");
            }
            catch (Exception e)
            {
                rtb1SetColor(Color.Black);
                rtb1SetText(e.ToString());
            }
        }

        public void parseClientTable(string nodeName)
        {
            List<string> table = new List<string>();
            var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\config/" + nodeName + ".txt";
            try
            {
                rtb1SetColor(Color.Black);
                rtb1SetText("Parsowanie pliku węzła " + nodeName + "...");
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        table.Add(line);
                    }
                }
                if (tables.ContainsKey(nodeName))
                    tables.Remove(nodeName);

                clientTables.Add(nodeName, table);

                rtb1SetColor(Color.Black);
                rtb1SetText("Parsowanie pliku zakończone.");
            }
            catch (Exception e)
            {
                rtb1SetColor(Color.Black);
                rtb1SetText(e.ToString());
            }
        }

        public void createInputSockets()
        {
            foreach (KeyValuePair<string, Socket> kvp in inputSockets)
            {
                Thread inputConnectionThread = new Thread(() => initializeInputSocket(kvp.Value, listeners[kvp.Key], kvp.Key));
                inputConnectionThread.Start();
            }
        }
        
        public void initializeInputSocket(Socket listenerSocket, int port, string nodeName)
        {
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            listenerSocket.Bind(endPoint);
            rtb1SetColor(Color.Black);

            while (true)
            {
                listenerSocket.Listen(0);
                Socket receiveSocket = listenerSocket.Accept();
                try
                {
                    byte[] buffer = new byte[receiveSocket.SendBufferSize];
                    int readByte;
                    do
                    {
                        readByte = receiveSocket.Receive(buffer);
                        byte[] rData = new byte[readByte];
                        Array.Copy(buffer, rData, readByte);

                        string message = System.Text.Encoding.UTF8.GetString(rData);
                        if (message.Contains("Node"))
                        {
                            string[] splitArray = message.Split('_');
                            //rtb1SetColor(Color.Green);
                            string node = splitArray[0];
                            //rtb1SetText(node + " wstał.");
                            activeNodes.Add(node);
                            rtb2Fill();
                            cb1fill();
                            Thread.Sleep(500);
                            sendTable(node);
                        }
                        else if (message.Contains("client"))
                        {
                            string[] splitArray = message.Split('_');
                            //rtb1SetColor(Color.Green);
                            string node = splitArray[0];
                            //rtb1SetText(node + " wstał.");
                            activeNodes.Add(node);
                            rtb2Fill();
                            cb1fill();
                            Thread.Sleep(500);
                            sendClientTable(node);
                        }
                    }
                    while (readByte > 0);

                }
                catch (SocketException ex)
                {
                    rtb1SetColor(Color.DarkRed);
                    rtb1SetText("Połączenie z " + nodeName + " zerwane. Ponawiam połączenie...");
                    if (initiallyConnected[nodeName] == true)
                    {
                        if (activeNodes.Contains(nodeName))
                        {
                            activeNodes.Remove(nodeName);
                            rtb2Fill();
                            cb1fill();
                        }

                        outputSockets[nodeName].Close();
                        outputSockets[nodeName] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        Thread outputConnectionThread = new Thread(() => connectWithNode(outputSockets[nodeName], outputIPEndPoints[nodeName]));
                        outputConnectionThread.Start();

                        listenerSocket.Close();
                        Thread inputConnectionThread = new Thread(() => initializeInputSocket(listenerSocket, port, nodeName));
                        inputConnectionThread.Start();
                    }
                    break;
                }
            }
        }

        public void createOutputSockets()
        {
            foreach (KeyValuePair<string, Socket> kvp in outputSockets)
            {
                Thread outputConnectionThread = new Thread(() => connectWithNode(kvp.Value, outputIPEndPoints[kvp.Key]));
                outputConnectionThread.Start();
            }
        }

        private void connectWithNode(Socket socket, IPEndPoint ipEnd)
        {
            string node = nodes.Where(kvp => kvp.Value == ipEnd.Port).Select(kvp => kvp.Key).FirstOrDefault();
            rtb1SetColor(Color.Goldenrod);
            rtb1SetText("Sprawdzam połączenie z: " + node);
            while (true)
            {
                try
                {
                    socket.Connect(ipEnd);
                    rtb1SetColor(Color.DarkGreen);
                    rtb1SetText("Połączono z: " + node);
                    if (initiallyConnected[node] == false)
                        initiallyConnected[node] = true;
                    break;
                }
                catch (SocketException ex)
                {

                }
            }
        }

        public void sendTable(String nodeName)
        {
            rtb1SetColor(Color.SteelBlue);
            Socket output = outputSockets[nodeName];
            Thread.Sleep(100);
            rtb1SetText("Przesyłam  do " + nodeName + " tablicę kierowania.");

            try
            {
                output.Send(System.Text.Encoding.UTF8.GetBytes(buildStringFromTable(nodeName)));
            }
            catch (SocketException ex)
            {
                rtb1SetColor(Color.Red);
                rtb1SetText(nodeName + " nie odpowiada.");
                activeNodes.Remove(nodeName);
                rtb2Fill();
                cb1fill();
            }
        }

        public void sendClientTable(String nodeName)
        {
            rtb1SetColor(Color.SteelBlue);
            Socket output = outputSockets[nodeName];
            Thread.Sleep(100);
            rtb1SetText("Przesyłam  do " + nodeName + " tablicę kierowania.");

            try
            {
                output.Send(System.Text.Encoding.UTF8.GetBytes(buildStringFromTable(nodeName)));
            }
            catch (SocketException ex)
            {
                rtb1SetColor(Color.Red);
                rtb1SetText(nodeName + " nie odpowiada.");
                activeNodes.Remove(nodeName);
                rtb2Fill();
                cb1fill();
            }
        }

        public String buildStringFromTable(String nodeName)
        {
            string response = null;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tables[nodeName].Count; i++)
            {
                sb.Append(tables[nodeName][i]);
                if (i != tables[nodeName].Count - 1)
                {
                    sb.Append("|");
                }
            }
            response = sb.ToString();

            if (response.Length == 0)
            {
                response = "empty";
            }
            return response;
        }

        public String buildStringFromClientTable(String nodeName)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tables[nodeName].Count; i++)
            {
                sb.Append(tables[nodeName][i]);
                if (i != tables[nodeName].Count - 1)
                {
                    sb.Append("|");
                }
            }
            return sb.ToString();
        }

        public String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }

        public void rtb2Fill()
        {
            try
            {
                if (richTextBox2.InvokeRequired)
                {
                    Invoke((MethodInvoker)(() => fillWithActiveNodes()));
                }
                else
                {
                    fillWithActiveNodes();
                }
            }
            catch (Exception e) { }
        }

        public void fillWithActiveNodes()
        {
            richTextBox2.Clear();
            for (int i = 0; i < activeNodes.Count; i++)
            {
                richTextBox2.AppendText(activeNodes[i] + "\n");
            }
        }

        public void cb1fill()
        {
            try
            {
                if (comboBox1.InvokeRequired)
                {
                    Invoke((MethodInvoker)(() => fillComboBoxWithNodes()));
                }
                else
                {
                    fillComboBoxWithNodes();
                }
            }
            catch (Exception e) { }

        }

        public void fillComboBoxWithNodes()
        {
            comboBox1.Items.Clear();
            for (int i = 0; i < activeNodes.Count; i++)
            {
                comboBox1.Items.Add(activeNodes[i]);
            }
        }


        public void rtb1SetText(string text)
        {
            try
            {
                String fullMessage = GetTimestamp(DateTime.Now) + "\t" + text + "\n";
                if (richTextBox1.InvokeRequired)
                {
                    Invoke((MethodInvoker)(() => richTextBox1.AppendText(fullMessage)));
                    Invoke((MethodInvoker)(() => richTextBox1.ScrollToCaret()));
                }
                else
                {
                    richTextBox1.ScrollToCaret();
                    richTextBox1.AppendText(fullMessage);
                }
            }
            catch (Exception e) { }

        }

        public void rtb1SetColor(Color color)
        {
            /*
            try
            {
                if (richTextBox1.InvokeRequired)
                {
                    richTextBox1.Invoke((MethodInvoker)delegate
                    {
                        richTextBox1.SelectionColor = color;
                    });
                }
            }
            catch (Exception e) { }
            */

            try
            {
                if (richTextBox1.InvokeRequired)
                {
                    Invoke((MethodInvoker)(() => rtbColor(color)));
                }
                else
                {
                    rtbColor(color);
                }
            }
            catch (Exception e) { }

        }
        public void rtbColor(Color color)
        {
            richTextBox1.SelectionColor = color;
        }

    }
    
}
