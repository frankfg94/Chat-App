using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatCommunication
{
    public class ReceiverServer : CommunicatorBase
    {
        private TcpClient comm;

        public ReceiverServer(TcpClient comm)
        {
            this.comm = comm;
        }

        bool run = true;
        public static int msgGlobalId = 0;

        public void doOperation()
        {
            while (run)
            {
                try
                {
                    if (!comm.Connected)
                    {
                        Console.WriteLine("Client disconnected from the server : " + comm.Client.RemoteEndPoint);
                        comm.Dispose();
                        run = false;
                    }
                    else
                    {
                        Message data = Net.RcvMsg(comm.GetStream());
                        if (data is Message msg)
                        {
                            msg.Parse();
                            Console.WriteLine("Signal received : " + msg.fullCommand);
                            if (msg.author.isAuthentified)
                            {
                                doOperationsAsUser(msg, msg.CommandPart, comm);
                            }
                            else
                            {
                                LoginOrSubscribeAsUser(msg);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (comm.Connected)
                    {
                        Console.WriteLine("Invalid command format " + ex);
                        Console.WriteLine(ex.StackTrace);
                        Net.SendMsg(comm.GetStream(), new Message(User.GetBotUser(), "Invalid command format " + ex.Message));
                        Console.WriteLine("--> Sent an invalid command format message to the client");
                    }
                }
            }
        }

        // Sending a response to the client whether the connection is sucessful or not
        private void LoginOrSubscribeAsUser(Message msg)
        {
            // Create User
            if (msg.fullCommand.StartsWith("subscribe"))
            {
                Message respToTheClient;
                if (TryCreateUser(msg.GetArgument(ArgType.USERNAME), msg.GetArgument(ArgType.PASSWORD), out string errMsg))
                    respToTheClient = new Message(User.GetBotUser(), $"Your user has been created successfully, please connect");
                else
                    respToTheClient = new Message(User.GetBotUser(), $"(!) Couldn't create an user ({errMsg})");
                respToTheClient.mustBeParsed = false;
                Net.SendMsg(comm.GetStream(), respToTheClient);
            }
            else
            {
                // Verify that we just want to connect ourselves first
                if (!msg.fullCommand.StartsWith("connect"))
                {
                    var firstMes = new Message(User.GetBotUser(), "You have to connect yourself first before using any other command. Use : connect | u:{username} p:{password}");
                    firstMes.mustBeParsed = false;
                    Net.SendMsg(comm.GetStream(), firstMes);
                    return;
                }
                if (VerifyCredentials(msg.GetArgument(ArgType.USERNAME), msg.GetArgument(ArgType.PASSWORD), out string textMsg, out User connectedUser))
                {
                    connectedUser.isAuthentified = true;

                    // Updating the user
                    Data.userClients.Find(x => x.tcpClient.Client.Equals(comm.Client)).user = connectedUser;

                    Console.WriteLine("Connection authorized! sending the connected user to the client");
                }
                else
                {
                    Console.WriteLine("A client failed to connect itself to the server, Reason : " + textMsg);
                }
                var respToTheClient = new Message(User.GetBotUser(), $"auth status | m:{textMsg}");
                respToTheClient.mustBeParsed = true;
                respToTheClient.content = connectedUser;
                Net.SendMsg(comm.GetStream(), respToTheClient);
            }

        }

        private static object lockCreateUser = new object();
        private bool TryCreateUser(string username, string password, out string errMsg)
        {
            lock(lockCreateUser)
            {
                foreach (var user in User.GetAllUsers())
                {   
                    if (user.username.Equals(username))
                    {
                        errMsg = "User already exists";
                        return false;
                    }
                }
                User u = new User(username, password);
                User.userList.Add(u);
                errMsg = string.Empty;
                return true;
            }
        }

        public void doOperationsAsUser(Message msg, string commandLine, TcpClient comm)
        {
            switch (msg.CommandPart)
            {
                case "sync user list":
                    SendUsersToAllClients();
                    msg = null;
                    break;
                case "create topic":
                    AddNewTopic(msg);
                    msg = null;
                    break;
                case "delete topic":
                    DeleteTopic(msg);
                    msg = null;
                    break;
                case "update topic":
                    UpdateTopic(msg);
                    msg = null;
                    break;
                case "list topics":
                    msg = SendTopicsText(msg.author);
                    break;
                case "join topic":
                case "enter topic":
                case "join":
                    msg = EnterTopic(msg);
                    break;
                case "msg topic":
                case "send msg topic":
                    SendMessageInTopic(msg);
                    msg = null;
                    break;
                case "list msg topic":
                case "view topic":
                    msg = GetConversationOfTopic(msg.GetArgument(ArgType.NAME));
                    break;
                case "msg user":
                    SendMessageToUser(msg);
                    msg = null;
                    break;
                case "send file user":
                    SendFileToUser(msg);
                    msg = null;
                    break;
                case "send file topic":
                    SendFileInTopic(msg);
                    msg = null;
                    break;
                case "send audio user":
                    SendAudioMsgToUser(msg);
                    msg = null;
                    break;
                case "send audio topic":
                    SendAudioMsgToTopic(msg);
                    msg = null;
                    break;
                case "download topic":
                    SyncTopicForHisClient(comm, msg, msg.GetArgument(ArgType.NAME));
                    msg = null;
                    break;
                case "download topics":
                    SyncTopicsForHisClient(comm, msg);
                    msg = null;
                    break;
                case "delete msg":
                    DeleteMsg(msg.GetArgument(ArgType.MSG_ID));
                    msg = null;
                    break;
                case "edit msg":
                    if (msg.content is ChatMessage cMsg)
                        EditMsgAndSend(cMsg);
                    else
                        EditMsgAndSend(msg.GetArgument(ArgType.MSG_ID), msg.GetArgument(ArgType.MESSAGE));
                    msg = null;
                    break;
                case "block user":
                    break;
                case "help":
                    break;
                case "disconnect":
                case "logoff":
                case "stop":
                    Disconnect(msg);
                    msg = null;
                    break;
                default:
                    string err = "unknown command was entered : " + msg.fullCommand;
                    msg.fullCommand = err;
                    Console.WriteLine(err);
                    break;

            }
            if (msg != null)
            {
                Net.SendMsg(comm.GetStream(), msg);
            }

        }

        private void SendUsersToAllClients()
        {
            Message m = new Message(User.GetBotUser(),"sync user list | data");
            m.content = User.userList;
            foreach (var tcpUser in Data.userClients)
            {
                Net.SendMsg(tcpUser.tcpClient.GetStream(),m);
            }
            Console.WriteLine("Sent user list to all clients");

        }



        public void CreateTopic(string name, string description, List<User> invitedUsers, TcpClient creatorComm = null)
        {

            var topic = new Topic(name);
            topic.users.AddRange(invitedUsers);

            // If the description is an empty string, then we consider that there is no description for the topic
            if (!string.Empty.Equals(description))
                topic.Description = description;
            else
                topic.Description = null;

            Data.topicList.Add(topic);
            Console.WriteLine($"Topic '{name}' created successfully ");
            Console.WriteLine("> Sending the topic to each user : ");
            if (creatorComm != null)
                Net.SendMsg(creatorComm.GetStream(), new Message(User.GetBotUser(), $"Your topic '{name}' has been created successfully"));
            foreach (var u in Data.userClients)
            {
                Net.SendMsg(u.tcpClient.GetStream(), new Message(User.GetBotUser(), $"sync topic | data") { mustBeParsed = true, content = topic });
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
                Net.SendMsg(u.tcpClient.GetStream(), new Message(User.GetBotUser(), "rcv new topic | data"));
            }
            Console.WriteLine($"Topic '{name}' created successfully ");
        }

        // Must be used by the server
        public Message GetConversationOfTopic(string name)
        {
            Topic t = Data.topicList.Find(x => x.Name.Equals(name));
            StringBuilder conversation = new StringBuilder();
            conversation.AppendLine();
            if (t.chatMessages.Count > 0)
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
            return new Message(User.GetBotUser(), conversation.ToString());

        }

        /// <summary>
        /// Server only
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public void SyncTopicsForHisClient(TcpClient client, Message msg)
        {
            msg.fullCommand = "sync topics | data";
            msg.content = Data.topicList;
            msg.mustBeParsed = true;
            Net.SendMsg(client.GetStream(), msg);
        }

        public void SyncTopicForHisClient(TcpClient comm, Message msg, string topicName)
        {
            msg.fullCommand = $"sync topic | n:{topicName}";
            msg.content = Data.topicList.Find(x => x.Name.Equals(topicName));
            msg.mustBeParsed = true;
            Net.SendMsg(comm.GetStream(), msg);
        }


        // msg user | u:William m:coucou
        public void SendMessageToUser(Message msg)
        {
            var destUsername = msg.GetArgument(ArgType.USERNAME);
            var messageContent = msg.GetArgument(ArgType.MESSAGE);
            TcpClient destClient = Data.RetrieveClientFromUsername(destUsername);
            var users = User.GetAllUsers();
            var destUser = users.Find(u => u.username.Equals(destUsername));
            var msgId = GetNewMsgIdSafe();

            // Send to the user
            SendMessageToUser(destClient.GetStream(), msg.author, destUser, messageContent, msgId);

            // Send the message to self
            if (!destUsername.Equals(msg.author.username))
                SendMessageToUser(Data.RetrieveClientFromUsername(msg.author.username).GetStream(), msg.author, destUser, messageContent, msgId);
        }

        // 1 . The server receives the message from User 1
        // 2 (HERE) . The server redirects the messsage to User 2
        // To find User 2, we use its id
        private void SendMessageToUser(Stream destStream, User sender, User destUser, string msgContent, int mId)
        {
            var chatMsg = new ChatMessage(DateTime.Now, sender, destUser.username, msgContent) { id = mId };
            Net.SendMsg(destStream, new Message(destUser, $"rcv user msg | data") { content = chatMsg, mustBeParsed = true });
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
                    new Message(User.GetBotUser(), $"sync topic | n:{topicName} ") { mustBeParsed = true, content = topicToEdit });
            }
        }

        public void DeleteTopic(Message msg)
        {
            var topicName = msg.GetArgument(ArgType.NAME);
            Data.topicList.RemoveAll(x => x.Name.Equals(topicName));

            // Synchronize all the clients to remove the topic
            var resp = new Message(User.GetBotUser(), "sync topics | data");
            resp.content = Data.topicList;
            resp.mustBeParsed = true;
            foreach (var tcpUser in Data.userClients)
            {
                Net.SendMsg(tcpUser.tcpClient.GetStream(), resp);
            }
            Console.WriteLine("topic deleted, topics synced with clients");
        }



        public void DeleteMsg(string idString)
        {
            int msgIdToDel = int.Parse(idString);
            SearchAndRemoveMessage(msgIdToDel, Data.topicList);
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
                        topic.chatMessages.Add(new ChatMessage(msg.date, msg.author, msg.destName, newContent) { id = msgIdToEdit });
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
                        , new Message(User.GetBotUser(), $"edit msg | id:{msgIdToEdit} m:{newContent}") { mustBeParsed = true, content = replaceContent }); ;
                    Console.WriteLine($"Sent to {user.username}:  edit msg | id:{msgIdToEdit} m:{newContent}");
                }
            }
            else
            {
                foreach (var user in Data.userClients)
                {
                    Net.SendMsg(user.tcpClient.GetStream()
                        , new Message(User.GetBotUser(), $"edit msg | id:{msgIdToEdit}  m:{newContent}") { mustBeParsed = true, content = replaceContent });
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

            if (editedTopic != null)
                foreach (var user in editedTopic.users)
                {
                    Net.SendMsg(Data.RetrieveClientFromUsername(user.username).GetStream()
                        , new Message(User.GetBotUser(), $"rmv msg | id:{msgIdToDel}") { mustBeParsed = true });
                }
            else
            {
                foreach (var user in Data.userClients)
                {
                    Net.SendMsg(user.tcpClient.GetStream()
                        , new Message(User.GetBotUser(), $"rmv msg | id:{msgIdToDel}") { mustBeParsed = true });
                }
            }

        }

        public void SendMessageInTopic(Message msg)
        {
            var topicName = msg.GetArgument(ArgType.NAME);
            var textContent = msg.GetArgument(ArgType.MESSAGE);

            if (msg.content != null && msg.content is ImageChatMessage chatMsg)
                SendMessageInTopic(msg.author, topicName, textContent, chatMsg);
            else
                SendMessageInTopic(msg.author, topicName, textContent);
        }

        public void SendFileToUser(Message msg)
        {
            SendFileToUser(msg.author, msg.GetArgument(ArgType.USERNAME), msg.content as byte[], msg.GetArgument(ArgType.NAME));
        }

        public void SendFileToUser(User fileSender, string destUsername, byte[] data, string nameWithFormat)
        {
            TcpClient destClient = Data.RetrieveClientFromUsername(destUsername);
            Console.WriteLine("Data to send size : " + data.Length);
            Net.SendMsg(
                    destClient.GetStream(),
                    new Message(fileSender, $"rcv file user | n:{nameWithFormat}") { content = data, mustBeParsed = true });
            Console.WriteLine("File sent to the user " + destUsername + " with a byte array : " + nameWithFormat);
        }


        public void SendFileInTopic(Message msg)
        {
            SendFileInTopic(msg.author, msg.GetArgument(ArgType.NAME), msg.GetArgument(ArgType.FILENAME_WITH_FORMAT), msg.content as byte[]);
        }

        private void SendFileInTopic(User fileSender, string topicName, string fileName, byte[] fileData)
        {
            Console.WriteLine($"Data to send size to the topic {topicName} : " + fileData.Length);
            Topic t = Data.topicList.Find(x => x.Name.Equals(topicName));
            foreach (var user in t.users)
            {
                TcpClient destClient = Data.RetrieveClientFromUsername(user.username);
                if (destClient != null)
                {
                    Net.SendMsg(
                            destClient.GetStream(),
                            new Message(fileSender, $"rcv file user | n:{fileName}") { content = fileData, mustBeParsed = true });
                    Console.WriteLine("File sent to the user " + user.username + " with a byte array : " + fileName);
                }
            }
        }

        public void AddNewTopic(Message m, bool autoJoin = true)
        {

            List<User> invitedUsernames = new List<User>();
            Console.WriteLine($"'{m.author.username}' wants to create a topic");

            if (autoJoin)
                invitedUsernames.Add(m.author);

            var description = m.GetOptionalArgument(ArgType.DESCRIPTION);
            var topicName = m.GetArgument(ArgType.NAME);
            var creatorTcp = Data.RetrieveClientFromUsername(m.author.username);
            CreateTopic(topicName, description, invitedUsernames, creatorTcp);
        }

        public void Disconnect(Message m)
        {
            var usToDisconnect = m.author;
            usToDisconnect.isAuthentified = false;
            foreach (var userTcp in Data.userClients.ToList())
            {
                if (userTcp.user != null && userTcp.user.username.Equals(usToDisconnect.username))
                {
                    Data.userClients.Remove(userTcp);
                }
            }
            foreach (var user in User.userList)
            {
                if (user.username.Equals(usToDisconnect.username))
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
            var topic = Data.topicList.Find(x => x.Name.Equals(destTopicName));
            foreach (var user in topic.users)
            {
                SendAudioMsgToUser(msg.author, user.username, audioData);
            }
        }


        public void SendAudioMsgToUser(Message msg)
        {
            var destUsername = msg.GetArgument(ArgType.USERNAME);
            var audioData = msg.content as byte[];
            SendAudioMsgToUser(msg.author, destUsername, audioData);
        }

        private void SendAudioMsgToUser(User msgSender, string destUsername, byte[] audioData)
        {
            var client = Data.RetrieveClientFromUsername(destUsername);
            Message m = new Message(msgSender, $"rcv audio | data") { mustBeParsed = true };
            m.content = audioData;
            Net.SendMsg(client.GetStream(), m);
        }

        public Message SendTopicsText(User senderOfRequest)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//////////// List of the Topics ///////////////");
            if (Data.topicList == null || Data.topicList.Count > 0)
            {
                int i = 1;
                foreach (Topic t in Data.topicList)
                {
                    var line = $"{i}) {t.Name} ({t.chatMessages.Count} msgs)";
                    if (t.users.Find(x => x.username.Equals(senderOfRequest.username)) == null)
                    {
                        line += " [Not joined]";
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
            return new Message(User.GetBotUser(), sb.ToString());
        }

        private static object lockSendMsg = new object();
        private static object lockEnterTopic = new object();
        public Message EnterTopic(Message m)
        {
            var topicName = m.GetArgument(ArgType.NAME);
            return EnterTopic(m.author, topicName);
        }

        public Message EnterTopic(User u, string topicName)
        {
            foreach (Topic t in Data.topicList)
            {
                if (t.Name.Equals(topicName))
                    return EnterTopic(u, t);
            }
            throw new NullReferenceException("Topic not found");
        }

        /// <summary>
        /// Use in the server side to not have to transfer data
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Message EnterTopic(User u, Topic topic)
        {
            lock (lockEnterTopic)
            {
                if (topic.users.Find(x => x.username.Equals(u.username)) == null)
                {
                    topic.users.Add(u);
                    Console.WriteLine($"User {u.username} has entered the chat in the topic " + topic.Name);
                    return GetConversationOfTopic(topic.Name);
                }
                else
                {
                    return new Message(User.GetBotUser(), "You have already joined this topic") { mustBeParsed = false };
                }
            }
        }


        private int GetNewMsgIdSafe()
        {
            lock (lockSendMsg)
            {
                return msgGlobalId++;
            }
        }

        public void SendMessageInTopic(User msgAuthor, string destTopicName, string content, ChatMessage senderChatMsg = null)
        {
            foreach (Topic t in Data.topicList)
            {
                if (t.Name.Equals(destTopicName))
                {
                    lock (lockSendMsg)
                    {
                        if (senderChatMsg == null)
                            t.AddMessageAndSync(new ChatMessage(DateTime.Now, msgAuthor, destTopicName, content) { id = GetNewMsgIdSafe() });
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
