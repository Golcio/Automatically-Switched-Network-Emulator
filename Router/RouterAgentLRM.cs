using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RouterAgentLRM
    {
        public static void LRMStart(string received_data, List<String[]> switchTables)
        {

            string[] splitArray = received_data.Split('_');

            if (splitArray[0].Equals("LinkConnectionRequest"))
            {
                LinkConnectionRequest(splitArray[1], switchTables);
            }
            else if (splitArray[0].Equals("LinkConnectionDeallocation"))
            {
                LinkConnectionDeallocation(switchTables);
            }
        }
        
        private static void LinkConnectionRequest(string points, List<String[]> switchTables)
        {
            string[] split = points.Split(',');
            string label1 = split[1];
            string label2 = split[3];
            string port1 = split[0].Split('/')[1];
            string port2 = split[2].Split('/')[1];

            string[] switchtablerow = { port1, label1, port2, label2 };
            switchTables.Add(switchtablerow);
            Router.RouterMain.WriteLine(switchTables[0][0] + "_" + switchTables[0][1] + "_" + switchTables[0][2] + "_" + switchTables[0][3]);

        }
        private static void LinkConnectionDeallocation(List<String[]> switchTables)
        {
            switchTables.Clear();
            //jakieś wywołanie funkcji cc która uruchamia machinę wypierdalania wszystkiego
        }
    }
}
