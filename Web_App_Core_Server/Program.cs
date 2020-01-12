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


            // Sample data
            var userList = new List<User>()
            {
                new User("brad","brad"){ImgData = Properties.Resources.radbrad },
                new User("François","123") { ImgData = Properties.Resources.Francois},
                new User("keanu", "k") { ImgData = Properties.Resources.keanu},
                new User("Nicolas", "gb") { ImgData = Properties.Resources.Nicolas}
            };
            User.userList = userList;
            DisplayUserList(userList);
            server.Start(new IPAddress(new byte[] { 127, 0, 0, 1 }), 8976);
        }

        /// <summary>
        /// For testing purposes
        /// </summary>
        /// <param name="userList"></param>
        private static void DisplayUserList(List<User> userList)
        {
            Console.WriteLine("/////// User list /////" + Environment.NewLine + "Use these credentials on a client to connect yourself");
            foreach (var u in userList)
            {
                Console.WriteLine("name : " + u.username + " / password :  " + u.password);
            }
        }
    }
}
