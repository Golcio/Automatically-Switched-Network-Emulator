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
        public RouterConfigParser(string filename, int routernumber, ref string port, ref string cloudport, ref string agentport, ref string managerport)
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
                        else if (line.Equals("agent"))
                            currentParsing = "agent";
                        else if (line.Equals("manager"))
                            currentParsing = "manager";
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
                            else if (currentParsing.Equals("agent"))
                            {
                                agentport = line;
                            }
                            else if (currentParsing.Equals("manager"))
                            {
                                managerport = line;
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
