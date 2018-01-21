using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Control_Node
{
    class ConnectionController
    {

        //Buffor wiadommości, które przychodzą na powyższy port
        Stack<string> Buffor = new Stack<string>();
        //komponenty CC, którymi zarządza dany CC, key to numer podsieci, którą zarządza CCdziecko,
        //value to numer portu tego dziecka.
        Dictionary<string, string> children = new Dictionary<string, string>();
        Dictionary<string, string> partners = new Dictionary<string, string>();
        //istniejące połączenia int-numer połączenia, string- numery podsieci, których CC biorą udział
        //w tym połączeniu
        //Dictionary<int, string> connections = new Dictionary<int, string>();

        //Dictionary<int, int> confirmations = new Dictionary<int, int>();
        int udpListenPort, subnetworkNumber, RCPort, partnerPort, parentPort;
        //int - numer połączenia, string - numer podsieci, bool - czy doszło potwierdzenie
        Dictionary<int, Dictionary<string, bool>> connections = new Dictionary<int, Dictionary<string, bool>>();
        Dictionary<string, string> connectionsSNPPs = new Dictionary<string, string>(); 
        Dictionary<string, string> capacity = new Dictionary<string, string>();
        Dictionary<string, string> confirmations = new Dictionary<string, string>();
        List<string> connectionsToRestore = new List<string>();
        bool czyBreak = false;
        public ConnectionController(int udpListenPort, int subnetworkNumber, int RCPort, int partnerPort, int parentPort)
        {
            this.udpListenPort = udpListenPort;
            this.subnetworkNumber = subnetworkNumber;
            this.RCPort = RCPort;
            this.partnerPort = partnerPort;
            this.parentPort = parentPort;
            FamilyTies();
            if (partnerPort > 0)
                Partners(subnetworkNumber.ToString());
            //dwa podstawowe wątki, czyli odbieranie żądań i analizowanie ich.
            Thread receiveThread = new Thread(() => receiving());
            receiveThread.Start();
            Thread analizeThread = new Thread(() => analizing());
            analizeThread.Start();

        }
        void receiving()
        {
            try
            {
                UdpClient udpServer = new UdpClient(udpListenPort);
                while (true)
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, udpListenPort);
                    var data = udpServer.Receive(ref remoteEP);
                    Buffor.Push(Encoding.UTF8.GetString(data));
                    WriteLine("Otrzymano wiadomosc o tresci " + Encoding.UTF8.GetString(data));
                }
            }
            catch (Exception e)
            {

            }
        }
        void AddingChildren(string subnetworkNumber, string remotePort)
        {
            children.Add(subnetworkNumber, remotePort);
            WriteLine("Dodano CC z podsieci nr " + subnetworkNumber + " do zarządzanych CC.");
        }

        void AddingPartners(string subnetworkNumber, string mySubnetworkNumber, string remotePort)
        {
            if (partners.ContainsKey(subnetworkNumber))
                return;
            else
            {
                partners.Add(subnetworkNumber, remotePort);
                Partners(mySubnetworkNumber.ToString());
                WriteLine("Dodano CC z podsieci nr " + subnetworkNumber + " do partnerujących CC.");
            }
        }
        void analizing()
        {
            while (true)
            {
                if (Buffor.Count > 0)
                {
                    //Wiadomość wygląda następująco: "messageType_restMessage*numerpołączenia#remoteIP;remotePort
                    //Interesujące nas elementy to: messageType, restMessage oraz remotePort
                    //remoteIP nie jest potrzebne ze względu na to, że wszystkie komponenty mają ten sam adres IP.
                    string message = Buffor.Pop();
                    string messageType = message.Split('_')[0];
                    string restMessage = message.Split('_')[1].Split('*')[0];
                    string connectionORportNumber = message.Split('_')[1].Split('*')[1];
                    string[] array = restMessage.Split(',');
                    switch (messageType)
                    {
                        case "FamilyTies":
                            WriteLine("Otrzymano informacje o zarzadzanym CC podsieci nr " + restMessage);
                            AddingChildren(restMessage, connectionORportNumber);
                            break;
                        case "Partners":
                            WriteLine("Otrzymano informacje o partnerujacym CC podsieci nr " + restMessage);
                            AddingPartners(restMessage, subnetworkNumber.ToString(), connectionORportNumber);
                            break;
                        case "ConnectionRequest":
                            WriteLine("Otrzymano ConnectionRequest o treści : " + message + ".");
                            if (!connectionsSNPPs.ContainsKey(connectionORportNumber))
                                connectionsSNPPs.Add(connectionORportNumber, restMessage);
                            if (!capacity.ContainsKey(connectionORportNumber))
                                capacity.Add(connectionORportNumber, restMessage.Split(',')[2]);
                            else
                                capacity[connectionORportNumber] = restMessage.Split(',')[2];
                            if (!confirmations.ContainsKey(connectionORportNumber))
                                confirmations.Add(connectionORportNumber, "parent");
                            else
                                confirmations[connectionORportNumber] = "parent";
                            //restMessage: punkt1,punkt2,przepustowosc
                            RouteTableQuery(restMessage, connectionORportNumber);
                            break;
                        case "RouteTableQuery":
                            WriteLine("Otrzymano RouteQuery.");
                            //restMessage: numerPodsieci:punkt1,punkt2;numerPodsieci2:punkt1,punkt2
                            ConnectionRequest(restMessage, connectionORportNumber);
                            break;
                        case "PeerCoordination":
                            WriteLine("Otrzymano PeerCoordination.");
                            if (!capacity.ContainsKey(connectionORportNumber))
                                capacity.Add(connectionORportNumber, restMessage.Split(',')[2]);
                            else
                                capacity[connectionORportNumber] = restMessage.Split(',')[2];
                            RouteTableQueryAfterPeer(restMessage, connectionORportNumber);
                            if (!confirmations.ContainsKey(connectionORportNumber))
                                confirmations.Add(connectionORportNumber, "partner");
                            else
                                confirmations[connectionORportNumber] = "partner";
                            break;
                        case "ConnectionConfirmation":
                            WriteLine("Otrzymano ConnectionConfirmation.");
                            ConfirmationsController(restMessage, connectionORportNumber);
                            break;
                        case "BreakConnection":
                            //idzie z dołu
                            czyBreak = true;
                            WriteRedLine("Połączenie numer " + connectionORportNumber + " zostało zerwane. Prawdopodobnie jest to wina koparki. ");
                            BreakConnectionReact(restMessage, connectionORportNumber);
                            break;
                        case "Disconnection":
                            //idzie z gory
                            WriteLine("Rozpoczęto procedurę zwalniania połączenia numer " + connectionORportNumber);
                            Disconnection(connectionORportNumber);
                            break;
                        case "DisconnectionConfirmation":
                            WriteLine("Otrzymano potwierdzenie zwolnienia połączenia numer " + connectionORportNumber);
                            DisconnectionsController(restMessage, connectionORportNumber);
                            break;
                    }
                }
                Thread.Sleep(50);
            }
        }

        void BreakConnectionReact(string restMessage, string connectionNumber)
        {
            //string subnetworkNumber = restMessage;
            if (subnetworkNumber == 10 || subnetworkNumber == 11)
            {
                WriteLine("Rozpoczęto procedurę zwalniania połączenia numer " + connectionNumber);
                connectionsToRestore.Add(connectionNumber);
                Disconnection(connectionNumber);
            }
            else
            {
                var client = new UdpClient();
                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), parentPort);
                client.Connect(point);
                string message = "BreakConnection_" + restMessage + "*" + connectionNumber;
                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                //var receivedData = client.Receive(ref point);
                WriteLine("Wysłano alert o zerwanym połączeniu numer " + connectionNumber + " w podsieci numer " + restMessage);
            }
            
        }
        void Disconnection(string connectionNumber)
        {
            int connectionNumb, port;
            Int32.TryParse(connectionNumber, out connectionNumb);
            foreach (KeyValuePair<int, Dictionary<string, bool>> kvp in connections)
            {
                if(kvp.Key == connectionNumb)
                {
                    foreach(KeyValuePair<string, bool> kvpC in connections[connectionNumb])
                    {
                        foreach(KeyValuePair<string, string> kvpCC in children)
                        {
                            if(kvpC.Key.Equals(kvpCC.Key))
                            {
                                Int32.TryParse(kvpCC.Value, out port);
                                var client = new UdpClient();
                                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                                client.Connect(point);
                                string message = "Disconnection_" + " " + "*" + connectionNumber;
                                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                            }
                        }
                        foreach(KeyValuePair<string, string> kvpP in partners)
                        {
                            if(kvpC.Key.Equals(kvpP.Key))
                            {
                                Int32.TryParse(kvpP.Value, out port);
                                var client = new UdpClient();
                                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                                client.Connect(point);
                                string message = "Disconnection_" + " " + "*" + connectionNumber;
                                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                            }
                        }
                    }
                    
                }
            }
        }

        void ConfirmationsController(string restMessage, string connectionNumber)
        {
            string subnetworkNumber = restMessage;
            Int32.TryParse(connectionNumber, out int connectionNumb);
            connections[connectionNumb][subnetworkNumber] = true;
            WriteLine("Otrzymano potwierdzenie zestawienia połączenia numer " + connectionNumber + " od CC w podsieci numer " + subnetworkNumber);
            int confirmations = 0;
            foreach (KeyValuePair<string, bool> kvp in connections[connectionNumb])
            {
                if (kvp.Value == true)
                    confirmations++;
            }
            if (confirmations == connections[connectionNumb].Count)
                ConnectionConfirmation(connectionNumber);
        }

        void DisconnectionsController(string restMessage, string connectionNumber)
        {
            string subnetworkNumberString = restMessage;
            Int32.TryParse(connectionNumber, out int connectionNumb);
            connections[connectionNumb][subnetworkNumberString] = false;
            WriteLine("Otrzymano potwierdzenie zwolnienia połączenia numer " + connectionNumber + " od CC w podsieci numer " + subnetworkNumberString);
            int confirmations = connections[connectionNumb].Count;
            foreach (KeyValuePair<string, bool> kvp in connections[connectionNumb])
            {
                if (kvp.Value == false)
                    confirmations--;
            }
            if (confirmations == 0 )
            {
                if (((subnetworkNumber == 10 || subnetworkNumber == 11) && czyBreak == true))
                {
                    //ConnectionRequest(connectionsSNPPs[connectionNumber], connectionNumber);
                    RouteTableQuery(connectionsSNPPs[connectionNumber], connectionNumber);
                    czyBreak = false;
                }
                else
                    DisconnectionConfirmation(connectionNumber);
            }
                
        }

        void DisconnectionConfirmation(string connectionNumber)
        {
            string port = " ";
            foreach (KeyValuePair<string, string> kvp in confirmations)
            {
                if (kvp.Key.Equals(connectionNumber))
                    port = kvp.Value;
            }
            if (port.Equals("parent"))
            {
                var client = new UdpClient();
                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), parentPort);
                client.Connect(point);
                string message = "DisconnectionConfirmation_" + subnetworkNumber + "*" + connectionNumber;
                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                //var receivedData = client.Receive(ref point);
                WriteLine("Wysłano potwierdzenie zwolnienia połączenia o numerze " + connectionNumber);
            }
            else if (port.Equals("partner"))
            {
                var client = new UdpClient();
                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), partnerPort);
                client.Connect(point);
                string message = "DisconnectionConfirmation_" + subnetworkNumber + "*" + connectionNumber;
                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                //var receivedData = client.Receive(ref point);
                WriteLine("Wysłano potwierdzenie zwolnienia połączenia o numerze " + connectionNumber);
            }
        }
        void ConnectionConfirmation(string connectionNumber)
        {
            string port = " ";
            foreach(KeyValuePair<string, string> kvp in confirmations)
            {
                if (kvp.Key.Equals(connectionNumber))
                    port = kvp.Value;
            }
            if(port.Equals("parent"))
            {
                var client = new UdpClient();
                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), parentPort);
                client.Connect(point);
                string message = "ConnectionConfirmation_" + subnetworkNumber + "*" + connectionNumber;
                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                //var receivedData = client.Receive(ref point);
                WriteLine("Wysłano ConnectionConfirmation połączenia o numerze " + connectionNumber);
            }
            else if (port.Equals("partner"))
            {
                var client = new UdpClient();
                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), partnerPort);
                client.Connect(point);
                string message = "ConnectionConfirmation_" + subnetworkNumber + "*" + connectionNumber;
                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                //var receivedData = client.Receive(ref point);
                WriteLine("Wysłano ConnectionConfirmation połączenia o numerze " + connectionNumber);
            }
            
        }

        //RC zestaw mi połączenie pomiędzy tymi dwoma punktami: w wiadomości przekazywane są dwa punkty.
        void RouteTableQuery(string routeQuery, string connectionNumber)
        {
            try
            {
                if (routeQuery == "NOPATH")
                    WriteRedLine("Nie można zestawić takiego połączenia.");
                else
                {
                    var client = new UdpClient();
                    IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), RCPort);
                    client.Connect(point);
                    string message = "RouteTableQuery_" + routeQuery + "*" + connectionNumber;
                    client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                }
            }
            catch (Exception e)
            {

            }

        }
        void RouteTableQueryAfterPeer(string routeQuery, string connectionNumber)
        {
            try
            {
                string capacityString =" ";
                foreach (KeyValuePair<string, string> kvpCC in capacity)
                {
                    if (connectionNumber.Equals(kvpCC.Key))
                        capacityString = kvpCC.Value;
                }
                if (routeQuery == "NOPATH")
                    WriteRedLine("Nie można zestawić takiego połączenia.");
                else
                {
                    var client = new UdpClient();
                    IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), RCPort);
                    client.Connect(point);
                    string message = "RouteTableQuery_" + routeQuery + "," + capacityString + "*" + connectionNumber;
                    client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                }
            }
            catch (Exception e)
            {

            }

        }


        //Wysyłanie wiadomosci "elo ziom, zarządzam podsiecią nr "subnetworkNumber", 
        //a ty zarządzasz mną. Jak coś to siędzę na tym porcie, elo pis joł
        void FamilyTies()
        {
            try
            {
                var client = new UdpClient();
                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), parentPort);
                client.Connect(point);
                string message = "FamilyTies_" + subnetworkNumber.ToString() + "*" + udpListenPort;
                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
            }
            catch (Exception e)
            {

            }

        }

        //w momencie, gdy dostaniemy wiadomość od RC z podsieciami pomiędzy którymi 
        //będzie przebiegało połączenie wysyłamy do nich żądanie zestawienia połączenia ConnectionRequest.
        //Funkcja analizuje wiadomość: jakie sieci biorą udział w połączeniu, jakie podsieci powiadomić.
        //Wysyła do nich ConnectionRequest.
        void ConnectionRequest(string message, string connectionNumberString)
        {
            //message wygląda tak: numerpodsieci:interfejsy;numerpodsieci:interfejsy...
            int connectionNumber;
            Int32.TryParse(connectionNumberString, out connectionNumber);
            string[] oneSplitMessage = message.Split(';'), splitArray;
            string subnetwork = " ", restMessage = " ";
            string capacityString = " ";
            //connections.Add(connectionNumber, message + "*" + oneSplitMessage.Length.ToString());
            //confirmations.Add(connectionNumber, 0);
            if (!connections.ContainsKey(connectionNumber))
                connections.Add(connectionNumber, new Dictionary<string, bool>());
            else
                connections[connectionNumber] = new Dictionary<string, bool>();
            foreach (KeyValuePair<string,string> kvpCC in capacity)
            {
                if (connectionNumberString.Equals(kvpCC.Key))
                    capacityString = kvpCC.Value;
            }
            foreach (string element in oneSplitMessage)
            {
                //Console.WriteLine("element in oneSplitMessage to: " + element);
                splitArray = element.Split(':');
                subnetwork = splitArray[0];
                restMessage = splitArray[1];

                foreach (KeyValuePair<string, string> kvpC in children)
                {
                    if (kvpC.Key.Equals(subnetwork))
                    {
                        int port;
                        Int32.TryParse(kvpC.Value, out port);
                        var client = new UdpClient();
                        IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                        client.Connect(point);
                        string messageOut = "ConnectionRequest_" + restMessage + "," + capacityString + "*" + connectionNumber.ToString();
                        client.Send(Encoding.UTF8.GetBytes(messageOut), Encoding.UTF8.GetBytes(messageOut).Length);
                        //var receivedData = client.Receive(ref point);
                        WriteLine("Wyslano ConnectionRequest do podsieci nr " + subnetwork + " o tresci " + messageOut);
                        connections[connectionNumber].Add(subnetwork, false);
                    }
                }

                Console.WriteLine(partners.Count);
                foreach (KeyValuePair<string, string> kvpP in partners)
                {
                    if (kvpP.Key.Equals(subnetwork))
                    {
                        int port;
                        Int32.TryParse(kvpP.Value, out port);
                        var client = new UdpClient();
                        IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                        client.Connect(point);
                        string messageOut = "PeerCoordination_" + restMessage + "," + capacityString + "*" + connectionNumber.ToString();
                        client.Send(Encoding.UTF8.GetBytes(messageOut), Encoding.UTF8.GetBytes(messageOut).Length);
                        //var receivedData = client.Receive(ref point);
                        WriteLine("Wyslano PeerCoordination do podsieci nr " + subnetwork + " o tresci " + restMessage);
                        connections[connectionNumber].Add(subnetwork, false);
                    }
                }
            }
        }
        void Partners(string subnetworkNumber)
        {
            try
            {
                var client = new UdpClient();
                IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), partnerPort);
                client.Connect(point);
                string message = "Partners_" + subnetworkNumber + "*" + udpListenPort;
                client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
            }
            catch (Exception e)
            { }
        }
        public static void WriteLine(String text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tCC: " + text);
        }
        public static void WriteRedLine(String text)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(GetTimestamp(DateTime.Now) + "\tCC: " + text);
        }
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd HH:mm:ss:ffff");
        }
    }
}