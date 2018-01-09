using System;
using System.Collections.Generic;
using System.Linq;
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
        static RoutingController routingController = new RoutingController();
        static Dictionary<String, int> controllers = new Dictionary<String, int>();
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

            // TYMCZASOWO - TESTOWANKO WYLICZANIA SCIEZKI-------------------------------------
            Console.WriteLine("Ścieżka z R6.S1/1 do R8.S1/3:");
            Console.WriteLine(routingController.RouteQuery("R6.S1/1", "R8.S1/3"));
            //--------------------------------------------------------------------------------
        }

        public static void Start()
        {
            Console.Title = "Control Node " + subnetworknumber;
            new ControlParser("controlconfig" + subnetworknumber + ".txt", subnetworknumber, ref ccport, ref rcport, controllers, routingController);
            Thread connectioncontrollerthread = new Thread(() => ConnectionController());
            Thread routingcontrollerthread = new Thread(() => RoutingController());
            connectioncontrollerthread.Start();
            routingcontrollerthread.Start();
        }

        public static void ConnectionController()
        {

        }

        public static void RoutingController()
        {

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
