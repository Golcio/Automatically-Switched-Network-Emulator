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
        static UdpClient listener;
        static IPEndPoint groupEP;
        static Socket sender;
        static Dictionary<string, string> nextlrms;
        static List<string> labelpool;
        static Random rnd = new Random();

        public RouterAgentLRM(string lrmport, Dictionary<string, string> nextlrms, List<string> labelpool)
        {
            listener = new UdpClient(Int32.Parse(lrmport));
            groupEP = new IPEndPoint(IPAddress.Any, Int32.Parse(lrmport));
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            RouterAgentLRM.nextlrms = new Dictionary<string, string>(nextlrms);
            RouterAgentLRM.labelpool = new List<string>(labelpool);
        }

        public static void LRMStart(List<String[]> switchTables)
        {
            string received_data;
            byte[] receive_byte_array;

            try
            {
                while (true)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    Router.RouterMain.WriteLine("LRM: Otrzymano:\t" + received_data);
                    string[] splitArray = received_data.Split('_');

                    if (splitArray[0].Equals("LinkConnectionRequest"))
                    {
                        LinkConnectionRequest(splitArray[1], switchTables, labelpool, nextlrms);
                    }
                    else if (splitArray[0].Equals("LinkConnectionDeallocation"))
                    {
                        LinkConnectionDeallocation(switchTables);
                    }
                    else if (splitArray[0].Equals("LinkConnection"))
                    {
                        LinkConnection();
                    }
                }
                //9:R3.S1/1,21,R4.S1/3,37;5:R5.S1/1,14,R5.S1/3,88
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void LinkConnectionRequest(string points, List<String[]> switchTables, List<string> labelpool, Dictionary<string, string> nextlrms)
        {
            string nextlrm = null;
            string[] splitarray = points.Split(',');
            foreach(KeyValuePair<string, string> kvp in nextlrms)
            {
                if(kvp.Key == splitarray[1])
                {
                    nextlrm = kvp.Value;
                    
                }
            }
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Int32.Parse(nextlrm));
            int r = rnd.Next(labelpool.Count);
            
            byte[] send_buffer = Encoding.ASCII.GetBytes("LinkConnection_" + );
            sender.SendTo(send_buffer, sending_end_point);

        }
        //R3.S1/1,R4.S1/3
        private static void LinkConnection(string port1, string label1, string port2, string label2, List<String[]> switchTables)
        {
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
