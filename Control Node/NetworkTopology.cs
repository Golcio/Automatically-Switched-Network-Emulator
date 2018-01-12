using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_Node
{
    class NetworkTopology
    {
        List<LinkConnection> linkConnections = new List<LinkConnection>();
        Dictionary<string, Dictionary<string, int>> vertices = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, List<string>> SNPPs = new Dictionary<string, List<string>>();
        public Dictionary<string, string> clientsSNPPs = new Dictionary<string, string>();
        public Dictionary<string, string> outputSNPPs = new Dictionary<string, string>();

        public void create()
        {
            foreach (LinkConnection lc in linkConnections)
            {
                if (!vertices.ContainsKey(lc.SNPa))
                    vertices.Add(lc.SNPa, new Dictionary<string, int>());
                vertices[lc.SNPa].Add(lc.SNPb, lc.cost);
            }
        }

        public void addLinkConnection(string[] lc)
        {
            int bandwidth = Int32.Parse(lc[4]);
            linkConnections.Add(new LinkConnection(lc[0], lc[2], lc[1], lc[3], bandwidth));
        }

        public string[] getSNPPsPair(string SNPa, string SNPb)
        {
            string[] pair = new string[2];
            foreach (LinkConnection lc in linkConnections)
            {
                if (lc.SNPa.Equals(SNPa) && lc.SNPb.Equals(SNPb))
                {
                    pair[0] = lc.SNPPa;
                    pair[1] = lc.SNPPb;
                }
            }
            return pair;
        }

        public Dictionary<string, List<string>> getSNPPs()
        {
            return SNPPs;
        }

        public List<string> shortest_path(string start, string finish)
        {
            var previous = new Dictionary<string, string>();
            var distances = new Dictionary<string, int>();
            var nodes = new List<string>();

            List<string> path = null;
            foreach (var vertex in vertices)
            {
                if (vertex.Key == start)
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                }

                nodes.Add(vertex.Key);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);

                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == finish)
                {
                    path = new List<string>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in vertices[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        class LinkConnection
        {
            public string SNPPa;
            public string SNPPb;
            public string SNPa;
            public string SNPb;
            public int cost;

            public LinkConnection(string SNPPa, string SNPPb, string SNPa, string SNPb, int bandwidth)
            {
                this.SNPPa = SNPPa;
                this.SNPPb = SNPPb;
                this.SNPa = SNPa;
                this.SNPb = SNPb;
                float cost1 = 1 / (float)bandwidth;
                cost = (int)(1000 * cost1);
            }
        }
    }
}
