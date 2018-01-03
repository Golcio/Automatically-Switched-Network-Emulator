using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientTSST8
{
    public class Receiver
    {
        Socket socket;
        MainWindow mainWindow;
        public int packetNumber = 0;
        public Receiver(string listeningIP, string listeningPort, MainWindow mainWindow)
        {
            int port;
            this.mainWindow = mainWindow;
            if (Int32.TryParse(listeningPort, out port))
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(listeningIP), port);
                socket.Bind(ipEnd);
                socket.Listen(0);
                socket = socket.Accept();
            }
         }
        
        public void receive()
        {
            while (true)
            {
                try
                {
                    byte[] Buffer = new byte[socket.SendBufferSize];
                    int readByte = socket.Receive(Buffer);
                    byte[] rData = new byte[readByte];
                    Array.Copy(Buffer, rData, readByte);

                    byte[] ifPackageBytes = new byte[4];
                    for (int i = 4; i < 8; i++)
                    {
                        ifPackageBytes[i - 4] = rData[i];
                    }
                    int ifPackage = BitConverter.ToInt32(ifPackageBytes, 0);

                    if (ifPackage == 0)
                    {
                        byte[] packetBytes = new byte[rData.Length - 4];
                        for (int i = 0; i < 4; i++)
                        {
                            packetBytes[i] = rData[i];
                        }
                        for (int i = 8; i < rData.Length; i++)
                        {
                            packetBytes[i - 4] = rData[i];
                        }
                        DTOs.Packet packet = new DTOs.Packet();
                        packet = DTOs.Packet.ByteToPacket(rData, 4);
                        string message = packet.Message;
                        int label = packet.Labels.Peek();
                        mainWindow.Invoke(new Action(delegate () { mainWindow.writeReceipt("Odebrana wiadomość: " + message + " nr " + (packetNumber + 1).ToString() + " z etykietą " + label + "\n"); }));
                        packetNumber++;
                    }

                    else if (ifPackage == 1)
                    {
                        Queue<byte[]> packets = new Queue<byte[]>();
                        DTOs.Package package = DTOs.Package.ByteToPackage(rData, 8);
                        int g = package.packageQueue.Count;
                        for (int i = 0; i < g; i++)
                        {
                            byte[] receivedData = package.packageQueue.Dequeue();
                            for (int j = 0; j < 4; j++)
                            {
                                receivedData[j] = rData[j];
                            }
                            packets.Enqueue(receivedData);
                        }
                        while (packets.Count!= 0)
                        {
                            DTOs.Packet packet = new DTOs.Packet();
                            packet = DTOs.Packet.ByteToPacket(packets.Dequeue(), 4);
                            string message = packet.Message;
                            int label = packet.Labels.Peek();
                            mainWindow.Invoke(new Action(delegate () { mainWindow.writeReceipt("Odebrana wiadomość: " + message + " nr " + (packetNumber + 1).ToString() + " z etykietą " + label + "\n"); }));
                            packetNumber++;
                        }
                    }

                }
                catch (SocketException ex)
                {

                }
            }
        }

    }
}
