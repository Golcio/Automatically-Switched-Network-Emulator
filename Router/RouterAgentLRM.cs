using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Router
{
    class RouterAgentLRM
    {
        static UdpClient listener;
        static IPEndPoint groupEP;
        static UdpClient listener2;
        static IPEndPoint groupEP2;
        static Socket sender;
        static Dictionary<string, string> nextlrms;
        static List<string> labelpool;
        static Random rnd = new Random();
        static string lrmtolrmport;
        static string ccport;

        public RouterAgentLRM(string lrmport, string lrmtolrmport, string ccport, Dictionary<string, string> nextlrms, List<string> labelpool)
        {
            RouterAgentLRM.lrmtolrmport = lrmtolrmport;
            RouterAgentLRM.ccport = ccport;
            listener = new UdpClient(Int32.Parse(lrmport));
            groupEP = new IPEndPoint(IPAddress.Any, Int32.Parse(lrmport));
            listener2 = new UdpClient(Int32.Parse(lrmtolrmport));
            groupEP2 = new IPEndPoint(IPAddress.Any, Int32.Parse(lrmtolrmport));
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            RouterAgentLRM.nextlrms = new Dictionary<string, string>(nextlrms);
            RouterAgentLRM.labelpool = new List<string>(labelpool);
        }

        public static void LRMStart(List<String[]> switchTables)
        {
            string received_data;
            byte[] receive_byte_array;
            string[] returnvalues = null;
            string label1 = null;
            string label2 = null;
            string port1 = null;
            string port2 = null;

            try
            {
                while (true)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    Router.RouterMain.WriteLine("LRM: Otrzymano:\t" + received_data);
                    string[] splitArray = received_data.Split('_');
                    //wiadomość od CC
                    if (splitArray[0].Equals("LinkConnectionRequest"))
                    {
                        Thread lkr = new Thread(() =>
                        {
                            returnvalues = LinkConnectionRequest(splitArray[1], switchTables, labelpool, nextlrms);
                            label2 = returnvalues[0];
                            port1 = returnvalues[1];
                            port2 = returnvalues[2];
                        });

                        lkr.Start();
                    }
                    //wiadomośc od CC (jeszcze nie gotowe)
                    else if (splitArray[0].Equals("LinkConnectionDeallocation"))
                    {
                        LinkConnectionDeallocation(switchTables);
                    }
                    //wiadomośc od wcześniejszego LRMa
                    else if (splitArray[0].Equals("LinkConnection"))
                    {
                        Send("LinkConnectionConfirmation", splitArray[2]);
                        label1 = splitArray[1];
                    }
                    //warunek pozwalający zestawić połączenie czyli dodać linijkę do tablicy komutacji
                    if(label1 != null && label2 != null)
                    {
                        LinkConnection(port1, label1, port2, label2, switchTables);
                        label1 = null;
                        label2 = null;
                        port1 = null;
                        port2 = null;
                        Send("LinkConnectionRequestConfirm", ccport);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string[] LinkConnectionRequest(string points, List<String[]> switchTables, List<string> labelpool, Dictionary<string, string> nextlrms)
        {
            string nextlrm = null;
            string[] splitarray = points.Split(',');
            foreach (KeyValuePair<string, string> kvp in nextlrms)
            {
                if (kvp.Key == splitarray[1])
                {
                    nextlrm = kvp.Value;
                }
            }
            int r = rnd.Next(labelpool.Count);
            string label2 = labelpool[r];
            string port1 = splitarray[0].Split('/')[1];
            string port2 = splitarray[1].Split('/')[1];
            string[] returnvalues = { label2, port1, port2 };
            if (nextlrm != null)
            {
                Send("LinkConnection_" + label2 + "_" + lrmtolrmport, nextlrm);
                string received_data;
                byte[] receive_byte_array;
                receive_byte_array = listener2.Receive(ref groupEP2);
                received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                Router.RouterMain.WriteLine("LRM: Otrzymano potwierdzenie:\t" + received_data);
            }
            return returnvalues;
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

        private static void Send(string message, string port)
        {
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Int32.Parse(port));
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);
            sender.SendTo(send_buffer, sending_end_point);
        }
    }
}
