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
        public static readonly List<ClientUser> userClients = new List<ClientUser>();
        public static TcpClient RetrieveClientFromUsername(string username)
        {
            User u = User.GetAllUsers().Find(x => x.username.Equals(username));
            if (u != null && u.isAuthentified)
            {
                foreach (var userClient in userClients)
                {
                    if (userClient.user.username.Equals(username))
                        return userClient.tcpClient;
                }
            }
            return null;
        }
    }    

    public class ClientUser
    {
        public TcpClient tcpClient;
        public User user;
    }

}
