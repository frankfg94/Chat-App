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

        #region class variables
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

        public string Description { get; set; }

        #endregion

        // Used for notifying the ui that a property changed and that it must be updated
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Topic t = (Topic)obj;
                return t.Name.Equals(Name);
            }
        }

        public Topic(string name)
        {
            this.Name = name;
        }

        // run by the server
        public void AddMessageAndSync(ChatMessage chatMessage)
        {
            chatMessages.Add(chatMessage);
            Console.WriteLine("Message sent in topic : " + Name);

            // Refresh the messages for each client
            foreach (var u in users)
            {
                if(u.isAuthentified)
                {
                    var msg = new Message(User.GetBotUser(), $"refresh topic | n:{Name}") ;
                    msg.mustBeParsed = true;
                    msg.content = chatMessage;
                    var client = Data.RetrieveClientFromUsername(u.username);    
                
                    if(client != null)
                        Net.SendMsg(client.GetStream(), msg); // Refresh all the clients that are in this topic
                }
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, users, chatMessages, infos, Name, addInfos, Description);
        }
    }
}
