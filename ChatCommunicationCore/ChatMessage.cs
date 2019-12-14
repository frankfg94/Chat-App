using System;

namespace ChatCommunication
{
    [Serializable]
    public class ChatMessage
    {
        public DateTime date;
        public User author { get; set; }
        public string content { get; set; }

        public ChatMessage(DateTime date, User author, string content)
        {
            this.date = date;
            this.author = author;
            this.content = content;
        }

        public string ShortTimeString => date.ToShortTimeString();

        public override string ToString()
        {
          return  $"[{author.username}] {content} ({date})";
        }

    }
}