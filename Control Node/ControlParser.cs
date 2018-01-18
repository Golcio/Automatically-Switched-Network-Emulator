using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_Node
{
    class ControlParser
    {
        public ControlParser(string filename, int subnetworknumber, ref string ccport, ref string rcport, Dictionary<String, int> controllers, RoutingController rc, ref int partnerPort, ref int parentPort)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Control_Node.ControlMain.GetTimestamp(DateTime.Now) + "\tParsowanie pliku konfiguracyjnego węzła sterowania podsieci nr " + subnetworknumber + "...");
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    string currentParsing = null;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Equals("CC"))
                            currentParsing = "CC";
                        else if (line.Equals("PartnerPort"))
                            currentParsing = "PartnerPort";
                        else if (line.Equals("ParentPort"))
                            currentParsing = "ParentPort";
                        else if (line.Equals("ConnectionControllers"))
                            currentParsing = "ConnectionControllers";
                        else if (line.Equals("RC"))
                            currentParsing = "RC";
                        else if (line.Equals("SNPPs"))
                            currentParsing = "SNPPs";
                        else if (line.Equals("LinkConnections"))
                            currentParsing = "LinkConnections";
                        else if (line.Equals("Clients"))
                            currentParsing = "Clients";
                        else if (line.Equals("RemoteRCs"))
                            currentParsing = "RemoteRCs";
                        else
                        {
                            if (currentParsing.Equals("CC"))
                            {
                                ccport = line;
                            }
                            else if (currentParsing.Equals("PartnerPort"))
                            {
                                int port;
                                Int32.TryParse(line, out port);
                                partnerPort = port;
                            }
                            else if (currentParsing.Equals("ParentPort"))
                            {
                                int port;
                                Int32.TryParse(line, out port);
                                parentPort = port;
                            }
                            else if (currentParsing.Equals("ConnectionControllers"))
                            {
                                string[] splitArray = line.Split('_');
                                controllers.Add(splitArray[0], Int32.Parse(splitArray[1]));
                            }
                            else if (currentParsing.Equals("RC"))
                            {
                                rcport = line;
                            }
                            else if (currentParsing.Equals("SNPPs"))
                            {
                                string[] splitArray = line.Split('_');
                                if (rc.getSNPPs().ContainsKey(splitArray[0]))
                                    rc.getSNPPs()[splitArray[0]].Add(splitArray[1]);
                                else
                                {
                                    rc.getSNPPs().Add(splitArray[0], new List<string>());
                                    rc.getSNPPs()[splitArray[0]].Add(splitArray[1]);
                                }
                            }
                            else if (currentParsing.Equals("LinkConnections"))
                            {
                                string[] splitArray = line.Split('_');
                                string[] lc = new string[5];
                                lc[0] = splitArray[0];
                                foreach (KeyValuePair<string, List<string>> kvp in rc.getSNPPs())
                                {
                                    if (kvp.Value.Contains(splitArray[0]))
                                        lc[1] = kvp.Key;
                                }
                                lc[2] = splitArray[1];
                                foreach (KeyValuePair<string, List<string>> kvp in rc.getSNPPs())
                                {
                                    if (kvp.Value.Contains(splitArray[1]))
                                        lc[3] = kvp.Key;
                                }
                                lc[4] = splitArray[2];
                                rc.addLinkConnection(lc);
                            }
                            else if (currentParsing.Equals("Clients"))
                            {
                                string[] splitArray = line.Split('_');
                                rc.getClientsSNPPs().Add(splitArray[0], splitArray[1]);
                            }
                            else if (currentParsing.Equals("RemoteRCs"))
                            {
                                string[] splitArray = line.Split('_');
                                rc.getRemoteRCsPorts().Add(splitArray[0], splitArray[1]);
                                rc.getRemoteRCs().Add(splitArray[0], splitArray[2]);
                            }
                        }

                    }
                }
                rc.createTopology();

                Console.WriteLine(Control_Node.ControlMain.GetTimestamp(DateTime.Now) + "\tParsowanie pliku zakończone.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Control_Node.ControlMain.WriteMenu();
            }
        }
    }
}
