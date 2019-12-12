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

        public void Start(IPAddress iPAddress,int port)
        {
            TcpListener l = new TcpListener(iPAddress, port);
            l.Start();
            Console.WriteLine("Server Started ! ");
            while (true)
            {
                var comm = l.AcceptTcpClient();
                Console.WriteLine("Connection established for endpoint : " + comm.Client.RemoteEndPoint);
                Data.userClients.Add(new ClientUser { user = new User("guest", "guest"), tcpClient = comm});
                new Thread(new ReceiverServer(comm).doOperation).Start();
            }
        }
    }
}
