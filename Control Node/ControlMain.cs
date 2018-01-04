using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_Node
{
    class ControlMain
    {
        static int subnetworknumber;
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
                subnetworknumber = Int32.Parse(key.KeyChar.ToString());
            }
            else
            {
                WriteMenu();
            }
            Console.Clear();
        }
    }
}
