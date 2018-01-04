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
        public ControlParser(string filename, int subnetworknumber, ref string ccport, ref string rcport, Dictionary<String,int> controllers)
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
                        else if (line.Equals("ConnectionControllers"))
                            currentParsing = "ConnectionControllers";
                        else if (line.Equals("RC"))
                            currentParsing = "RC";
                        else
                        {
                            if (currentParsing.Equals("CC"))
                            {
                                ccport = line;
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

                        }

                    }
                }

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
