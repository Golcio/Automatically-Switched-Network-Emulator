﻿using System;
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
        static string rcport;
        static int routernumber;

        public RouterAgentLRM(string lrmport, string lrmtolrmport, string ccport, Dictionary<string, string> nextlrms, List<string> labelpool, string rcport, List<string[]> switchTables, int routernumber)
        {
            RouterAgentLRM.lrmtolrmport = lrmtolrmport;
            RouterAgentLRM.ccport = ccport;
            RouterAgentLRM.rcport = rcport;
            listener = new UdpClient(Int32.Parse(lrmport));
            groupEP = new IPEndPoint(IPAddress.Any, Int32.Parse(lrmport));
            listener2 = new UdpClient(Int32.Parse(lrmtolrmport));
            groupEP2 = new IPEndPoint(IPAddress.Any, Int32.Parse(lrmtolrmport));
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            RouterAgentLRM.nextlrms = new Dictionary<string, string>(nextlrms);
            RouterAgentLRM.labelpool = new List<string>(labelpool);
            RouterAgentLRM.routernumber = routernumber;
            Router.RouterMain.WriteLine("Link Resource Manager Utworzony.");
            Thread lrm = new Thread(() => LRMStart(switchTables));
            lrm.Start();
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
            string connectionid = null;
            string SNPPs = null;

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
                        connectionid = received_data.Split('*')[1];
                        SNPPs = splitArray[1].Split('*')[0];
                        Thread lcr = new Thread(() =>
                        {
                            returnvalues = LinkConnectionRequest(splitArray[1].Split('*')[0], switchTables, labelpool, nextlrms);
                            label2 = returnvalues[0];
                            port1 = returnvalues[1];
                            port2 = returnvalues[2];
                            if (returnvalues.Length == 4)
                            {
                                label1 = returnvalues[3];
                            }

                            if (label1 != null && label2 != null)
                            {
                                LinkConnection(port1, label1, port2, label2, switchTables, connectionid);
                                label1 = null;
                                label2 = null;
                                port1 = null;
                                port2 = null;
                                Send("LinkConnectionRequestConfirm_" + routernumber + "*" + connectionid, ccport);
                                Router.RouterMain.WriteLine("LRM: Zestawiono połączenie nr " + connectionid);
                            }
                        });

                        lcr.Start();
                    }
                    //wiadomośc od CC (jeszcze nie gotowe)
                    else if (splitArray[0].Equals("LinkConnectionDeallocation"))
                    {
                        LinkConnectionDeallocation(switchTables, splitArray[1]);
                    }
                    //wiadomośc od wcześniejszego LRMa
                    else if (splitArray[0].Equals("LinkConnection"))
                    {
                        Send("LinkConnectionConfirmation", splitArray[2]);
                        Router.RouterMain.WriteLine("LRM: Wysłano potwierdzenie otrzymania wiadomości z etykietą " + splitArray[1] + " na port zwrotny " + splitArray[2]);
                        label1 = splitArray[1];
                    }
                    else if (splitArray[0].Equals("BreakConnection"))
                    {
                        Router.RouterMain.WriteLine("LRM: Zerwano połączenie pomiędzy interfejsem " + splitArray[1].Split('/')[1] + " routera " + splitArray[1].Split('.')[0] + " i interfejsem " + splitArray[2].Split('/')[1] + " routera " + splitArray[2].Split('.')[0] + ".");
                        Send(received_data, rcport);
                        Router.RouterMain.WriteLine("LRM: Wysłano informację o zerwanym łączu do RC na port " + rcport);
                        Send("BreakConnection_" + " " + "*" + connectionid, ccport);
                        Router.RouterMain.WriteLine("LRM: Wysłano informację o zerwanym łączu do CC na port " + ccport);
                        Send(received_data, "14099");
                    }
                    else if (splitArray[0].Equals("RestoreConnection"))
                    {
                        Router.RouterMain.WriteLine("LRM: Naprawiono połączenie pomiędzy interfejsem " + splitArray[1].Split('/')[1] + " routera " + splitArray[1].Split('.')[0] + " i interfejsem " + splitArray[2].Split('/')[1] + " routera " + splitArray[2].Split('.')[0] + ".");
                        Send(received_data, rcport);
                        Router.RouterMain.WriteLine("LRM: Wysłano informację o naprawionym łączu do RC na port " + rcport);
                        Send("RestoreConnection_" + " " + "*" + connectionid, ccport);
                        Router.RouterMain.WriteLine("LRM: Wysłano informację o naprawionym łączu do CC na port " + ccport);
                        Send(received_data, "14099");
                    }

                    //warunek pozwalający zestawić połączenie czyli dodać linijkę do tablicy komutacji
                    if (label1 != null && label2 != null)
                    {
                        LinkConnection(port1, label1, port2, label2, switchTables, connectionid);
                        label1 = null;
                        label2 = null;
                        port1 = null;
                        port2 = null;
                        Send("LinkConnectionRequestConfirm_" + routernumber + "*" + connectionid, ccport);
                        Router.RouterMain.WriteLine("LRM: Zestawiono połączenie nr " + connectionid);
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
            string label1 = null;
            string nextlrm = null;
            string[] splitarray = points.Split(',');
            foreach (KeyValuePair<string, string> kvp in nextlrms)
            {
                if (kvp.Key == splitarray[1] && kvp.Value != "0")
                {
                    nextlrm = kvp.Value;

                }

                if (kvp.Key == splitarray[0] && kvp.Value == "0")
                {
                    label1 = "0";
                }
            }
            int r = rnd.Next(labelpool.Count);
            string label2 = labelpool[r];
            string port1 = splitarray[0].Split('/')[1];
            string port2 = splitarray[1].Split('/')[1];
            string[] returnvalues;
            if (label1 == null)
            {
                returnvalues = new string[] { label2, port1, port2 };
            }
            else
            {
                returnvalues = new string[] { label2, port1, port2, label1 };
            }

            if (nextlrm != null)
            {
                Send("LinkConnection_" + label2 + "_" + lrmtolrmport, nextlrm);
                Router.RouterMain.WriteLine("LRM: Wysłano etykietę " + label2 + " oraz port zwrotny " + lrmtolrmport + " do LRMa działającego na porcie " + nextlrm);
                string received_data;
                byte[] receive_byte_array;
                receive_byte_array = listener2.Receive(ref groupEP2);
                received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                Router.RouterMain.WriteLine("LRM: Otrzymano potwierdzenie:\t" + received_data);
            }
            return returnvalues;
        }
        //R3.S1/1,R4.S1/3
        private static void LinkConnection(string port1, string label1, string port2, string label2, List<String[]> switchTables, string connectionid)
        {
            string[] switchtablerow = { port1, label1, port2, label2, connectionid };
            switchTables.Add(switchtablerow);
            Router.RouterMain.WriteLine("LRM: Dodano wpis do tablicy komutacji: " + switchTables[switchTables.Count - 1][0] + "_" + switchTables[switchTables.Count - 1][1] + "_" + switchTables[switchTables.Count - 1][2] + "_" + switchTables[switchTables.Count - 1][3] + "_" + switchTables[switchTables.Count - 1][4]);

        }
        private static void LinkConnectionDeallocation(List<String[]> switchTables, string connectionid)
        {
            int counter = 0;
            string[] removeline = null;
            foreach (string[] line in switchTables)
            {
                if (line[4].Equals(connectionid))
                {
                    removeline = line;
                    Send("LinkConnectionDeallocationConfirm_ *" + connectionid, ccport);
                    Router.RouterMain.WriteLine("LRM: Wysłano potwierdzenie dealokacji łącza połączenia nr " + connectionid);
                    counter++;
                }
            }

            if(removeline != null)
            {
                Router.RouterMain.WriteLine("LRM: Usunięto wpis z tablicy: " + removeline[0] + "_" + removeline[1] + "_" + removeline[2] + "_" + removeline[3] + "_" + removeline[4]);
                switchTables.Remove(removeline);
            }
            
            if(counter == 0)
            {
                Send("LinkConnectionDeallocationDenied_ *" + connectionid, ccport);
                Router.RouterMain.WriteLine("LRM: Operacja dealokacji anulowana: nie ma połączenia o numerze " + connectionid);
            }
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
