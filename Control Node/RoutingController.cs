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

        public RoutingController(string id)
        {
            subnetworknumber = id;
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

        public string RouteQuery(string pathStart, string pathEnd)
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
            sb.Append(SNPa + ":");
            sb.Append(pathStart + ",");
            List<string> tempRoute = localTopology.shortest_path(SNPa, SNPb);
            if (tempRoute.Count > 1)
                for (int i = 0; i < tempRoute.Count - 1; i++)
                {
                    string[] pair = localTopology.getSNPPsPair(tempRoute[i], tempRoute[i + 1]);
                    sb.Append(pair[0]);
                    foreach (KeyValuePair<string, List<string>> kvp in getSNPPs())
                    {
                        if (kvp.Value.Contains(pair[1]))
                            sb.Append("_" + kvp.Key + ":");
                    }
                    sb.Append(pair[1] + ",");
                }
            sb.Append(pathEnd);
            return sb.ToString();
        }

        public void LocalTopologyIn()
        {

        }

        public void NetworkTopologyIn(string SNPid)
        {
            Send("NetworkTopologyIn_" + subnetworknumber, remoteRCsPorts[SNPid]);
        }

        public void RemoteTopologyIn()
        {
            
        }

        public void LocalTopologyOut()
        {

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
        }

        public void Send(String message, String source)
        {
            Boolean exception_thrown = false;
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Int32.Parse(source));
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);

            try
            {
                sending_socket.SendTo(send_buffer, sending_end_point);
            }
            catch (Exception send_exception)
            {
                exception_thrown = true;
            }
            if (exception_thrown == false)
            {
                Console.WriteLine("Wysłano (RC):    " + message);
            }
            else
            {
                exception_thrown = false;
                Console.WriteLine("Message failed to send.");
            }
        }
    }
}
