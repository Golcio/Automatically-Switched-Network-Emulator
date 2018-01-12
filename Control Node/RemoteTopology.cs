using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_Node
{
    class RemoteTopology
    {
        public Dictionary<string, Topology> topologies = new Dictionary<string, Topology>();

        public class Topology { 
            public string inputSNPP;
            public Dictionary<string, string> clientsSNPPs = new Dictionary<string, string>();
        }
    }
}
