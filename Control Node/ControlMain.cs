using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Control_Node
{
    class ControlMain
    {
        static int subnetworknumber;
        static string ccport;
        static string rcport;
        static int parentPort;
        static int partnerPort;
        static RoutingController routingController;
        static Dictionary<String, int> controllers = new Dictionary<String, int>();
        static int ccportINT;
        static int rcportINT;
        
        static void Main(string[] args)
        {
            WriteMenu();
            Console.ReadKey();
        }

        public static void WriteMenu()
        {
            Console.Title = "Control Node";
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Wybierz jakiej podsieci chcesz być węzłem sterowania:");
            Console.WriteLine(" ------------------");
            Console.WriteLine(" 1. Control Node 9");
            Console.WriteLine(" 2. Control Node 10");
            Console.WriteLine(" 3. Control Node 11");
            Console.WriteLine(" ------------------");

            ConsoleKeyInfo key = Console.ReadKey();
            if (char.IsDigit(key.KeyChar))
            {
                subnetworknumber = Int32.Parse(key.KeyChar.ToString()) + 8;
            }
            else
            {
                WriteMenu();
            }
            Console.Clear();
            Start();
        }

        public static void Start()
        {
            Console.Title = "Control Node " + subnetworknumber;
            routingController = new RoutingController(subnetworknumber.ToString());
            new ControlParser("controlconfig" + subnetworknumber + ".txt", subnetworknumber, ref ccport, ref rcport, controllers, routingController, ref partnerPort, ref parentPort);           
            Thread connectioncontrollerthread = new Thread(() => ConnectionController());
            Thread routingcontrollerthread = new Thread(() => RoutingController());
            connectioncontrollerthread.Start();
            routingcontrollerthread.Start();
            Int32.TryParse(ccport, out ccportINT);
            Int32.TryParse(rcport, out rcportINT);
            ConnectionController connectionController = new ConnectionController(ccportINT, subnetworknumber, rcportINT, partnerPort, parentPort);
        }

        public static void ConnectionController()
        {

        }

        public static void RoutingController()
        {
            routingController.setCCport(ccport);
            Thread initiallyExchangeTopologiesThread = new Thread(() => routingController.initiallyExchangeTopologies());
            initiallyExchangeTopologiesThread.Start();

            bool done = false;
            UdpClient listener = new UdpClient(Int32.Parse(rcport));
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Int32.Parse(rcport));
            string received_data;
            byte[] receive_byte_array;

            try
            {
                while (!done)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    string[] splitArray = received_data.Split('_');
                    if (splitArray[0].Equals("NetworkTopologyIn"))
                    {
                        routingController.NetworkTopologyOut(splitArray[1]);
                    }
                    else if (splitArray[0].Equals("NetworkTopologyOut"))
                    {
                        ShortTopology.ShortTopologyParse(splitArray[2], splitArray[1], routingController);
                        string domain = null;
                        if (splitArray[1].Equals("10"))
                            domain = "AS 1";
                        else if (splitArray[1].Equals("11"))
                            domain = "AS 2";
                        Control_Node.RoutingController.WriteLine("Otrzymano topologię " + domain);
                    }
                    else if (splitArray[0].Equals("RouteTableQuery"))
                    {
                        string[] splitArray2 = splitArray[1].Split('*');
                        string[] splitArray3 = splitArray2[0].Split(',');
                        routingController.RouteQuery(splitArray3[0], splitArray3[1], splitArray3[2], splitArray2[1]);
                    }
                    else if (splitArray[0].Equals("ConnectionBroken"))
                    {
                        routingController.ConnectionBroken(splitArray[1], splitArray[2]);
                    }
                    else if (splitArray[0].Equals("ConnectionRestored"))
                    {
                        routingController.ConnectionBroken(splitArray[1], splitArray[2]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
        }

        public static void LinkResourceManager()
        {

        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
    }
}
