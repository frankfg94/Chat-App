using System;
using System.Collections.Generic;
using System.Text;

namespace ChatCommunication
{
    [Serializable]
    public class ImageChatMessage : ChatMessage
    {
        public byte[] imgData { get; set; }
        public ImageChatMessage(DateTime date, User author, string destName, string content, byte[] imgData) : base(date, author, destName, content)
        {
            this.imgData = imgData;
        }
    }
}
