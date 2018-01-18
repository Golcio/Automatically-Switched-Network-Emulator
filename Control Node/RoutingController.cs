﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Control_Node
{
    class RoutingController
    {
        NetworkTopology localTopology = new NetworkTopology();
        public RemoteTopology remoteTopology = new RemoteTopology();
        Dictionary<string, string> remoteRCsPorts = new Dictionary<string, string>();
        string subnetworknumber;
        string ccport;

        public RoutingController(string id)
        {
            subnetworknumber = id;
        }

        public void setCCport(string ccport)
        {
            this.ccport = ccport;
        }

        public void createTopology()
        {
            localTopology.create();
        }

        public void addLinkConnection(string[] lc)
        {
            localTopology.addLinkConnection(lc);
        }

        public Dictionary<string, List<string>> getSNPPs()
        {
            return localTopology.getSNPPs();
        }

        public Dictionary<string, string> getClientsSNPPs()
        {
            return localTopology.clientsSNPPs;
        }

        public Dictionary<string, string> getRemoteRCs()
        {
            return localTopology.outputSNPPs;
        }

        public Dictionary<string, string> getRemoteRCsPorts()
        {
            return remoteRCsPorts;
        }

        public void initiallyExchangeTopologies()
        {
            foreach (KeyValuePair<string, string> kvp in remoteRCsPorts)
            {
                NetworkTopologyIn(kvp.Key);
                NetworkTopologyOut(kvp.Key);
            }
        }

        public void RouteQuery(string pathStart, string pathEnd, string bandwidth, string connectionID)
        {
            WriteLine("Wyliczanie ścieżki z " + pathStart + " do " + pathEnd + " >= "
                + bandwidth + " Mbps");
            int intbandwidth = Int32.Parse(bandwidth);
            float cost1 = 1 / (float)intbandwidth;
            int cost = (int)(100000 * cost1);
            StringBuilder remotesb = new StringBuilder();
            bool ifRemoteClient = false;
            string remoteAS = null;
            foreach (KeyValuePair<string, RemoteTopology.Topology> kvp in remoteTopology.topologies)
                foreach (KeyValuePair<string, string> kvp2 in kvp.Value.clientsSNPPs)
                {
                    if (kvp2.Value.Equals(pathEnd))
                    {
                        remoteAS = kvp.Key;
                        remotesb.Append(";" + kvp.Key + ":");
                        remotesb.Append(kvp.Value.inputSNPP + "," + pathEnd);
                        ifRemoteClient = true;
                    }
                }
            if (ifRemoteClient == false)
            {
                string SNPa = null;
                string SNPb = null;
                foreach (KeyValuePair<string, List<string>> kvp in getSNPPs())
                {
                    if (kvp.Value.Contains(pathStart))
                        SNPa = kvp.Key;
                    if (kvp.Value.Contains(pathEnd))
                        SNPb = kvp.Key;
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("RouteQuery_");
                sb.Append(SNPa + ":");
                sb.Append(pathStart + ",");
                List<string> tempRoute;
                StringBuilder snpps = new StringBuilder();
                snpps.Append(pathStart + " ");
                string output = null;
                try
                {
                    tempRoute = localTopology.shortest_path(SNPa, SNPb, cost);
                    if (tempRoute.Count > 1)
                        for (int i = 0; i < tempRoute.Count - 1; i++)
                        {
                            string[] pair = localTopology.getSNPPsPair(tempRoute[i], tempRoute[i + 1]);
                            snpps.Append(pair[0] + " " + pair[1] + " ");
                            sb.Append(pair[0]);
                            foreach (KeyValuePair<string, List<string>> kvp in getSNPPs())
                            {
                                if (kvp.Value.Contains(pair[1]))
                                    sb.Append(";" + kvp.Key + ":");
                            }
                            sb.Append(pair[1] + ",");
                        }
                    sb.Append(pathEnd);
                    snpps.Append(pathEnd);
                    output = sb.ToString();
                    Send(output, ccport);
                    WriteLine("Ścieżka z " + pathStart + " do " + pathEnd + ": [" + snpps.ToString() + "]");
                }
                catch (Exception e)
                {
                    WriteLine("Brak ścieżki z " + pathStart + " do " + pathEnd);
                    output = "RouteQuery_NOPATH";
                    Send(output, ccport);
                }
            }
            else
            {
                string SNPa = null;
                string SNPb = null;
                string ASend = localTopology.outputSNPPs[remoteAS];
                foreach (KeyValuePair<string, List<string>> kvp in getSNPPs())
                {
                    if (kvp.Value.Contains(pathStart))
                        SNPa = kvp.Key;
                    if (kvp.Value.Contains(ASend))
                        SNPb = kvp.Key;
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("RouteQuery_");
                sb.Append(SNPa + ":");
                sb.Append(pathStart + ",");
                List<string> tempRoute;
                StringBuilder snpps = new StringBuilder();
                snpps.Append(pathStart + " ");
                string output = null;
                try
                {
                    tempRoute = localTopology.shortest_path(SNPa, SNPb, cost);
                    if (tempRoute.Count > 1)
                        for (int i = 0; i < tempRoute.Count - 1; i++)
                        {
                            string[] pair = localTopology.getSNPPsPair(tempRoute[i], tempRoute[i + 1]);
                            snpps.Append(pair[0] + " " + pair[1] + " ");
                            sb.Append(pair[0]);
                            foreach (KeyValuePair<string, List<string>> kvp in getSNPPs())
                            {
                                if (kvp.Value.Contains(pair[1]))
                                    sb.Append(";" + kvp.Key + ":");
                            }
                            sb.Append(pair[1] + ",");
                        }
                    sb.Append(ASend);
                    sb.Append(remotesb.ToString());
                    snpps.Append(pathEnd);
                    output = sb.ToString();
                    WriteLine("Wysyłam " + output + " do " + ccport);
                    Console.ReadKey();
                    Send(output, ccport);
                    WriteLine("Ścieżka z " + pathStart + " do " + pathEnd + ": [" + snpps.ToString() + "]");
                }
                catch (Exception e)
                {
                    output = "RouteQuery_NOPATH";
                    Send(output, ccport);
                    WriteLine("Brak ścieżki z " + pathStart + " do " + pathEnd);
                }
            }
        }

        public void ConnectionBroken(string SNPPa, string SNPPb)
        {
            localTopology.breakLinkConnection(SNPPa, SNPPb);
            WriteLine("Aktualizacja topologii: zerwane połączenie " + SNPPa + " - " + SNPPb);
        }

        public void ConnectionRestored(string SNPPa, string SNPPb)
        {
            localTopology.restoreLinkConnection(SNPPa, SNPPb);
            WriteLine("Aktualizacja topologii: przywrócone połączenie " + SNPPa + " - " + SNPPb);
        }

        public void NetworkTopologyIn(string SNPid)
        {
            Send("NetworkTopologyIn_" + subnetworknumber, remoteRCsPorts[SNPid]);
        }

        public void NetworkTopologyOut(String SNPid)
        {
            ShortTopology shortTopology = new ShortTopology();
            foreach (KeyValuePair<string, string> kvp in localTopology.clientsSNPPs)
                shortTopology.clients.Add(kvp.Key, kvp.Value);
            shortTopology.inputSNPP = localTopology.outputSNPPs[SNPid];

            string networkTopologyString = shortTopology.ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append("NetworkTopologyOut_" + subnetworknumber + "_");
            sb.Append(networkTopologyString);
            Send(sb.ToString(), remoteRCsPorts[SNPid]);
            string domain = null;
            if (subnetworknumber.Equals("10"))
                domain = "AS 1";
            else if (subnetworknumber.Equals("11"))
                domain = "AS 2";
            WriteLine("Wysylam topologię do " + domain);
        }

        public void Send(String message, String destination)
        {
            Boolean exception_thrown = false;
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Int32.Parse(destination));
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);

            try
            {
                sending_socket.SendTo(send_buffer, sending_end_point);
            }
            catch (Exception send_exception)
            {
                exception_thrown = true;
            }
        }
        
        public static void WriteLine(String text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tRC: " + text);
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
    }
}
