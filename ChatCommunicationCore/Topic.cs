using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChatCommunication
{
    [Serializable]
    public class Topic : INotifyPropertyChanged
    {
        private string name;
        
        public List<User> users = new List<User>();
        public List<ChatMessage>  chatMessages = new List<ChatMessage>();
        private string infos = string.Empty;


        public string Name { get => name; set => name = value; }
        public string addInfos
        {
            get { return infos; }
            set
            {
                infos = value;
                // We indicate that we want to sync the UI
                NotifyPropertyChanged();
            }
        }

        public Topic(string name)
        {
            this.Name = name;
        }

        // run by the server
        internal void AddMessageAndSync(ChatMessage chatMessage)
        {
            chatMessages.Add(chatMessage);
            Console.WriteLine("Message sent in topic : " + Name);

            // Refresh the messages for each client
            foreach (var u in this.users)
            {
                var msg = new Message(User.GetBotUser(), $"refresh topic | n:{Name}") ;
                msg.mustBeParsed = true;
                msg.content = chatMessage;
                Net.SendMsg(Data.RetrieveClientFromUsername(u.username).GetStream(), msg); // Refresh all the clients that are in this topic
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
