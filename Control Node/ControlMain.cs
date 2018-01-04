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
        }

        public static void Start()
        {
            Console.Title = "Control Node " + subnetworknumber;
            new ControlParser("controlconfig" + subnetworknumber + ".txt", subnetworknumber, ref ccport, ref rcport, controllers);
            Thread connectioncontrollerthread = new Thread(() => connectionController());
            Thread routingcontrollerthread = new Thread(() => routingController());
            connectioncontrollerthread.Start();
            routingcontrollerthread.Start();
        }

        public static void connectionController()
        {

        }

        public static void routingController()
        {

        }

        public static void linkResourceManager()
        {

        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
    }
}
