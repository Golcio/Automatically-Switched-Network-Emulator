using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace Router
{
    class RouterMain
    {
        static string port;
        static string cloudport;
        static string ccport;
        static string higherccport;
        static int routernumber;
        static string ip = "127.0.0.1";
        static Queue<byte[]> packetQueue = new Queue<byte[]>();
        static RouterSwitcher switcher;
        static List<String[]> switchTables = new List<string[]>();
        static void Main(string[] args)
        {
            WriteMenu();
            Console.ReadKey();
        }

        public static void WriteMenu()
        {
            Console.Title = "networkNode";
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Wybierz jakim chcesz być routerem:");
            Console.WriteLine(" ----------------");
            Console.WriteLine(" 1. networkNode1");
            Console.WriteLine(" 2. networkNode2");
            Console.WriteLine(" 3. networkNode3");
            Console.WriteLine(" 4. networkNode4");
            Console.WriteLine(" 5. networkNode5");
            Console.WriteLine(" 6. networkNode6");
            Console.WriteLine(" 7. networkNode7");
            Console.WriteLine(" 8. networkNode8");
            Console.WriteLine(" ----------------");

            ConsoleKeyInfo key = Console.ReadKey();
            if (char.IsDigit(key.KeyChar))
            {
                routernumber = Int32.Parse(key.KeyChar.ToString());
            }
            else
            {
                WriteMenu();
            }
            Console.Clear();
            StartRouter();
        }

        private static void StartRouter()
        {
            //////////////////////////////SPRAWDZANIE CZY DZIAŁA LRM/////////////////////////////////////
            Router.RouterAgentLRM.LRMStart("LinkConnectionRequest_R5.S1/1,14,R5.S1/3,88", switchTables);
            /////////////////////////////////////////////////////////////////////////////////////////////
            Console.Title = "networkNode" + routernumber;
            new RouterConfigParser("config" + routernumber + ".txt", routernumber, ref port, ref cloudport, ref ccport, ref higherccport);
            try
            {

                RouterSender sender = new RouterSender(ip, cloudport);
                RouterReceiver receiver = new RouterReceiver(ip, port);

                sender.senderThread(packetQueue);
                receiver.socketReceive(packetQueue, switchTables);
            }
            catch (SocketException ex)
            {
                Router.RouterMain.WriteMenu();
            }
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }

        public static void WriteLine(String text)
        {
            Console.WriteLine(GetTimestamp(DateTime.Now) + "\t" + text);
        }
    }
}
