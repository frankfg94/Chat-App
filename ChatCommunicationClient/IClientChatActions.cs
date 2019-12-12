namespace ChatCommunication
{
    public interface IClientChatActions
    {
        void SendFileToUser(string username);
        void ConfigureFileToSend(Message m);
        void ConfigureAudioToSend(Message m);
        void HandleMsg(ChatMessage msg);
        void HandleFileFromUser(Message m);
        void ReceiveAudioMsg(Message m);
        void DisplayAuthentificationResult(Message m);
        void DownloadTopics(Message m);        
    }
}