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
    class RouterSender
    {
        Socket socket;
        static Dictionary<int, DTOs.Package> packages = new Dictionary<int, DTOs.Package>();
        static bool timesup = false;
        static int timeleft = 0;
        static readonly object SyncObject = new object();
        public RouterSender(string destinationIP, string destinationPort)
        {
            int port;
            if (Int32.TryParse(destinationPort, out port))
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tSocket wysyłający utworzony.");
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(destinationIP), port);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tSocket wysyłający nawiązuje połączenie...");
                while (true)
                {
                    try
                    {
                        socket.Connect(ipEnd);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tSocket wysyłający pomyślnie nawiązał połączenie.");
                        break;
                    }
                    catch (SocketException ex)
                    {

                    }
                }

            }
        }

        public void socketSend(Queue<byte[]> packetQueue)
        {
            while (true)
            {
                lock (SyncObject)
                {
                    if (packetQueue.Count != 0)
                    {
                        byte[] packet = packetQueue.Dequeue();
                        byte[] currentPortBytes = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            currentPortBytes[i] = packet[i];
                        }
                        int currentPort = BitConverter.ToInt32(currentPortBytes, 0);

                        if (!packages.ContainsKey(currentPort))
                        {
                            packages.Add(currentPort, new DTOs.Package());
                            Thread timerThread = new Thread(() => queueCountdown(currentPort));
                            timerThread.Start();
                        }

                        packages[currentPort].packageQueue.Enqueue(packet);

                        if (packages[currentPort].packageQueue.Count == 5)
                        {
                            byte[] packageBytes = DTOs.Package.PackageToByte(packages[currentPort]);
                            byte[] output = new byte[packageBytes.Length + 8];
                            byte[] ifPackage = BitConverter.GetBytes(1);
                            Array.Copy(packageBytes, 0, output, 8, packageBytes.Length);
                            Array.Copy(currentPortBytes, 0, output, 0, 4);
                            Array.Copy(ifPackage, 0, output, 4, 4);

                            packages[currentPort].packageQueue.Clear();

                            try
                            {
                                socket.Send(output);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tPrzesłano paczkę zawierającą 5 pakietów.");
                            }
                            catch (Exception e)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                }

            }
            socket.Close();
        }

        public void queueCountdown(int currentPort)
        {
            while (true)
            {
                Thread.Sleep(1000); //na razie co sekundę
                if (packages[currentPort].packageQueue.Count > 0)
                {
                    int size = packages[currentPort].packageQueue.Count;
                    byte[] packageBytes = DTOs.Package.PackageToByte(packages[currentPort]);
                    byte[] output = new byte[packageBytes.Length + 8];
                    byte[] ifPackage = BitConverter.GetBytes(1);
                    byte[] currentPortBytes = BitConverter.GetBytes(currentPort);
                    Array.Copy(packageBytes, 0, output, 8, packageBytes.Length);
                    Array.Copy(currentPortBytes, 0, output, 0, 4);
                    Array.Copy(ifPackage, 0, output, 4, 4);

                    packages[currentPort].packageQueue.Clear();

                    try
                    {
                        socket.Send(output);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tPrzesłano paczkę zawierającą " + size + " pakietów.\n");
                    }
                    catch (Exception e)
                    {
                        return;
                    }
                }
            }
        }

        public void senderThread(Queue<byte[]> packetQueue)
        {
            Thread outthread;
            outthread = new Thread(() => socketSend(packetQueue));
            outthread.Start();
        }
        
    }
}
