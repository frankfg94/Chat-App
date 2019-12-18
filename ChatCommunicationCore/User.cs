using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChatCommunication
{
    [Serializable]
    public class User
    {
        private byte[] imgData;
        public byte[] ImgData
        {
            get { return imgData; }
            set
            {
                if (value != imgData)
                {
                    imgData = value;
                }
            }
        }

        public static List<User> GetAllUsers()
        {
            if (userList == null)
                userList = new List<User>()
            {
                new User("Marc","m"),
                new User("François","123"),
                new User("Marine","Marine")
            };

            return userList;
        }

        public static List<User> userList;

        public event PropertyChangedEventHandler PropertyChanged;

        public string username { get; set; }

        public string addInfos { get; set; }

        /// <summary>
        /// Return the user that represents the server
        /// </summary>
        /// <returns></returns>
        public static User GetBotUser()
        {
            return bot;
        }


        public string password;
        public static int idUser = 0;
        public bool isAuthentified = false;
        private static User bot = new User("Bot","bot");

        public User(string login, string password)
        {
            this.username = login;
            this.password = password;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                User u = (User)obj;
                return u.username.Equals(u.username);
            }
        }


 
    }
}
