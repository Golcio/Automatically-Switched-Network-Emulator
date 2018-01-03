using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Router
{
    class RouterReceiver
    {
        Socket socket;

        public RouterReceiver(string listeningIP, string listeningPort)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tSocket odbierający utworzony.");

            int port;
            if (Int32.TryParse(listeningPort, out port))
            {
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(listeningIP), port);
                socket.Bind(ipEnd);
                socket.Listen(0);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tNasłuchiwanie...");
                socket = socket.Accept();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tSocket odbierający nawiązał połączenie.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tPort parsing failed.");
                Console.ReadKey();
            }
        }

        public void socketReceive(Queue<byte[]> packetQueue, List<String[]> switchTables)
        {
            while (true)
            {
                byte[] buffer = new byte[socket.SendBufferSize];
                int readByte;
                readByte = socket.Receive(buffer);
                byte[] receivedData = new byte[readByte];
                Array.Copy(buffer, receivedData, readByte);
                byte[] currentPort = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    currentPort[i] = receivedData[i];
                }
                int port = BitConverter.ToInt32(currentPort, 0);
                byte[] ifPackageBytes = new byte[4];
                for (int i = 4; i < 8; i++)
                {
                    ifPackageBytes[i - 4] = receivedData[i];
                }
                int ifPackage = BitConverter.ToInt32(ifPackageBytes, 0);

                Console.ForegroundColor = ConsoleColor.Cyan;
                if (ifPackage == 0)
                {
                    Console.WriteLine("\n" + Router.RouterMain.GetTimestamp(DateTime.Now) + "\tOdebrano pakiet na interfejsie wejściowym " + port);
                    byte[] packetBytes = new byte[receivedData.Length - 4];
                    for (int i = 0; i < 4; i++)
                    {
                        packetBytes[i] = receivedData[i];
                    }
                    for (int i = 8; i < receivedData.Length; i++)
                    {
                        packetBytes[i - 4] = receivedData[i];
                    }
                    try
                    {
                        packetQueue.Enqueue(Router.RouterSwitcher.switchPacket(packetBytes, switchTables));
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tBrak dalszej ścieżki, pakiet stracony.");
                    }
                }
                else if (ifPackage == 1)
                {
                    Console.WriteLine("\n" + Router.RouterMain.GetTimestamp(DateTime.Now) + "\tOdebrano paczkę.");
                    DTOs.Package package = DTOs.Package.ByteToPackage(receivedData, 8);
                    int g = package.packageQueue.Count;
                    for (int i = 0; i < g; i++)
                    {
                        byte[] rData = package.packageQueue.Dequeue();
                        for (int j = 0; j < 4; j++)
                        {
                            rData[j] = receivedData[j];
                        }
                        try
                        {
                            packetQueue.Enqueue(Router.RouterSwitcher.switchPacket(rData, switchTables));
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tBrak dalszej ścieżki, pakiet stracony.");
                        }
                    }
                }
            }
        }
    }
}
