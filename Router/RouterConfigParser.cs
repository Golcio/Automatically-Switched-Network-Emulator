using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RouterConfigParser
    {
        public RouterConfigParser(string filename, int routernumber, ref string port, ref string cloudport, ref string ccport, ref string higherccport, ref string lrmport, ref string lrmtolrmport, Dictionary<string, string> nextlrms, List<string> labelpool)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tParsowanie pliku konfiguracyjnego routera nr " + routernumber + "...");
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    string currentParsing = null;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Equals("address"))
                            currentParsing = "address";
                        else if (line.Equals("cloud"))
                            currentParsing = "cloud";
                        else if (line.Equals("ConnectionControler"))
                            currentParsing = "ConnectionControler";
                        else if (line.Equals("higherCC"))
                            currentParsing = "higherCC";
                        else if (line.Equals("lrm"))
                            currentParsing = "lrm";
                        else if (line.Equals("lrmtolrm"))
                            currentParsing = "lrmtolrm";
                        else if (line.Equals("nextlrms"))
                            currentParsing = "nextlrms";
                        else if (line.Equals("labelpool"))
                            currentParsing = "labelpool";
                        else
                        {
                            if (currentParsing.Equals("address"))
                            {
                                port = line;
                            }
                            else if (currentParsing.Equals("cloud"))
                            {
                                cloudport = line;
                            }
                            else if (currentParsing.Equals("ConnectionControler"))
                            {
                                ccport = line;
                            }
                            else if (currentParsing.Equals("higherCC"))
                            {
                                higherccport = line;
                            }
                            else if (currentParsing.Equals("lrm"))
                            {
                                lrmport = line;
                            }
                            else if (currentParsing.Equals("lrmtolrm"))
                            {
                                lrmtolrmport = line;
                            }
                            else if (currentParsing.Equals("nextlrms"))
                            {
                                string[] splitarray = line.Split('_');
                                nextlrms.Add(splitarray[0], splitarray[1]);
                            }
                            else if (currentParsing.Equals("labelpool"))
                            {
                                labelpool.Add(line);
                            }

                        }
                    }
                }
                Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tParsowanie pliku zakończone.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Router.RouterMain.WriteMenu();
            }
        }
    }
}
