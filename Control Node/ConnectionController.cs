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
        static int udpListenPort = 11002;
        static Stack<string> Buffor = new Stack<string>();
        public ConnectionController()
        {
            Thread receiveThread = new Thread(() => receiving());
            receiveThread.Start();
        }
        static void receiving()
        {
            UdpClient udpServer = new UdpClient(udpListenPort);
            while (true)
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, udpListenPort);
                var data = udpServer.Receive(ref remoteEP);
                Buffor.Push(Encoding.UTF8.GetString(data));
                Console.WriteLine("Otrzymano wiadomosc od " + remoteEP.ToString() + " o tresci " + Encoding.UTF8.GetString(data));
                string response = "200 OK";
                udpServer.Send(Encoding.UTF8.GetBytes(response), Encoding.UTF8.GetBytes(response).Length, remoteEP);
            }
        }

        void analizing()
        {
            string message = Buffor.Pop();
            string[] oneSplitMessage = message.Split('_');
            string messageType = oneSplitMessage[0], restMessage = oneSplitMessage[1];
            switch (messageType)
            {
                case "ConnectionRequest":
                    Console.WriteLine("Otrzymano ConnectionRequest.");
                    RouteTableQuery(restMessage);
                    break;
                case "RouteQuery":
                    Console.WriteLine("Otrzymano RouteQuery.");
                    LinkConnectionRequest(restMessage);
                    break;
                case "PeerCoordination":
                    Console.WriteLine("Otrzymano PeerCoordination.");
                    break;
            }
        }

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

        void LinkConnectionRequest(string linkRequest)
        {
            var client = new UdpClient();
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15002);
            client.Connect(point);
            string message = "LinkConnectionRequest_" + linkRequest;
            client.Send(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(message).Length);
            var receivedData = client.Receive(ref point);
            Console.WriteLine("Otrzymano potwierdzenie wysłania LinkConnectionRequest. ");
        }
    }
}
