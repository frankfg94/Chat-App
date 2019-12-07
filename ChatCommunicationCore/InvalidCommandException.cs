using System;
using System.Runtime.Serialization;

namespace ChatCommunication
{
    [Serializable]
    public class InvalidCommandFormatException : Exception
    {
        public InvalidCommandFormatException()
        {
        }

        public InvalidCommandFormatException(string message) : base(message)
        {
            
        }

        public InvalidCommandFormatException(string command, string reason) : base(command)
        {
        }

        public InvalidCommandFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidCommandFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}