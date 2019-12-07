using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ChatCommunication
{
    [Serializable]
    public class Topic
    {
        private string name;
        
        /// <summary>
        /// A remplacer avec un hashset ?
        /// </summary>
        public List<User> users = new List<User>();
        public List<ChatMessage>  chatMessages = new List<ChatMessage>();

        public string Name { get => name; set => name = value; }

        public Topic(string name)
        {
            this.Name = name;
        }

        // run by the server
        internal void AddMessageAndSync(TcpClient client ,ChatMessage chatMessage)
        {
            chatMessages.Add(chatMessage);
            Console.WriteLine("Message sent in topic : " + Name);

            // Refresh the client messages
            foreach (var u in users)
            {
                var msg = new Message(User.GetBotUser(), $"refresh topic | n:{Name}") ;
                msg.mustBeParsed = true;
                msg.content = chatMessage;
                Net.SendMsg(Data.RetrieveClientFromUsername(u.username).GetStream(), msg); // Refresh all the clients that are in this topic
            }
        }
    }
}
