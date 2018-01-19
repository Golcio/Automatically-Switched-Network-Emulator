using ClientTSST8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    class CallingPartyCallController
    {
        string nccPort;
        string inputPort;
        string myid;
        MainWindow mainWindow;

        public CallingPartyCallController(string inputPort, string nccPort, string myid, MainWindow mainWindow)
        {
            this.nccPort = nccPort;
            this.inputPort = inputPort;
            this.myid = "client" + myid;
            this.mainWindow = mainWindow;

            Thread listenThread = new Thread(() => initializeInputSocket());
            listenThread.Start();
        }

        void initializeInputSocket()
        {
            bool done = false;
            UdpClient listener = new UdpClient(Int32.Parse(inputPort));
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Int32.Parse(inputPort));
            string received_data;
            byte[] receive_byte_array;

            try
            {
                while (!done)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    string[] splitArray = received_data.Split('_');
                    if (splitArray[0].Equals("CallAccept"))
                    {
                        CallAccept(splitArray[1]);
                    }
                    else if (splitArray[0].Equals("CallConfirmed"))
                    {
                        if (splitArray[1].Equals("YES"))
                        {
                            mainWindow.connectedProcedure(splitArray[2]);
                        }
                        else
                        {
                            MessageBox.Show("Połączenie z " + splitArray[2] + " nie powiodło się.");
                            mainWindow.disconnectedProcedure();
                        }
                    }
                    else if (splitArray[0].Equals("Disconnected"))
                    {
                        mainWindow.writeToConsole("Rozłączono z " + splitArray[1] + ".");
                        mainWindow.disconnectedProcedure();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
        }

        public void Send(String message)
        {
            bool exception_thrown = false;
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Int32.Parse(nccPort));
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);

            try
            {
                sending_socket.SendTo(send_buffer, sending_end_point);
            }
            catch (Exception send_exception)
            {
                exception_thrown = true;
            }
        }

        public void CallRequest(string destinationid, string capacity)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("CallRequest_" + myid + "_" + destinationid + "_" + capacity);
            string message = messagesb.ToString();
            Send(message);
        }

        public void CallTeardown(string destinationid)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("CallTeardown_" + myid + "_" + destinationid);
            string message = messagesb.ToString();
            Send(message);
        }

        public void CallAccept(string callingid)
        {
            StringBuilder messagesb = new StringBuilder();
            DialogResult dialogResult = MessageBox.Show("Połączenie od " + callingid + ". Akceptować?", "Nowe połączenie", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                messagesb.Append("CallAcceptResponse_" + callingid + "_" + myid + "_YES");
            }
            else if (dialogResult == DialogResult.No)
            {
                messagesb.Append("CallAcceptResponse_" + callingid + "_" + myid + "_NO");
            }
            Send(messagesb.ToString());
        }
    }
}
