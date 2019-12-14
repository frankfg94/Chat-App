namespace ChatCommunication
{
    public interface IClientChatActions
    {
        void ConfigureFileToSend(Message m);
        void ConfigureAudioToSend(Message m);
        void AddMsgUser(ChatMessage msg);
        void HandleFileFromUser(Message m);
        void ReceiveAudioMsg(Message m);
        void DisplayAuthentificationResult(Message m);
        void DisplayTopics(Message m);        
    }
}