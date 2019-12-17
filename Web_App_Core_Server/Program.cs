using ChatCommunication;
using System;
using System.Collections.Generic;
using System.Net;

namespace Web_App_Core_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            var userList = new List<User>()
            {
                new User("Marc","m"){ImgData = Properties.Resources.Marc },
                new User("François","123") { ImgData = Properties.Resources.Francois},
                new User("Marine", "Marine") { ImgData = Properties.Resources.Marine},
                new User("Nicolas", "gb") { ImgData = Properties.Resources.Nicolas}
            };
            User.userList = userList;
            server.Start(new IPAddress(new byte[] { 127, 0, 0, 1 }), 8976);
        }
    }
}
