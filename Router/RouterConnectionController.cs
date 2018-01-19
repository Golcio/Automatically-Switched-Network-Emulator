﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Router
{
    class RouterConnectionController
    {
        //nr portu na którym słucha CC
        int udpListenPort;
        int parentPort;
        //Buffor wiadommości, które przychodzą na powyższy port
        Stack<string> Buffor = new Stack<string>();
        int subnetworkNumber;
        int lrmPort;
        Dictionary<int, string> connections = new Dictionary<int, string>();

        public RouterConnectionController(int routerNumber, int udpListenPort, int parentPort, int lrmPort)
        {
            //dwa podstawowe wątki, czyli odbieranie żądań i analizowanie ich.
            this.subnetworkNumber = routerNumber;
            this.udpListenPort = udpListenPort;
            this.parentPort = parentPort;
            this.lrmPort = lrmPort;
            FamilyTies();
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
                Buffor.Push(Encoding.UTF8.GetString(data));
                WriteLine("Otrzymano wiadomosc o tresci " + Encoding.UTF8.GetString(data));
                string response = "200 OK";
                udpServer.Send(Encoding.UTF8.GetBytes(response), Encoding.UTF8.GetBytes(response).Length, remoteEP);
            }
        }

        void analizing()
        {
            while (true)
            {
                if (Buffor.Count > 0)
                {
                    //Wiadomość wygląda następująco: "messageType_restMessage*connectionNumber
                    //Interesujące nas elementy to: messageType, restMessage oraz connectionNumber
                   
                    string message = Buffor.Pop();
                    string messageType = message.Split('_')[0];
                    string restMessage = message.Split('_')[1].Split('*')[0];
                    string connectionORportNumber = message.Split('_')[1].Split('*')[1];

                    switch (messageType)
                    {
                        case "ConnectionRequest":
                            WriteLine("Otrzymano ConnectionRequest.");
                            LinkConnectionRequest(restMessage, connectionORportNumber);
                            break;
                        case "LinkConnectionRequestConfirm":
                            ConnectionConfirmation(restMessage, connectionORportNumber);
                            break;
                    }
                }
                Thread.Sleep(50);
            }
        }
        

        void ConnectionConfirmation(string restMessage, string connectionNumber)
        {
            var client = new UdpClient();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), parentPort);
            client.Connect(point);
            string message = "ConnectionConfirmation_" + restMessage + "*" + connectionNumber;
            client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
        }
        //Jeżeli CC jest CC routerowym to wtedy ConnectionRequest przesyłane jest do LRMa, 
        //który ma za zadanie zestawić połączenie.
        void LinkConnectionRequest(string linkRequest, string connectionNumberGiven)
        {
            var client = new UdpClient();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), lrmPort);
            client.Connect(point);
            string message = "LinkConnectionRequest_" + linkRequest + "*" + connectionNumberGiven;
            client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
        }

        //Wysyłanie wiadomosci "elo ziom, zarządzam podsiecią nr "subnetworkNumber", 
        //a ty zarządzasz mną. Jak coś to siędzę na tym porcie, elo pis joł
        void FamilyTies()
        {
            var client = new UdpClient();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), parentPort);
            client.Connect(point);
            string message = "FamilyTies_" + subnetworkNumber.ToString() + "*" + udpListenPort.ToString();
            client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);

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
