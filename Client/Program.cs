using Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientTSST8
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string path = "ClientsConfig.txt";
            Reader reader = new Reader(path);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartingWindow(path, reader));
            //Agent agent = new Agent(reader.getChosenDefinition()[5], reader.getChosenDefinition()[6], reader.getChosenDefinition()[0]);

            //agent.agentSender(reader.getChosenDefinition()[0]);
            //Thread agentReceiverThread = new Thread(()=> agent.agentReceiver(reader.getChosenDefinition()[0]));
            //agentReceiverThread.Start();
            Sender sender = new Sender("127.0.0.1", reader.getChosenDefinition()[4]);
            string cpccinput = reader.getChosenDefinition()[5];
            string nccport = reader.getChosenDefinition()[6];
            string myid = reader.getChosenDefinition()[0];


            MainWindow mainWindow = new MainWindow(sender, reader, cpccinput, nccport, myid);
            Receiver receiver = new Receiver("127.0.0.1", reader.getChosenDefinition()[3], mainWindow);

            Thread receiverThread = new Thread(() => receiver.receive());
            receiverThread.Start();
            Application.Run(mainWindow);

            Environment.Exit(0);

        }
    }
}
