using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Router
{
    class RouterAgent
    {
        Socket agentSendSocket;
        Socket agentReceiveSocket;
        public string response;

        public RouterAgent(int routernumber, string agentport, string managerport)
        {
            int sendNMSPort;
            if (Int32.TryParse(managerport, out sendNMSPort))
            {
                agentSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendNMSPort);

                agentSendSocket.Connect(ipEnd);

                agentSendSocket.Send(Encoding.UTF8.GetBytes("networkNode" + routernumber));
            }
            int receiveNMSPort;
            if (Int32.TryParse(agentport, out receiveNMSPort))
            {
                agentReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEnd1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), receiveNMSPort);
                agentReceiveSocket.Bind(ipEnd1);

                agentReceiveSocket.Listen(0);
                agentReceiveSocket = agentReceiveSocket.Accept();
            }
        }

        public void agentReceiver(List<String[]> list)
        {

            while (true)
            {
                try
                {
                    byte[] Buffer = new byte[agentReceiveSocket.SendBufferSize];
                    int readByte = agentReceiveSocket.Receive(Buffer);
                    byte[] rData = new byte[readByte];
                    Array.Copy(Buffer, rData, readByte);
                    response = Encoding.UTF8.GetString(rData);
                    if (response.Length > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(GetTimestamp(DateTime.Now) + "\tOdebrano tablicę kierowania od systemu zarządzania.");
                        list.Clear();
                        if (response.Equals("empty"))
                        {
                            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tTablica nie zawiera teraz żadnych wpisów.");
                        }
                        else
                        {
                            string[] split = response.Split('|');
                            for (int i = 0; i < split.Length; i++)
                            {
                                string[] split2 = split[i].Split('_');
                                if (split2.Length == 4)
                                {
                                    string[] switchTable = { split2[0], split2[1], split2[2], split2[3] };
                                    list.Add(switchTable);
                                }
                                else if (split2.Length == 5)
                                {
                                    string[] switchTable = { split2[0], split2[1], split2[2], split2[3], split2[4] };
                                    list.Add(switchTable);
                                }

                            }
                            String s = String.Format("\t\t\t\t| {0,-5}  |  {1,-5}  |  {2,-5}  |  {3,-5}  |  {4,-6} |\n", "IWE", "EWE", "IWY", "EWY", "Tunel?");
                            s += String.Format("\t\t\t\t| {0,-5}  |  {1,-5}  |  {2,-5}  |  {3,-5}  |  {4,-6} |\n", "", "", "", "", "");
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (list[i].Length == 4)
                                {
                                    s += String.Format("\t\t\t\t| {0,-5}  |  {1,-5}  |  {2,-5}  |  {3,-5}  |  {4,-6} |\n", list[i][0], list[i][1], list[i][2], list[i][3], "NIE");
                                }
                                else if (list[i].Length == 5)
                                {
                                    s += String.Format("\t\t\t\t| {0,-5}  |  {1,-5}  |  {2,-5}  |  {3,-5}  |  {4,-6} |\n", list[i][0], list[i][1], list[i][2], list[i][3], "TAK");
                                }
                            }
                            Console.WriteLine(s);

                        }

                    }


                }
                catch (SocketException ex)
                {
                }
            }

        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
    }
}
