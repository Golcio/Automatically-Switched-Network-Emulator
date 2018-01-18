﻿using System;
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
        Dictionary<int, int> confirmations = new Dictionary<int, int>();
        int udpListenPort, subnetworkNumber, RCPort, partnerPort, parentPort;
        //int - numer połączenia, string - numer podsieci, bool - czy doszło potwierdzenie
        Dictionary<int, Dictionary<string, bool>> connections = new Dictionary<int, Dictionary<string, bool>>();

        public ConnectionController(int udpListenPort, int subnetworkNumber, int RCPort, int partnerPort, int parentPort)
        {
            this.udpListenPort = udpListenPort;
            this.subnetworkNumber = subnetworkNumber;
            this.RCPort = RCPort;
            this.partnerPort = partnerPort;
            this.parentPort = parentPort;
            FamilyTies();
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
                    Console.WriteLine(remoteEP.ToString());
                    Buffor.Push(Encoding.UTF8.GetString(data));
                    Console.WriteLine("Otrzymano wiadomosc o tresci " + Encoding.UTF8.GetString(data));
                    string response = "200 OK";
                    udpServer.Send(Encoding.UTF8.GetBytes(response), Encoding.UTF8.GetBytes(response).Length, remoteEP);
                }
            }
            catch (Exception e)
            {

            }
        }
        void AddingChildren(string subnetworkNumber, string remotePort)
        {
            children.Add(subnetworkNumber, remotePort);
            Console.WriteLine("Dodano CC z podsieci nr " + subnetworkNumber + " do zarządzanych CC.");
        }

        void AddingPartners(string subnetworkNumber, string remotePort)
        {
            partners.Add(subnetworkNumber, remotePort);
            Console.WriteLine("Dodano CC z podsieci nr " + subnetworkNumber + " do partnerujących CC.");
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
                    string connectionORportNumber = message.Split('_')[1].Split('*')[0]; 

                    switch (messageType)
                    {
                        case "FamilyTies":
                            Console.WriteLine("Otrzymano informacje o zarzadzanym CC podsieci nr " + restMessage);
                            AddingChildren(restMessage, connectionORportNumber);
                            break;
                        case "Partners":
                            Console.WriteLine("Otrzymano informacje o partnerujacym CC podsieci nr " + restMessage);
                            AddingPartners(restMessage, connectionORportNumber);
                            break;
                        case "ConnectionRequest":
                            Console.WriteLine("Otrzymano ConnectionRequest.");
                            //restMessage: punkt1,punkt2,przepustowosc
                            RouteQuery(restMessage, connectionORportNumber);
                            break;
                        case "RouteQuery":
                            Console.WriteLine("Otrzymano RouteQuery.");
                            //restMessage: numerPodsieci:punkt1,punkt2;numerPodsieci2:punkt1,punkt2
                            ConnectionRequest(restMessage, connectionORportNumber);
                            break;
                        case "PeerCoordination":
                            Console.WriteLine("Otrzymano PeerCoordination.");
                            RouteQuery(restMessage, connectionORportNumber);
                            break;
                        case "ConnectionConfirmation":
                            Console.WriteLine("Otrzymano ConnectionConfirmation.");
                            ConfirmationsController(restMessage, connectionORportNumber);
                            break;
                        case "WyjebanePolaczenie":

                            break;
                        case "koniec połączenia":
                            break;
                    }
                }
                Thread.Sleep(50);
            }
        }

        void ConfirmationsController(string restMessage, string connectionNumber)
        {
            string subnetworkNumber = restMessage.Split(':')[0];
            Int32.TryParse(connectionNumber, out int connectionNumb);
            connections[connectionNumb][subnetworkNumber] = true;
            Console.WriteLine("Otrzymano potwierdzenie zestawienia połączenia numer " + connectionNumber + " od CC w podsieci numer " + subnetworkNumber);
        }

        //RC zestaw mi połączenie pomiędzy tymi dwoma punktami: w wiadomości przekazywane są dwa punkty.
        void RouteQuery(string routeQuery, string connectionNumber)
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
                    string message = "RouteQuery_" + routeQuery + "*" + connectionNumber;
                    client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
                    var receivedData = client.Receive(ref point);
                    Console.WriteLine("Otrzymano potwierdzenie wysłania RouteQuery. ");
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
                //var receivedData = client.Receive(ref point);
                Console.WriteLine("Otrzymano potwierdzenie wysłania FamilyTies. ");
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
            //connections.Add(connectionNumber, message + "*" + oneSplitMessage.Length.ToString());
            //confirmations.Add(connectionNumber, 0);
            foreach (string element in oneSplitMessage)
            {
                Console.WriteLine("element in oneSplitMessage to: " + element);
                splitArray = element.Split(':');
                subnetwork = splitArray[0];
                restMessage = splitArray[1];
                connections.Add(connectionNumber, new Dictionary<string, bool>());
                
                foreach (KeyValuePair<string, string> kvpC in children)
                {
                    if (kvpC.Key == subnetwork)
                    {
                        int port;
                        Int32.TryParse(kvpC.Value, out port);
                        var client = new UdpClient();
                        IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                        client.Connect(point);
                        string messageOut = "ConnectionRequest_" + restMessage + "*" + connectionNumber.ToString();
                        client.Send(Encoding.UTF8.GetBytes(messageOut), Encoding.UTF8.GetBytes(messageOut).Length);
                        var receivedData = client.Receive(ref point);
                        Console.WriteLine("Wyslano ConnectionRequest do podsieci nr " + subnetwork + " o tresci " + restMessage);
                        connections[connectionNumber].Add(subnetwork, false);
                    }
                    else
                    {
                        foreach (KeyValuePair<string, string> kvpP in partners)
                        {
                            int port;
                            Int32.TryParse(kvpP.Value, out port);
                            var client = new UdpClient();
                            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                            client.Connect(point);
                            string messageOut = "PeerCoordination_" + restMessage + "*" + connectionNumber.ToString();
                            client.Send(Encoding.UTF8.GetBytes(messageOut), Encoding.UTF8.GetBytes(messageOut).Length);
                            var receivedData = client.Receive(ref point);
                            Console.WriteLine("Wyslano PeerCoordination do podsieci nr " + subnetwork + " o tresci " + restMessage);
                            connections[connectionNumber].Add(subnetwork, false);
                        }
                    }
                }            }
            
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
                //var receivedData = client.Receive(ref point);
                WriteLine("Otrzymano potwierdzenie partnerstwa. ");
            } 
            catch(Exception e)
            {  }
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