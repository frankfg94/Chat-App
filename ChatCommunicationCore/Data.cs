using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatCommunication
{
    public class Data
    {
        public static List<Topic> topicList = new List<Topic>();
        public static List<User> users = new List<User>();
        public static readonly List<TcpClient> clients = new List<TcpClient>();
    }
}
