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

        public string username { get; set; }

        public string addInfos { get; set; }


        public string password;
        readonly int id = 0;
        public static int idUser = 0;
        public bool isAuthentified = false;

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

        public void CreateTopic(string name, string description, List<User> invitedUsers, TcpClient creatorComm = null)
        {
            
                var topic = new Topic(name);
                topic.users.AddRange(invitedUsers);
                if (description != null)
                    topic.Description = description;
                Data.topicList.Add(topic);
                Console.WriteLine($"Topic '{name}' created successfully ");
                Console.WriteLine("> Sending the topic to each user : ");
                if(creatorComm != null)
                      Net.SendMsg(creatorComm.GetStream(), new Message(User.GetBotUser(), $"Your topic '{name}' has been created successfully"));
                foreach (var u in Data.userClients)
                {
                    Net.SendMsg(u.tcpClient.GetStream(), new Message(User.GetBotUser(), $"sync topic | data") {mustBeParsed = true, content = topic });
                    Console.WriteLine("Sent to : " + u.user.username);
                }
        }

        public void CreateTopicAndNotifyAll(string name, List<User> invitedUsers)
        {
                var topic = new Topic(name);
                topic.users.AddRange(invitedUsers);
                Data.topicList.Add(topic);
                Console.WriteLine("> User list on this new server : ");
                foreach (var u in Data.userClients)
                {
                    Net.SendMsg(u.tcpClient.GetStream(),new Message(User.GetBotUser(),"rcv new topic | data"));
                }
                Console.WriteLine($"Topic '{name}' created successfully ");
        }

        // Must be used by the server
        public Message GetConversationOfTopic(string name)
        {
            Topic t = Data.topicList.Find(x => x.Name.Equals(name));
            StringBuilder conversation = new StringBuilder();
            conversation.AppendLine();
                if(t.chatMessages.Count > 0)
                {
                    foreach (var msg in t.chatMessages)
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
            msg.fullCommand = "sync topics | data";
            msg.content = Data.topicList;
            msg.mustBeParsed = true;
            Net.SendMsg(client.GetStream(),msg);
        }

        public void SyncTopicForHisClient(TcpClient comm, Message msg, string topicName)
        {
            msg.fullCommand = $"sync topic | n:{topicName}";
            msg.content = Data.topicList.Find(x=>x.Name.Equals(topicName));
            msg.mustBeParsed = true;
            Net.SendMsg(comm.GetStream(), msg);
        }

        private static User bot = new User("Bot","bot");

        // msg user | u:William m:coucou
        public void SendMessageToUser( Message msg)
        {
            var destUsername = msg.GetArgument(ArgType.USERNAME); 
            var messageContent = msg.GetArgument(ArgType.MESSAGE);
            TcpClient destClient = Data.RetrieveClientFromUsername(destUsername);
            var users = GetAllUsers();
            var destUser = users.Find(u => u.username.Equals(destUsername));
            var msgId = GetNewMsgIdSafe();

            // Send to the user
            SendMessageToUser(destClient.GetStream(), msg.author, destUser, messageContent,msgId);

            // Send the message to self
            if(!destUsername.Equals(msg.author.username))
                SendMessageToUser(Data.RetrieveClientFromUsername(msg.author.username).GetStream(), msg.author,destUser, messageContent,msgId);
        }

        // 1 . The server receives the message from User 1
        // 2 (HERE) . The server redirects the messsage to User 2
        // To find User 2, we use its id
        private void SendMessageToUser(Stream destStream,User sender, User destUser, string msgContent, int mId)
        {
                var chatMsg = new ChatMessage(DateTime.Now, sender, destUser.username, msgContent) { id = mId };
                Net.SendMsg( destStream, new Message(destUser, $"rcv user msg | data") { content = chatMsg, mustBeParsed = true});
                Console.WriteLine("msg sent to user : " + destUser.username);
        }

        public void UpdateTopic(Message msg)
        {
            var topicName = msg.GetArgument(ArgType.NAME);
            var topicToEdit = Data.topicList.Find(x => x.Name.Equals(topicName));
            topicToEdit.Name = msg.GetArgument(ArgType.NEW_NAME);
            topicToEdit.Description = msg.GetArgument(ArgType.DESCRIPTION);

            foreach (var tcpUser in Data.userClients)
            {
                Net.SendMsg(tcpUser.tcpClient.GetStream(),
                    new Message(GetBotUser(),$"sync topic | n:{topicName} ") { mustBeParsed = true, content = topicToEdit }) ;
            }
        }

        public void DeleteTopic(Message msg)
        {
              var topicName = msg.GetArgument(ArgType.NAME);
              Data.topicList.RemoveAll(x => x.Name.Equals(topicName));

            // Synchronize all the clients to remove the topic
             var resp = new Message(GetBotUser(),"sync topics | data");
             resp.content = Data.topicList;
             resp.mustBeParsed = true;
             foreach (var tcpUser in Data.userClients)
             {
                    Net.SendMsg(tcpUser.tcpClient.GetStream(), resp);
             }
             Console.WriteLine("topic deleted, topics synced with clients");
        }

        /// <summary>
        /// Return the user that represents the server
        /// </summary>
        /// <returns></returns>
        public static User GetBotUser()
        {
            return bot;
        }

        public void DeleteMsg(string idString)
        {
            int msgIdToDel = int.Parse(idString);
            SearchAndRemoveMessage(msgIdToDel,Data.topicList);
        }

        public void EditMsgAndSend(string idString, string newContent)
        {
            int msgIdToEdit = int.Parse(idString);
            SearchAndEditMessage(msgIdToEdit, newContent, Data.topicList);
        }

        /// <summary>
        /// Replace the message with the id of msg variable with the content of the msg message
        /// </summary>
        /// <param name="msg"></param>
        public void EditMsgAndSend(ChatMessage msg)
        {
            SearchAndEditMessage(msg.id, msg.content, Data.topicList, msg);
        }

        private void SearchAndEditMessage(int msgIdToEdit, string newContent, List<Topic> topicList, ChatMessage replaceContent = null)
        {
            Topic editedTopic = null;
            foreach (var topic in topicList)
            {
                foreach (var msg in topic.chatMessages.ToList())
                {
                    if (msg.id == msgIdToEdit)
                    {
                        topic.chatMessages.Remove(msg);
                        topic.chatMessages.Add(new ChatMessage(msg.date, msg.author, msg.destName, newContent) { id = msgIdToEdit});
                        editedTopic = topic;
                    }
                }
            }
            // We only send the message to the users that need to be updated (that are in the topic)
            if (editedTopic != null)
            {
                foreach (var user in editedTopic.users)
                {
                    Net.SendMsg(Data.RetrieveClientFromUsername(user.username).GetStream()
                        , new Message(GetBotUser(), $"edit msg | id:{msgIdToEdit} m:{newContent}") { mustBeParsed = true, content = replaceContent });
                    Console.WriteLine($"Sent to {user.username}:  edit msg | id:{msgIdToEdit} m:{newContent}");
                }
            }
            else
            {
                foreach (var user in Data.userClients)
                {
                    Net.SendMsg(user.tcpClient.GetStream()
                        , new Message(GetBotUser(), $"edit msg | id:{msgIdToEdit}  m:{newContent}") { mustBeParsed = true, content = replaceContent });
                    Console.WriteLine($"Sent to {user.user.username}:  edit msg | id:{msgIdToEdit} m:{newContent}");
                }
            }
        }

        private void SearchAndRemoveMessage(int msgIdToDel, List<Topic> topicList)
        {
            Topic editedTopic = null;
            foreach (var topic in topicList)
            {
                foreach (var msg in topic.chatMessages.ToList())
                {
                    if (msg.id == msgIdToDel)
                    {
                        topic.chatMessages.Remove(msg);
                        editedTopic = topic;
                    }
                }
            }

            if(editedTopic!=null)
                foreach (var user in editedTopic.users)
                {
                    Net.SendMsg(Data.RetrieveClientFromUsername(user.username).GetStream()
                        ,new Message(GetBotUser(), $"rmv msg | id:{msgIdToDel}") { mustBeParsed = true });
                }
            else
            {
                foreach (var user in Data.userClients)
                {
                    Net.SendMsg(user.tcpClient.GetStream()
                        , new Message(GetBotUser(), $"rmv msg | id:{msgIdToDel}") { mustBeParsed = true });
                }
            }
            
        }

        public void SendMessageInTopic(Message msg)
        {
            var topicName = msg.GetArgument(ArgType.NAME);
            var textContent = msg.GetArgument(ArgType.MESSAGE);

            if(msg.content != null && msg.content is ImageChatMessage chatMsg)
                SendMessageInTopic(topicName,textContent, chatMsg);
            else
                SendMessageInTopic(topicName,textContent);
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

        public void AddNewTopic(Message m, bool autoJoin = true)
        {

            List<User> invitedUsernames = new List<User>();
            Console.WriteLine($"'{m.author.username}' wants to create a topic");

            if(autoJoin)
              invitedUsernames.Add(m.author);

           var description = m.GetOptionalArgument(ArgType.DESCRIPTION);
           var topicName = m.GetArgument(ArgType.NAME);
           var creatorTcp = Data.RetrieveClientFromUsername(m.author.username);
           CreateTopic(topicName, description ,invitedUsernames, creatorTcp );
        }

        public void Disconnect(TcpClient requester, Message m)
        {
            var usToDisconnect = m.author;
            usToDisconnect.isAuthentified = false;
            foreach (var userTcp in Data.userClients)
            {
                if(userTcp.user.username.Equals(usToDisconnect.username))
                {
                    userTcp.user = null;
                }
            }
            foreach (var user in User.userList)
            {
                if(user.username.Equals(usToDisconnect.username))
                {
                    user.isAuthentified = false;
                }
            }
            Console.WriteLine("User : " + m.author.username + " is now disconnected");
        }

        public void SendAudioMsgToTopic(Message msg)
        {
            var destTopicName = msg.GetArgument(ArgType.NAME);
            var audioData = msg.content as byte[];
            var topic = Data.topicList.Find(x=>x.Name.Equals(destTopicName));
            foreach (var user in topic.users)
            {
                SendAudioMsgToUser(user.username,audioData);
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

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

        public Message SendTopicsText()
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

        private static object lockSendMsg = new object();
        private static object lockEnterTopic = new object();
        public Message EnterTopic(Message m)
        {
            var topicName = m.GetArgument(ArgType.NAME);
            return m.author.EnterTopic(topicName);
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
        /// Use in the server side to not have to transfer data
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Message EnterTopic(Topic topic)
        {
            lock(lockEnterTopic)
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
        }

        public static int msgGlobalId = 0;
       
        private int GetNewMsgIdSafe()
        {
            lock(lockSendMsg)
            {
                return msgGlobalId++;
            }
        }

        public void SendMessageInTopic(string name, string content, ChatMessage senderChatMsg = null)
        {
            foreach (Topic t in Data.topicList)
            {
                if (t.Name.Equals(name))
                {
                        lock(lockSendMsg)
                        {
                            if(senderChatMsg == null)
                                t.AddMessageAndSync(new ChatMessage(DateTime.Now, this, name, content) { id = GetNewMsgIdSafe()});
                            else
                            {
                                senderChatMsg.id = GetNewMsgIdSafe();
                                t.AddMessageAndSync(senderChatMsg);
                            }
                        }
                }
            }
        }
 
    }
}
