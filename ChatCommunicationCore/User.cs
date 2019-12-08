using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        /// The current topic an user is in
        /// </summary>
        Topic currentTopic = null;

        public User(string login, string password)
        {
            this.username = login;
            this.password = password;
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

        // msg user | u:William m:coucou
        public void SendMessageToUser( Message msg)
        {
            var username = msg.GetArgument(ArgType.USERNAME); // william
            var chatMessage = msg.GetArgument(ArgType.MESSAGE); // coucou
            TcpClient destClient = Data.RetrieveClientFromUsername(username); // william
            SendMessageToUser(destClient.GetStream(), username, chatMessage);
        }

        // 1 . The server receives the message from User 1
        // 2 (HERE) . The server redirects the messsage to User 2
        // To find User 2, we use its id
        private void SendMessageToUser(Stream destStream, string username, string msgContent)
        {
           var users =  User.GetAllUsers();
           var destUser = users.Find(u => u.username.Equals(username));
            if (destUser == null)
            {
                throw new NullReferenceException($"Destination User '{username}' not found");
            }
            else
            {
                var chatMsg = new ChatMessage(DateTime.Now, this, msgContent);
                Net.SendMsg(
                    destStream,
                    new Message(this, $"rcv user msg | data") { content = chatMsg, mustBeParsed = true});
                Console.WriteLine("msg sent to user : " + destUser.username);
            }
           
        }

   

        /// <summary>
        /// Return the user that represents the server
        /// </summary>
        /// <returns></returns>
        public static User GetBotUser()
        {
            return bot;
        }

        public void SendMessageInTopic(Message msg)
        {
            var topicName = msg.GetArgument(ArgType.NAME);
            var chatMessage = msg.GetArgument(ArgType.MESSAGE);
            SendMessageInTopic(topicName,chatMessage);
        }

        public void SendFileToUser(Message msg)
        {
            SendFileToUser(msg.GetArgument(ArgType.USERNAME),msg.content as byte[], msg.GetArgument(ArgType.NAME));
        }

        public void SendFileToUser(string destUsername, byte[] data, string nameWithFormat)
        {
           TcpClient destClient = Data.RetrieveClientFromUsername(destUsername);
            Console.WriteLine("Data to send size : " + data.Length);
            Net.SendMsg(
                    destClient.GetStream(),
                    new Message(this, $"rcv file user | n:{nameWithFormat}") { content = data, mustBeParsed = true });
            Console.WriteLine("File sent to the user "+ destUsername +" with a byte array : " + nameWithFormat);
        }


        public void SendFileInTopic(Message msg)
        {
            SendFileInTopic(msg.GetArgument(ArgType.NAME), msg.GetArgument(ArgType.FILENAME_WITH_FORMAT), msg.content as byte[]);
        }

        private void SendFileInTopic(string topicName, string fileName, byte[] fileData)
        {
            Console.WriteLine($"Data to send size to the topic {topicName} : " + fileData.Length);
            Topic t = Data.topicList.Find(x => x.Name.Equals(topicName));
            foreach (var user in t.users)
            {
                TcpClient destClient = Data.RetrieveClientFromUsername(user.username);
                Net.SendMsg(
                        destClient.GetStream(),
                        new Message(this, $"rcv file user | n:{fileName}") { content = fileData, mustBeParsed = true });
                Console.WriteLine("File sent to the user " + user.username + " with a byte array : " + fileName);
            }
        }

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

        public void SendAudioMsgToUser(Message msg)
        {
            var destUsername = msg.GetArgument(ArgType.USERNAME);
            var audioData = msg.content as byte[];
            SendAudioMsgToUser(destUsername,audioData);
        }

        private void SendAudioMsgToUser(string destUsername, byte[] audioData)
        {
            var client = Data.RetrieveClientFromUsername(destUsername);
            Message m = new Message(this, $"rcv audio | data") { mustBeParsed = true};
            m.content = audioData;
            Net.SendMsg(client.GetStream(), m);
        }

        public static List<User> userList;

        public static List<User> GetAllUsers()
        {
            if(userList == null)
             userList = new List<User>()
            {
                new User("Marc","m"),
                new User("François","123"),
                new User("Marine","Marine")
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
                    var line = $"{i}) {t.Name} ({t.chatMessages.Count} msgs)" ;
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
            if (topic.users.Find(x => x.username.Equals(username)) == null)
            {
                topic.users.Add(this);
                Console.WriteLine($"User {username} has entered the chat in the topic " + topic.Name);
                return GetConversationOfTopic(topic.Name);
            }
            else
            {
                return new Message(this, "You have already joined this topic") { mustBeParsed = false};
            }

        }

        public void SendMessageInTopic(string name, string content)
        {
            foreach (Topic t in Data.topicList)
            {
                if (t.Name.Equals(name))
                {
                    t.AddMessageAndSync(new ChatMessage(DateTime.Now, this, content));
                }
            }
        }
 
    }
}
