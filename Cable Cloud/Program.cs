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
        static List<String[]> nodeInputPorts = new List<string[]>();
        static List<String[]> nodeOutputPorts = new List<string[]>();
        static List<String[]> connections = new List<string[]>();
        static Dictionary<String, int> nodes = new Dictionary<String, int>();
        static Dictionary<String, int> listeners = new Dictionary<String, int>();
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
                        else if (line.Equals("nodeInputPorts"))
                            currentParsing = "nodeInputPorts";
                        else if (line.Equals("nodeOutputPorts"))
                            currentParsing = "nodeOutputPorts";
                        else if (line.Equals("connections"))
                            currentParsing = "connections";
                        else
                        {
                            if (currentParsing.Equals("nodes"))
                            {
                                string[] splitArray = line.Split('_');
                                int nodeport = Int32.Parse(splitArray[1]);
                                nodes.Add(splitArray[0], nodeport);
                                outputIPEndPoints.Add(splitArray[0], new IPEndPoint(IPAddress.Parse("127.0.0.1"), nodeport));
                                outputSockets.Add(splitArray[0], new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                                initiallyConnected.Add(splitArray[0], false);
                                int listener = Int32.Parse(splitArray[2]);
                                inputSockets.Add(splitArray[0], new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                                listeners.Add(splitArray[0], listener);
                            }
                            else if (currentParsing.Equals("nodeInputPorts"))
                            {
                                string[] splitArray = line.Split('_');
                                nodeInputPorts.Add(splitArray);
                            }
                            else if (currentParsing.Equals("nodeOutputPorts"))
                            {
                                string[] splitArray = line.Split('_');
                                nodeOutputPorts.Add(splitArray);
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
                Thread inputConnectionThread = new Thread(() => initializeInputSocket(kvp.Value, listeners[kvp.Key], kvp.Key));
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

                        byte[] currentPortByte = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            currentPortByte[i] = receivedData[i];
                        }
                        int currentPort = BitConverter.ToInt32(currentPortByte, 0);
                        string node = null;
                        bool flag = false;
                        for (int i = 0; i < nodeOutputPorts.Count; i++)
                        {
                            for (int j = 1; j < nodeOutputPorts[i].Length; j++)
                            {
                                if (nodeOutputPorts[i][j].Equals(currentPort.ToString()))
                                {
                                    node = nodeOutputPorts[i][0];
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == true)
                            {
                                break;
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine(GetTimestamp(DateTime.Now) + "\tOdebrano pakiet na porcie " + listeners[node]);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(GetTimestamp(DateTime.Now) + "\tPakiet nadany z interfejsu wyjściowego " + currentPort);
                        lock (SyncObject)
                        {
                            sendPacket(receivedData, currentPort);
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
            string node = nodes.Where(kvp => kvp.Value == ipEnd.Port).Select(kvp => kvp.Key).FirstOrDefault();
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

        static void sendPacket(byte[] receivedData, int currentPort)
        {
            processInputPacket(ref currentPort);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tPrzekazuję pakiet do węzła z interfejsem wejściowym " + currentPort);

            byte[] intBytes = BitConverter.GetBytes(currentPort);
            byte[] result = intBytes;

            for (int i = 0; i < 4; i++)
            {
                receivedData[i] = result[i];
            }

            int outputPort = 0;
            for (int i = 0; i < nodeInputPorts.Count; i++)
            {
                for (int j = 1; j < nodeInputPorts[i].Length; j++)
                {
                    if (Int32.Parse(nodeInputPorts[i][j]) == currentPort)
                    {
                        outputPort = nodes[nodeInputPorts[i][0]];
                        break;
                    }
                }
            }
            string node = nodes.Where(kvp => kvp.Value == outputPort).Select(kvp => kvp.Key).FirstOrDefault();
            try
            {
                outputSockets[node].Send(receivedData);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tWysyłam do socketa działającego na porcie " + outputPort);
            }
            catch (SocketException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(GetTimestamp(DateTime.Now) + "\tBrak połączenia z: " + node + ". Pakiet zgubiony");                
            }
            
        }

        private static void processInputPacket(ref int currentPort)
        {
            for (int i = 0; i < connections.Count(); i++)
            {
                if (Int32.Parse(connections[i][0]) == currentPort)
                {
                    currentPort = Int32.Parse(connections[i][1]);
                }
            }
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
    }
}
