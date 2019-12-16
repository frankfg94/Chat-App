using System;

namespace ChatCommunication
{
    [Serializable]
    public class ChatMessage
    {
        public int id = -1;

        public DateTime date { get; set; }
        public User author { get; set; }
        public string destName { get; set; }
        public string content { get; set; }

        public ChatMessage(DateTime date, User author, string destName, string content) 
        {
            this.date = date;
            this.author = author;
            this.destName = destName;
            this.content = content;
        }

        public string ShortTimeString => date.ToShortTimeString();

        public override string ToString()
        {
          return  $"[{author.username}] {content} ({date})";
        }

    }
}