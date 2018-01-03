using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientTSST8
{
    public class Agent
    {
        Socket agentSendSocket;
        Socket agentReceiveSocket;
        //MainWindow mainWindow;
        public string response;
        public static string[,] labelTable;
        public static string destinationPort = null;
        public static string label;
        public static string[] table;
        public Agent(string inputNMS, string outputNMS, string clientName)
        {
            int sendNMSPort;
            if (Int32.TryParse(outputNMS, out sendNMSPort))
            {
                agentSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendNMSPort);
                agentSendSocket.Connect(ipEnd);
                agentSendSocket.Send(Encoding.UTF8.GetBytes("client" + clientName));
            }
            int receiveNMSPort;
            if (Int32.TryParse(inputNMS, out receiveNMSPort))
            {
                ///receive
                agentReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEnd1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), receiveNMSPort);
                agentReceiveSocket.Bind(ipEnd1);
                agentReceiveSocket.Listen(0);
                agentReceiveSocket = agentReceiveSocket.Accept();
            }

        }

        public void agentReceiver(string clientName)
        {

            while (true)
            {
                try
                {
                    byte[] Buffer = new byte[agentReceiveSocket.SendBufferSize];
                    int readByte = agentReceiveSocket.Receive(Buffer);
                    byte[] rData = new byte[readByte];
                    Array.Copy(Buffer, rData, readByte);
                    response = Encoding.UTF8.GetString(rData);

                    if (response.Length > 0)
                    {
                        table = response.Split('|');
                        labelTable = new string[table.Length, 3];
                        for (int i = 0; i < table.Length; i++)
                        {
                            labelTable[i, 0] = table[i].Split('_')[0];
                            labelTable[i, 1] = table[i].Split('_')[1];
                            labelTable[i, 2] = table[i].Split('_')[2];
                        }

                        for (int i = 0; i < table.Length; i++)
                        {
                            Console.WriteLine(table[i] + " " + table[i] + " " + table[i]);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    //mainWindow.Invoke(new Action(delegate () { mainWindow.writeNMSReceipt("Problem z połączeniem NMS"); }));
                }
            }

        }
        public static List<string> getLabelList(string clientName)
        {
            List<string> labels = new List<string>();
            Thread.Sleep(200);
            for (int i = 0; i < table.Length; i++)
            {
                if (clientName == labelTable[i, 0])
                {
                    labels.Add(labelTable[i, 1]);
                }
            }
            return labels;
        }

        public static void setDestinationPort(string clientName)
        {
            for (int i = 0; i < table.Length; i++)
            {
                if (clientName == labelTable[i, 0])
                {
                    destinationPort = labelTable[i, 2];
                }
            }
        }
    }
}
