using System;
using System.Net.Sockets;
using System.Threading;

namespace ChatCommunication
{
    public class ReceiverServer : CommunicatorBase
    {
        private TcpClient comm;

        public ReceiverServer(TcpClient comm)
        {
            this.comm = comm;
        }

        bool run = true;
        public void doOperation()
        {
            while (run)
            {
                try
                {
                    if (!comm.Connected)
                    {
                        Console.WriteLine("Client disconnected from the server : " + comm.Client.RemoteEndPoint);
                        comm.Dispose();
                        run = false;
                    }
                    else
                    {
                        Message data = Net.RcvMsg(comm.GetStream());
                        if (data is Message msg)
                        {
                                msg.Parse();
                                Console.WriteLine("Signal received : " + msg.fullCommand);
                                if(msg.user.isAuthentified)
                                {
                                    doOperationsAsUser(msg,msg.CommandPart,comm);
                                }
                                else
                                {
                                    TryConnectAsUser(msg);
                                }
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (comm.Connected)
                    {
                        Console.WriteLine("Invalid command format " + ex);
                        Console.WriteLine(ex.StackTrace);
                        Net.SendMsg(comm.GetStream(), new Message(User.GetBotUser(), "Invalid command format " + ex.Message));
                        Console.WriteLine("--> Sent an invalid command format message to the client");
                    }
                }
            }
        }

        // Sending a response to the client whether the connection is sucessful or not
        private void TryConnectAsUser(Message msg)
        {
            // Verify that we just want to connect ourselves first
            if(!msg.fullCommand.StartsWith("connect"))
            {
                var firstMes = new Message(User.GetBotUser(),"You have to connect yourself first before using any other command. Use : connect | u:{username} p:{password}");
                firstMes.mustBeParsed = false;
                Net.SendMsg(comm.GetStream(), firstMes);
                return;
            }

            if (VerifyCredentials(msg.GetArgument(ArgType.USERNAME), msg.GetArgument(ArgType.PASSWORD), out string textMsg, out User connectedUser))
            {
                connectedUser.isAuthentified = true;

                // Updating the user
                Data.userClients.Find(x => x.tcpClient.Client.Equals(comm.Client)).user = connectedUser;
               
                Console.WriteLine("Connection authorized! sending the connected user to the client");
            }
            else
            {
                Console.WriteLine("A client failed to connect itself to the server, Reason : " + textMsg);
            }
            var respToTheClient = new Message(User.GetBotUser(), $"auth status | r:{msg.user.isAuthentified} m:{textMsg.Replace(' ', '_')}");
            respToTheClient.mustBeParsed = true;
            respToTheClient.content = connectedUser;
            Net.SendMsg(comm.GetStream(), respToTheClient);
            
        }

        public void doOperationsAsUser(Message msg,string commandLine, TcpClient comm)
    {
        switch (msg.CommandPart)
        {
            case "create topic":
                msg = msg.user.AddNewTopic(msg);
                break;
            case "list topics":
                msg = msg.user.ListTopics();
                break;
            case "join topic":
            case "enter topic":
            case "join":
                msg = msg.user.EnterTopic(msg);
                break;
            case "msg topic":
                msg.user.SendMessageInTopic(comm, msg);
                msg = null;
                break;
            case "view topic":
                msg = msg.user.GetConversationOfTopic(msg.GetArgument(ArgType.NAME));
                break;
            case "msg user":
                msg.user.SendMessageToUser(msg);
                msg = null;
                break;
            case "send file user":
               msg.user.SendFileToUser(msg);
               msg = null;
               break;
            case "download topics":
                msg.user.SyncTopicsForHisClient(comm, msg);
                msg = null;
                break;
            case "help":
                break;
            default:
                string err = "unknown command was entered : " + msg.fullCommand;
                msg.fullCommand = err;
                Console.WriteLine(err);
                break;

        }
            if (msg != null)
            {
                Net.SendMsg(comm.GetStream(), msg);
            }

        }
    }

}
