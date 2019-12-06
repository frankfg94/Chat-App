using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatCommunication
{
    public class Server : CommunicatorBase
    {

        public static List<Topic> topicList = new List<Topic>();
        public static List<User> users = new List<User>();
        public static readonly List<TcpClient> clients = new List<TcpClient>();

        public void Start(IPAddress iPAddress,int port)
        {
            TcpListener l = new TcpListener(iPAddress, port);
            l.Start();
            Console.WriteLine("Server Started ! ");
            while (true)
            {
                var comm = l.AcceptTcpClient();
                Console.WriteLine("Connection established for endpoint : " + comm.Client.RemoteEndPoint);
                clients.Add(comm);
                new Thread(new ReceiverServer(comm).doOperation).Start();
            }
        }


    }
}
