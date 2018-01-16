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
        bool routerCC;
        int udpListenPort = 11002;
        Stack<string> Buffor = new Stack<string>();
        Dictionary<string, string> children = new Dictionary<string, string>();
        public ConnectionController()
        {
            Thread receiveThread = new Thread(() => receiving());
            receiveThread.Start();
            Thread analizeThread = new Thread(() => analizing());
            analizeThread.Start();

        }
        void receiving()
        {
            UdpClient udpServer = new UdpClient(udpListenPort);
            while (true)
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, udpListenPort);
                var data = udpServer.Receive(ref remoteEP);
                Console.WriteLine(remoteEP.ToString());
                Buffor.Push(Encoding.UTF8.GetString(data) + "#" + remoteEP);
                Console.WriteLine("Otrzymano wiadomosc od " + remoteEP.ToString() + " o tresci " + Encoding.UTF8.GetString(data));
                string response = "200 OK";
                udpServer.Send(Encoding.UTF8.GetBytes(response), Encoding.UTF8.GetBytes(response).Length, remoteEP);
            }
        }
        void AddingChildren(string subnetworkNumber, string remotePort)
        {
            children.Add(subnetworkNumber, remotePort);
        }
        void analizing()
        {
            while (true)
            {
                if (Buffor.Count > 0)
                {
                    string message = Buffor.Pop();
                    string[] oneSplitMessage = message.Split('_'), secondSplitMessage = oneSplitMessage[1].Split('#');
                    string messageType = oneSplitMessage[0];
                    string restMessage = secondSplitMessage[0], remotePort = secondSplitMessage[1].Split(';')[1];

                    switch (messageType)
                    {
                        case "FamilyTies":
                            Console.WriteLine("Otrzymano informacje o zarzadzanym CC podsieci nr " + restMessage);
                            AddingChildren(restMessage, remotePort);
                            break;
                        case "ConnectionRequest":
                            Console.WriteLine("Otrzymano ConnectionRequest.");
                            if (routerCC)
                                RouteTableQuery(restMessage);
                            else
                                LinkConnectionRequest(restMessage);
                            break;
                        case "RouteQuery":
                            Console.WriteLine("Otrzymano RouteQuery.");
                            ConnectionRequest(restMessage);
                            break;
                        case "PeerCoordination":
                            Console.WriteLine("Otrzymano PeerCoordination.");
                            break;
                    }
                }
            }
        }

        //RC zestaw mi połączenie pomiędzy tymi dwoma punktami: w wiadomości przekazywane są dwa punkty.
        void RouteTableQuery(string routeQuery)
        {
            var client = new UdpClient();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15002);
            client.Connect(point);
            string message = "RouteTableQuery_" + routeQuery;
            client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
            var receivedData = client.Receive(ref point);
            Console.WriteLine("Otrzymano potwierdzenie wysłania RouteQuery. ");
        }

        //Jeżeli CC jest CC routerowym to wtedy ConnectionRequest przesyłane jest do LRMa, 
        //który ma za zadanie zestawić połączenie.
        void LinkConnectionRequest(string linkRequest)
        {
            var client = new UdpClient();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15002);
            client.Connect(point);
            string message = "LinkConnectionRequest_" + linkRequest;
            client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
            var receivedData = client.Receive(ref point);
            Console.WriteLine("Otrzymano potwierdzenie wysłania LinkConnectionRequest.");
        }

        //Wysyłanie wiadomosci "elo ziom, zarządzam podsiecią nr "subnetworkNumber", 
        //a ty zarządzasz mną. Jak coś to siędzę na tym porcie, elo pis joł
        void FamilyTies(string subnetworkNumber)
        {
            var client = new UdpClient();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15002);
            client.Connect(point);
            string message = "FamilyTies_" + subnetworkNumber;
            client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
            var receivedData = client.Receive(ref point);
            Console.WriteLine("Otrzymano potwierdzenie wysłania FamilyTies. ");
            
        }

        //w momencie, gdy dostaniemy wiadomość od RC z podsieciami pomiędzy którymi 
        //będzie przebiegało połączenie wysyłamy do nich żądanie zestawienia połączenia ConnectionRequest.
        //Funkcja analizuje wiadomość: jakie sieci biorą udział w połączeniu, jakie podsieci powiadomić.
        //Wysyła do nich ConnectionRequest.
        void ConnectionRequest(string message)
        {
            string[] oneSplitMessage = message.Split(';'), splitArray;
            string subnetwork = " ", restMessage = " ";

            foreach (string element in oneSplitMessage)
            {
                Console.WriteLine("element in oneSplitMessage to: " + element);
                splitArray = element.Split(':');
                subnetwork = splitArray[0];
                restMessage = splitArray[1];
                foreach (KeyValuePair<string, string> kvp in children)
                {
                    if (kvp.Key == subnetwork)
                    {
                        int port;
                        Int32.TryParse(kvp.Value, out port);
                        var client = new UdpClient();
                        IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                        client.Connect(point);
                        string messageOut = "ConnectionRequest_" + restMessage;
                        client.Send(Encoding.UTF8.GetBytes(messageOut), Encoding.UTF8.GetBytes(messageOut).Length);
                        var receivedData = client.Receive(ref point);
                        Console.WriteLine("Wyslano ConnectionRequest do podsieci nr " + subnetwork + " o tresci " + restMessage);
                    }
                }
            }
        }
    }
}
