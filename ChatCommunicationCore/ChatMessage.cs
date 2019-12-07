using System;

namespace ChatCommunication
{
    [Serializable]
    public class ChatMessage
    {
        public DateTime date;
        public User author;
        public string content;

        public ChatMessage(DateTime date, User author, string content)
        {
            this.date = date;
            this.author = author;
            this.content = content;
        }

        public override string ToString()
        {
          return  $"[{author.username}] {content} ({date})";
        }

    }
}