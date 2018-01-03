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
    public class Sender
    {
        Socket socket;
        public Sender(string destinationIP, string destinationPort)
        {
            int port;
            if (Int32.TryParse(destinationPort, out port))
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(destinationIP), port);
                while (true)
                {
                    try
                    {
                        socket.Connect(ipEnd);
                        break;
                    }
                    catch (SocketException ex)
                    {

                    }
                }


            }
        }
        public void send(string destination, string numberOfPacket, string sendingInterval, string source, string clientName, string label)
        {
            int destinationPort, sourcePort;
            Int32.TryParse(destination, out destinationPort);
            Int32.TryParse(source, out sourcePort);
            string[] stringTable = { "woda gazowana", "rum", "mięta", "lód", "brązowy cukier", "limonka" };
            int miliseconds = Int32.Parse(sendingInterval);
            int number = Int32.Parse(numberOfPacket);
            for (int i = 0; i < number; i++)
            {
                Random rnd = new Random();
                int stringNumber = rnd.Next(1, stringTable.Length);
                DTOs.Packet pakiet = new DTOs.Packet(stringTable[stringNumber], "127.0.0.1", sourcePort, "127.0.0.1", destinationPort, Int32.Parse(label));
                byte[] intBytes = BitConverter.GetBytes(sourcePort);
                byte[] result = intBytes;

                byte[] packet = DTOs.Packet.PacketToByte(pakiet);
                byte[] newArray = new byte[packet.Length + 8];
                packet.CopyTo(newArray, 8);
                for (int j = 0; j < 4; j++)
                {
                    newArray[j] = result[j];
                }
                byte[] ifPackage = BitConverter.GetBytes(0);
                for (int j = 4; j < 8; j++)
                {
                    newArray[j] = ifPackage[j-4];
                }
                packet = newArray;
                socket.Send(packet);
                Thread.Sleep(miliseconds);
            }
        }
    }
}
