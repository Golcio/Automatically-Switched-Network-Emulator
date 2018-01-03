using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientTSST8
{
    public class Reader
    {
        string[] clientDefinitions;
        string[] clientNames;
        static string[] chosenDefinition;
        
        public Reader(string path)
        {
            clientDefinitions = File.ReadAllLines(path);
            clientDefinitions = clientDefinitions.Skip(1).ToArray();
            clientNames = new string[clientDefinitions.Length];
            for(int i=0; i < clientDefinitions.Length; i++)
            {
                clientNames[i] = clientDefinitions[i].Split('|')[0];
            }
        }

        public string[] getClientNames()
        {
            return clientNames;
        }
        public string getClientName()
        {
            return chosenDefinition[0];
        }
                

        public void chooseDefinition(string clientName)
        {
            foreach (string definition in clientDefinitions)
                if (definition.Split('|')[0].Equals(clientName))
                    chosenDefinition = definition.Split('|');
        }

        public string[] getChosenDefinition()
        {
            return chosenDefinition;
        }
        public string[] getFreeNames()
        {
            string[] namesToReturn = new string[clientNames.Length - 1];
            int i = 0;
            foreach(string name in clientNames)
            {
                if(!name.Equals(chosenDefinition[0]))
                {
                    namesToReturn[i] = name;
                    i++;
                }
            }
            return namesToReturn;
        }
    }
}
