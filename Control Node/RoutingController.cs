using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_Node
{
    class RoutingController
    {
        NetworkTopology localTopology = new NetworkTopology();

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

        public string RouteQuery(string pathStart, string pathEnd)
        {
            string SNPa = null;
            string SNPb = null;
            foreach (KeyValuePair <string, List<string>> kvp in getSNPPs())
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

        public void NetworkTopologyIn()
        {

        }

        public void RemoteTopologyIn()
        {

        }

        public void LocalTopologyOut()
        {

        }

        public void NetworkTopologyOut()
        {

        }
    }
}
