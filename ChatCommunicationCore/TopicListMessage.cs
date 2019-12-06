using ChatCommunication;
using System;

[Serializable]
public class TopicListMessage : Message
{
    public readonly System.Collections.Generic.List<Topic> topicList;

    public TopicListMessage(User u,string command, System.Collections.Generic.List<Topic> topicList) : base( u ,  command)
	{
        this.topicList = topicList;
    }
}
