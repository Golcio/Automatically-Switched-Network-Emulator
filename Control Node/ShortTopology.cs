using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_Node
{
    class ShortTopology
    {
        public Dictionary<String, String> clients = new Dictionary<string, string>();
        public String inputSNPP = null;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(KeyValuePair<string, string> kvp in clients)
            {
                sb.Append(kvp.Key + "|" + kvp.Value + "$");
            }
            if (sb.Length > 0)
            {
                sb = sb.Remove(sb.Length - 1, 1);
                sb.Append("&");
            }
            if (inputSNPP != null)
                sb.Append(inputSNPP);
            return sb.ToString();
        }

        public static void ShortTopologyParse(string response, string SNPid, RoutingController rc)
        {
            RemoteTopology.Topology topology = new RemoteTopology.Topology();
            string[] array1 = response.Split('&');
            topology.inputSNPP = array1[1];

            string[] array2 = array1[0].Split('$');
            foreach (string kvp in array2)
            {
                string[] array3 = kvp.Split('|');
                topology.clientsSNPPs.Add(array3[0], array3[1]);
            }
            if (rc.remoteTopology.topologies.ContainsKey(SNPid))
            {
                rc.remoteTopology.topologies[SNPid] = topology;
            }
            else
                rc.remoteTopology.topologies.Add(SNPid, topology);
        }
    }
}
