using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ChatCommunication
{
    [Serializable]
    public class User
    {
        public string username;
        public string password;
        readonly int id = 0;
        public static int idUser = 0;
        public bool isAuthentified = false;

        /// <summary>
        /// The current topic an user has joined
        /// </summary>
        Topic currentTopic = null;

        public User(string login, string password)
        {
            this.username = login;
            this.password = password;
            this.id = idUser;
            idUser++;
        }

        public Message CreateTopic(string name, List<User> invitedUsers)
        {
            var topic = new Topic(name);
            topic.users.AddRange(invitedUsers);
            Data.topicList.Add(topic);
            Console.WriteLine("> User list on this new server : ");
            foreach (var u in topic.users)
            {
                Console.WriteLine("User : " + u.username);
            }
            Console.WriteLine($"Topic '{name}' created successfully ");
            return new Message(null,$"The topic '{name}' has been created successfully");
        }

        // Must be used by the server
        public Message GetConversationOfTopic(string name)
        {
            StringBuilder conversation = new StringBuilder();
            conversation.AppendLine();
            List<ChatMessage> msgs = Data.topicList.Find(x=>x.Name.Equals(name)).chatMessages;
                if(msgs.Count > 0)
                {
                    foreach (var msg in msgs)
                    {
                        conversation.AppendLine($"[{msg.author.username}] {msg.content} ({msg.date})");
                    }
                }
                else
                {
                    conversation.AppendLine($" >> The conversation is empty for the topic '{name}'");
                }
                return new Message(User.GetBotUser(),conversation.ToString());
           // }
        }

        /// <summary>
        /// Server only
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public void SyncTopicsForHisClient(TcpClient client,Message msg)
        {
            msg.fullCommand = "sync topics | dqdqsd";
            msg.content = Data.topicList;
            msg.mustBeParsed = true;
            Net.SendMsg(client.GetStream(),msg);
        }

        private static User bot = new User("Bot","bot");

        public void SendMessageToUser(TcpClient comm, Message msg)
        {
            var userId = int.Parse(msg.GetArgument(ArgType.USERNAME));
            var chatMessage = msg.GetArgument(ArgType.MESSAGE);
            SendMessageToUser(comm.GetStream(), userId, chatMessage);
        }

        // 1 . The server receives the message from User 1
        // 2 (HERE) . The server redirects the messsage to User 2
        // To find User 2, we use its id
        private void SendMessageToUser(Stream destStream, int destUserId, string text)
        {
           var users =  User.GetAllUsers();
           var destUser = users.Find(u => u.id == destUserId);
            if(destUser == null)
            {
                Console.WriteLine("User not found");
            }
            else
            {
                new ChatMessage(DateTime.Now, this,text);
                Console.WriteLine("msg sent to user!");
                Net.SendMsg(
                    destStream,
                    new Message(this,"rcv user msg | " + text));
            }
           
        }

        public static User GetBotUser()
        {
            return bot;
        }

        public void SendMessageInTopic(TcpClient client,Message msg)
        {
            var topicName = msg.GetArgument(ArgType.NAME);
            var chatMessage = msg.GetArgument(ArgType.MESSAGE);
            SendMessageInTopic(client,topicName,chatMessage);
        }

        ///// <summary>
        ///// Client side operation
        ///// </summary>
        ///// <returns></returns>
        //public Message RefreshTopic(string topicName,string newMsg)
        //{

        //}

        public Message AddNewTopic(Message m, bool authSelf = true)
        {
            List<CommandArg> commands = m.GetArguments();

            List<User> invitedUsernames = new List<User>();
            Console.WriteLine($"'{m.user.username}' wants to create a topic");

            if(authSelf)
              invitedUsernames.Add(m.user);

            var userList = GetAllUsers();
            foreach (CommandArg arg in commands)
            {
                if(arg.key == ArgType.USERNAME)
                {
                    // We verify that the user exist
                    invitedUsernames.Add(userList.Find(x => x.username.Equals(arg.value)));
                }
            }

            var topicName = commands.Find(c => c.key == ArgType.NAME).value;
            return CreateTopic(topicName,invitedUsernames);
        }


        public static List<User> userList;

        public static List<User> GetAllUsers()
        {
            if(userList == null)
             userList = new List<User>()
            {
                new User("Marc","m"),
                new User("François","123")
            };

            return userList;
        }

        public Message ListTopics()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//////////// List of the Topics ///////////////");
            if(Data.topicList == null || Data.topicList.Count > 0)
            {
                int i = 1;
                foreach(Topic t in Data.topicList)
                {
                    var line = i + ")" + t.Name;
                    if (t.users.Find(x => x.id == this.id) == null)
                    {
                        line += " [UNAUTHORIZED]";
                    }
                    sb.AppendLine(line);
                    i++;
                }
            }
            else
            {
                sb.AppendLine(" (°u°) No topics are in the memory, create one with [create topic | n:the_name]");
            }
            sb.AppendLine("///////////////////////////////////////////////");
            return new Message(User.GetBotUser(),sb.ToString()); 
        }

        public Message EnterTopic(Message m)
        {
            var topicName = m.GetArgument(ArgType.NAME);
            return m.user.EnterTopic(topicName);
        }

        public Message EnterTopic(string topicName)
        {
            foreach(Topic t in Data.topicList)
            {
                if (t.Name.Equals(topicName))
                    return EnterTopic(t);
            }
            throw new NullReferenceException("Topic not found");
        }

        /// <summary>
        /// Server Side
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Message EnterTopic(Topic topic)
        {
            if(!topic.users.Contains(this))
            {
                topic.users.Add(this);
            }

            Console.WriteLine($"User {username} has entered the chat in the topic " + topic.Name);
            return new Message(GetBotUser(), $"view topic | n:{topic.Name} ");
        }

        public void SendMessageInTopic(TcpClient client, string name, string content)
        {
            foreach (Topic t in Data.topicList)
            {
                if (t.Name.Equals(name))
                {
                    t.AddMessageAndSync(client,new ChatMessage(DateTime.Now, this, content));
                }
            }
        }

        public void SendPrivateMessage(User user)
        {

        }

        
    }
}
