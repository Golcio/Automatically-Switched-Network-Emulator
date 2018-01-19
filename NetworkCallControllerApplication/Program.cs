using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCallControllerApplication
{
    class Program
    {
        static int ASnumber;
        static string inputport;
        static string ccport;
        static string nccID;
        static NetworkCC ncc;
        static void Main(string[] args)
        {
            WriteMenu();
            Console.ReadKey();
        }

        public static void WriteMenu()
        {
            Console.Title = "Network Call Controller";
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Wybierz z którym AS chcesz być połączony:");
            Console.WriteLine(" ------------------");
            Console.WriteLine("1 - AS1");
            Console.WriteLine("2 - AS2");
            Console.WriteLine(" ------------------");

            ConsoleKeyInfo key = Console.ReadKey();
            if (char.IsDigit(key.KeyChar))
            {
                ASnumber = Int32.Parse(key.KeyChar.ToString());
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
            Console.Title = "NCC" + ASnumber;
            if (ASnumber == 1)
            {
                nccID = "1";
                inputport = "14051";
                ccport = "16001";
                Console.WriteLine("NCC" + nccID);
                Console.WriteLine("Port na którym słucha NCC to: " + inputport);
            }
            else
            {
                nccID = "2";
                inputport = "14052";
                ccport = "16003";
                Console.WriteLine("NCC" + nccID);
                Console.WriteLine("Port na którym słucha NCC to: " + inputport);
            }
            ncc = new NetworkCC(nccID, inputport, ccport);
        }
    }
    
}
