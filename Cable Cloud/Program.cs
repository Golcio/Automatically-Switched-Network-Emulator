using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Cable_Cloud
{
    class Program
    {
        static List<String[]> connections = new List<string[]>();
        static Dictionary<String, int> inputPorts = new Dictionary<String, int>();
        static Dictionary<String, int> outputPorts = new Dictionary<String, int>();
        static Dictionary<String, IPEndPoint> outputIPEndPoints = new Dictionary<String, IPEndPoint>();
        static Dictionary<String, Socket> inputSockets = new Dictionary<String, Socket>();
        static Dictionary<String, Socket> outputSockets = new Dictionary<String, Socket>();
        static Dictionary<String, bool> initiallyConnected = new Dictionary<string, bool>();
        static readonly object SyncObject = new object();

        static void Main(string[] args)
        {
            Console.Title = "Cable Cloud";
            parseConfigFile();
            createInputSockets();
            createOutputSockets();
            Console.ReadKey();
        }

        public static void parseConfigFile()
        {
            var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\CableCloud.txt";
            try
            {
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tParsowanie pliku konfiguracyjnego chmury kablowej...");
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
                        else
                        {
                            if (currentParsing.Equals("nodes"))
                            {
                                string[] splitArray = line.Split('_');
                                int outputPort = Int32.Parse(splitArray[1]);
                                outputPorts.Add(splitArray[0], outputPort);
                                outputIPEndPoints.Add(splitArray[0], new IPEndPoint(IPAddress.Parse("127.0.0.1"), outputPort));
                                outputSockets.Add(splitArray[0], new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                                initiallyConnected.Add(splitArray[0], false);
                                int inputPort = Int32.Parse(splitArray[2]);
                                inputSockets.Add(splitArray[0], new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                                inputPorts.Add(splitArray[0], inputPort);
                            }
                            else if (currentParsing.Equals("connections"))
                            {
                                string[] splitArray = line.Split('_');
                                connections.Add(splitArray);
                            }
                        }

                    }
                }
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tParsowanie pliku zakończone.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void createInputSockets()
        {
            foreach (KeyValuePair<string, Socket> kvp in inputSockets)
            {
                Thread inputConnectionThread = new Thread(() => initializeInputSocket(kvp.Value, inputPorts[kvp.Key], kvp.Key));
                inputConnectionThread.Start();
            }
        }

        public static void initializeInputSocket(Socket listenerSocket, int port, string nodeName)
        {
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            listenerSocket.Bind(endPoint);
            Console.ForegroundColor = ConsoleColor.Gray;

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
                        byte[] receivedData = new byte[readByte];
                        Array.Copy(buffer, receivedData, readByte);

                        byte[] interfaceByte = new byte[4];
                        try
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                interfaceByte[i] = receivedData[i];
                            }
                        }
                        catch (Exception e)
                        {
                            break;
                        }
                        int interfaceNumber = BitConverter.ToInt32(interfaceByte, 0);

                        string sourceNode = null;
                        foreach (KeyValuePair<string, int> kvp in inputPorts)
                        {
                            if (kvp.Value == port)
                                sourceNode = kvp.Key;
                        }

                        Console.ForegroundColor = ConsoleColor.Magenta;
                        if (sourceNode.Contains("client"))
                            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tOdebrano pakiet od " + sourceNode);
                        else
                            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tOdebrano pakiet od " + sourceNode + "nadany z interfejsu wyjściowego" + interfaceNumber);
                        lock (SyncObject)
                        {
                            sendPacket(receivedData, port, interfaceNumber);
                        }
                    } while (readByte > 0);
                        
                    }
                    catch (SocketException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(GetTimestamp(DateTime.Now) + "\tPołączenie z " + nodeName + " zerwane. Ponawiam połączenie...");
                        if (initiallyConnected[nodeName] == true)
                        {
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

        public static void createOutputSockets()
        {
            foreach (KeyValuePair<string, Socket> kvp in outputSockets)
            {
                Thread outputConnectionThread = new Thread(() => connectWithNode(kvp.Value, outputIPEndPoints[kvp.Key]));
                outputConnectionThread.Start();
            }
        }

        private static void connectWithNode(Socket socket, IPEndPoint ipEnd)
        {
            string node = outputPorts.Where(kvp => kvp.Value == ipEnd.Port).Select(kvp => kvp.Key).FirstOrDefault();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tSprawdzam połączenie z: " + node);
            while (true)
            {
                try
                {
                    socket.Connect(ipEnd);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(GetTimestamp(DateTime.Now) + "\tPołączono z: " + node);
                    if (initiallyConnected[node] == false)
                        initiallyConnected[node] = true;
                    break;
                }
                catch (SocketException ex)
                {

                }
            }
        }

        static void sendPacket(byte[] receivedData, int port, int interfaceNumber)
        {
            processInputPacket(ref port, ref interfaceNumber);

            string destNode = null;
            foreach (KeyValuePair<string, int> kvp in outputPorts)
            {
                if (kvp.Value == port)
                    destNode = kvp.Key;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            if (destNode.Contains("client"))
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tPrzekazuję pakiet do " + destNode);
            else
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tPrzekazuję pakiet do " + destNode + " na interfejs wejściowy " + interfaceNumber);

            byte[] intBytes = BitConverter.GetBytes(interfaceNumber);
            byte[] result = intBytes;

            for (int i = 0; i < 4; i++)
            {
                receivedData[i] = result[i];
            }

            try
            {
                outputSockets[destNode].Send(receivedData);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tPakiet przesłany do  " + destNode);
            }
            catch (SocketException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tBrak połączenia z: " + destNode + ". Pakiet zgubiony");                
            }
            
        }

        private static void processInputPacket(ref int port, ref int interfaceNumber)
        {
            for (int i = 0; i < connections.Count(); i++)
            {
                if ((Int32.Parse(connections[i][0]) == port) && (Int32.Parse(connections[i][1]) == interfaceNumber))
                {
                    port = Int32.Parse(connections[i][2]);
                    interfaceNumber = Int32.Parse(connections[i][3]);
                }
            }
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
    }
}
