﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;


namespace NetworkCallControllerApplication
{
    class NetworkCC
    {
        string nccID;
        string inputport;
        string ccport;
        string addressclientA = "C1";
        string addressclientB = "C2";
        string addressclientC = "C3";
        string connection_number;
        //Slownik numerow polaczen
        Dictionary<string, string[]> connectionsIDs = new Dictionary<string, string[]>();
        Dictionary<string, string> capacities = new Dictionary<string, string>();
        //Slownik portow w AS1
        Dictionary<string, string> AS1_ports = new Dictionary<string, string>();
        //Slownik portow w AS2
        Dictionary<string, string> AS2_ports = new Dictionary<string, string>();

        public NetworkCC(string nccID, string inputport, string ccport)
        {
            this.nccID = "NCC" + nccID;
            this.inputport = inputport;
            //Console.ReadKey();
            this.ccport = ccport;
            // Zrobione po to zeby mozna bylo okreslic czy polaczenie ma uzywac NetworkCallController
            AS1_ports.Add("C1", "14041");
            AS2_ports.Add("C2", "14042");
            AS2_ports.Add("C3", "14043");
            Thread listenThread = new Thread(() => initializeListenerInputSocket());
            listenThread.Start();
        }


        void initializeListenerInputSocket()
        {

            string source_address = null;
            string destination_address = null;
            string destination_port = null;
            string received_data;
            string nccport = null;
            bool done = false;
            UdpClient listener = new UdpClient(Int32.Parse(inputport));
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Int32.Parse(inputport));

            byte[] receive_byte_array;
            try
            {
                while (!done)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    WriteLine("Otrzymano: " + received_data);
                    string[] splitArray = received_data.Split('_');

                    //Tlumaczenie ClientA na C1 itd. i wpisywanie do tablicy, zeby wrzucic je do slownika z numerami polaczen
                    string[] addressesArray = null;
                    if (!splitArray[0].Equals("ConnectionConfirmation") && !splitArray[0].Equals("DisconnectionConfirmation"))
                    {
                        addressesArray = DirectoryRequest(splitArray[1], splitArray[2]);
                        source_address = addressesArray[0];
                        destination_address = addressesArray[1];

                        //Tlumaczenie adresow docelowych na porty
                        destination_port = PortTranslation(destination_address);

                        //Sprawdzanie na jaki port ncc ma wyslac
                        nccport = NCCcheck(inputport);

                        //Ustalanie numerow polaczen
                        connection_number = SetConnectionNumber(source_address, destination_address);
                    }

                    if (splitArray[0].Equals("CallAcceptResponse"))
                    {
                        if (splitArray[3].Equals("YES"))
                        {
                            //If sprawdza czy adres docelowy jest w naszej podsieci i od razu do CC czy w innej i wysyłamy do NCC)
                            if (AS2_ports.ContainsKey(source_address) & AS2_ports.ContainsKey(destination_address))
                            {
                                string[] array = DirectoryRequest(splitArray[1], splitArray[2]);
                                string connectionNumber = SetConnectionNumber(array[0], array[1]);
                                ConnectionRequest(array[0], array[1], capacities[connectionNumber], ccport, connectionNumber);
                                //ConnectionRequest(source_address, destination_address, splitArray[3], ccport, connection_number);
                            }

                            else
                            {
                                NetworkCallCoordinationIN(source_address, destination_address, splitArray[3], nccport);
                            }
                        }
                        else
                        {
                            connectionsIDs.Remove(connection_number);
                            capacities.Remove(connection_number);
                            WriteLine("Połączenie z " + destination_address + " nie powiodło się.");
                        }
                    }
                    else if (splitArray[0].Equals("CallRequest"))
                    {

                        //connectionsIDs.Add(connection_number, addressesArray);

                        if (connectionsIDs.ContainsKey(connection_number))
                        {
                            connectionsIDs[connection_number] = addressesArray;
                        }
                        else
                        {
                            connectionsIDs.Add(connection_number, addressesArray);
                        }

                        if (capacities.ContainsKey(connection_number))
                        {
                            capacities[connection_number] = splitArray[3];
                        }
                        else
                        {
                            capacities.Add(connection_number, splitArray[3]);
                        }

                        WriteLine("Policy: Sprawdzam uprawnienia dla " + clientIDToClientName(addressesArray[0]) + "...");
                        WriteLine("Policy: Uprawnienia potwierdzone.");

                        if (AS2_ports.ContainsKey(source_address) & AS2_ports.ContainsKey(destination_address))
                        {
                            WriteLine("Dictionary: " + clientIDToClientName(addressesArray[1]) + " znajduje się w AS 2.");
                            CallAccept(source_address, destination_address, splitArray[3], destination_port);
                        }
                        else
                        {
                            string asid = null;
                            if (nccID.Equals("NCC1"))
                                asid = "AS 2";
                            else
                                asid = "AS 1";
                            WriteLine("Dictionary: " + clientIDToClientName(addressesArray[1]) + " znajduje się w domenie " + asid + " i ma adres " + addressesArray[1]);
                            NetworkCallCoordinationOUT(source_address, destination_address, splitArray[3], nccport);
                        }
                    }
                    else if (splitArray[0].Equals("CallTeardown"))
                    {
                        string connectionToRemove = null;
                        foreach (KeyValuePair<string, string[]> kvp in connectionsIDs)
                        {
                            if (kvp.Value[0].Equals(source_address) && kvp.Value[1].Equals(destination_address))
                            {
                                string number = kvp.Key;
                                connectionToRemove = number;
                            }
                        }
                        ConnectionTeardown(ccport, connectionToRemove);
                    }
                    else if (splitArray[0].Equals("NetworkCallCoordinationOUT"))
                    {
                        CallAccept(splitArray[1], splitArray[2], splitArray[3], PortTranslation(splitArray[2]));
                    }
                    else if (splitArray[0].Equals("NetworkCallCoordinationIN"))
                    {
                        string connectionNumber = SetConnectionNumber(splitArray[1], splitArray[2]);
                        ConnectionRequest(splitArray[1], splitArray[2], capacities[connectionNumber], ccport, connectionNumber);
                    }
                    else if (splitArray[0].Equals("ConnectionConfirmation"))
                    {
                        /*
                        if (splitArray[3].Equals("YES"))
                        {
                            CallConfirmed(destination_address, "YES", destination_port);
                        }
                        else
                        {
                            CallConfirmed(destination_address, "NO", destination_port);
                        }
                        */
                        string[] splitArray2 = splitArray[1].Split('*');
                        string connectionNumber = splitArray2[1];
                        CallConfirmed(clientIDToClientName(connectionsIDs[connectionNumber][1]), 
                            "YES", PortTranslation(connectionsIDs[connectionNumber][0]));
                    }
                    else if (splitArray[0].Equals("DisconnectionConfirmation"))
                    {
                        string[] splitArray2 = splitArray[1].Split('*');
                        string connectionNumber = splitArray2[1];
                        string[] st1 = connectionsIDs[connectionNumber];
                        string[] st2 = DirectoryRequest(clientIDToClientName(st1[0]), clientIDToClientName(st1[1]));
                        string port = null;
                        if (AS1_ports.ContainsKey(st2[0]))
                            port = AS1_ports[st2[0]];
                        else if (AS2_ports.ContainsKey(st2[0]))
                            port = AS2_ports[st2[0]];
                        CallTeardown(clientIDToClientName(st1[1]), port);
                        connectionsIDs.Remove(connectionNumber);
                        capacities.Remove(connectionNumber);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
        }
        public void Send(String message, String destination)
        {
            Boolean exception_thrown = false;
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Int32.Parse(destination));
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);

            try
            {
                sending_socket.SendTo(send_buffer, sending_end_point);
            }
            catch (Exception send_exception)
            {
                exception_thrown = true;
            }
            if (exception_thrown == false)
            {
                WriteLine("Wysłano: " + message + " do " + destination);
            }
            else
            {
                exception_thrown = false;
                WriteLine("Nie udało się wysłać żądania");
            }
        }
        public void CallAccept(string sourceid, string destinationid, string capacity, string destination_port)
        {
            StringBuilder messagesb = new StringBuilder();

            messagesb.Append("CallAccept_" + clientIDToClientName(sourceid) + "_" + clientIDToClientName(destinationid) + "_" + capacity);
            string message = messagesb.ToString();
            Send(message, destination_port);
        }
        public void ConnectionRequest(string sourceid, string destinationid, string capacity, string ccport, string connection_number)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("ConnectionRequest_" + sourceid + "," + destinationid + "," + capacity + "*" + connection_number);
            string message = messagesb.ToString();
            Send(message, ccport);
        }
        //OUT bo wysylane do NCC po otrzymaniu CallRequest
        public void NetworkCallCoordinationOUT(string sourceid, string destinationid, string capacity, string destination_port)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("NetworkCallCoordinationOUT_" + sourceid + "_" + destinationid + "_" + capacity);
            string message = messagesb.ToString();
            Send(message, destination_port);
        }
        //IN bo wysylane spowrotem do NCC po otrzymaniu accept od CPCC
        public void NetworkCallCoordinationIN(string sourceid, string destinationid, string capacity, string destination_port)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("NetworkCallCoordinationIN_" + sourceid + "_" + destinationid + "_" + capacity);
            string message = messagesb.ToString();
            Send(message, destination_port);
        }
        public void CallRequest(string sourceid, string destinationid, string capacity, string destination_port)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("CallRequest_" + sourceid + "_" + destinationid + "_" + capacity);
            string message = messagesb.ToString();
            Send(message, destination_port);
        }
        public void CallConfirmed(string destinationid, string decision, string destination_port)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("CallConfirmed_" + decision + "_" + destinationid);
            string message = messagesb.ToString();
            Send(message, destination_port);
        }
        public void ConnectionTeardown(string ccport, string connection_number)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("Disconnection_ *" + connection_number);
            string message = messagesb.ToString();
            Send(message, ccport);
        }
        public void CallTeardown(string destination, string destinationport)
        {
            StringBuilder messagesb = new StringBuilder();
            messagesb.Append("Disconnected_" + destination);
            string message = messagesb.ToString();

            Send(message, destinationport);
        }
        public static void WriteLine(String text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(GetTimestamp(DateTime.Now) + "\t" + text);
        }
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
        public string[] DirectoryRequest(string sourceid, string destinationid)
        {
            string[] addressesArray = new string[2];
            string sourceaddress;
            string destinationaddress;

            if (sourceid.Equals("clientA"))
            {
                sourceaddress = addressclientA;
            }
            else if (sourceid.Equals("clientB"))
            {
                sourceaddress = addressclientB;
            }
            else
            {
                sourceaddress = addressclientC;
            }

            if (destinationid.Equals("clientA"))
            {
                destinationaddress = addressclientA;
            }
            else if (destinationid.Equals("clientB"))
            {
                destinationaddress = addressclientB;
            }
            else
            {
                destinationaddress = addressclientC;
            }

            addressesArray[0] = sourceaddress;
            addressesArray[1] = destinationaddress;

            return addressesArray;
        }
        public string PortTranslation(string destinationid)
        {
            string destination_port2 = null;

            if (destinationid.Equals("C1"))
            {
                destination_port2 = "14041";
            }
            else if (destinationid.Equals("C2"))
            {
                destination_port2 = "14042";
            }
            else
            {
                destination_port2 = "14043";
            }

            return destination_port2;
        }
        public string NCCcheck(string inputport)
        {
            string nccport;
            if (inputport.Equals("14051"))
            {
                nccport = "14052";
            }
            else
            {
                nccport = "14051";
            }
            return nccport;
        }

        public string clientIDToClientName(string clientID)
        {
            if (clientID.Equals("C1"))
                return "clientA";
            else if (clientID.Equals("C2"))
                return "clientB";
            else
                return "clientC";

        }

        public string SetConnectionNumber(string sourceid, string destinationid)
        {
            string connection_number = null;

            if (sourceid.Equals("C1") & destinationid.Equals("C2"))
            {
                connection_number = "1";
            }
            else if (sourceid.Equals("C1") & destinationid.Equals("C3"))
            {
                connection_number = "2";
            }
            else if (sourceid.Equals("C2") & destinationid.Equals("C3"))
            {
                connection_number = "3";
            }
            else if (sourceid.Equals("C2") & destinationid.Equals("C1"))
            {
                connection_number = "4";
            }
            else if (sourceid.Equals("C3") & destinationid.Equals("C1"))
            {
                connection_number = "5";
            }
            else if (sourceid.Equals("C3") & destinationid.Equals("C2"))
            {
                connection_number = "6";
            }
            return connection_number;
        }
    }
}
