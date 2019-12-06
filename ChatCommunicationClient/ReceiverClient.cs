using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ChatCommunication
{
    public class ReceiverClient : CommunicatorBase
    {
        private static string IP_SERVER_ADDRESS = "127.0.0.1";
        private static int PORT = 8976;

        User curUser = null;
        TcpClient comm;
        public void JoinServerConsole()
        {
            Console.WriteLine("[CLIENT] Creating TcpClient");
             comm = new TcpClient(IP_SERVER_ADDRESS, PORT);
            Console.WriteLine("[CLIENT] Connection OK");
            while (true)
            {
                string command = Console.ReadLine();

                /*
                 * Offline commands
                 */
                if(command.StartsWith("@local"))
                {
                    switch (command.Trim())
                    {
                        case "@local clear":
                            Console.Clear();
                            break;
                        case "@local list topics":
                            if(Data.topicList.Count > 0)
                            foreach (var topic in Data.topicList)
                            {
                                Console.WriteLine("\tSaved topic found : " + topic.Name + " / " + topic.chatMessages.Count + " msgs");
                            }
                            else
                                Console.WriteLine("No topics are locally saved");
                            break;
                        default:    
                            Console.WriteLine("Unrecognized local command");
                            break;
                    }
                }
                else
                {
                    if (curUser == null)
                    {
                        // Getting the user 'Marc'
                        curUser = User.GetAllUsers()[0];
                    }
                    Net.SendMsg(comm.GetStream(), new Message(curUser,command));
                    var response = Net.RcvMsg(comm.GetStream());

                    if(response is Message msg)
                    {
                        if(!msg.mustBeParsed)
                            Console.WriteLine(Environment.NewLine + "Info = " + msg.fullCommand);
                        else
                        {
                            try
                            {
                                 DoOperationClientSide(msg) ;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error parsing & processing in client : " + ex);
                            }
                        }
                    }
                }



            }

        }



        // When the client receives an instruction from the server

        void DoOperationClientSide(Message msg)
        {
            try
            {
                msg.Parse();
                switch (msg.CommandPart)
                {
                    case "auth status":
                        bool success = Convert.ToBoolean(msg.GetArgument(ArgType.RESULT));
                        string message = msg.GetArgument(ArgType.MESSAGE);
                        if (success)
                        {
                            curUser.isAuthentified = true;
                            Console.WriteLine("Credentials are validated for this user : " + message);
                        }
                        else
                            Console.WriteLine("Credentials are wrong for this user :" + message);
                        break;
                    case "refresh topic":
                        var content = msg.content as ChatMessage;
                        string topicName = msg.GetArgument(ArgType.NAME);
                        Console.WriteLine(content);
                        break;
                    case "sync topics":
                        Data.topicList = msg.content as List<Topic>;
                        Console.WriteLine("Sync of topics is a success!");
                        break;
                    default:
                        string err = "unknown command was entered : " + msg.fullCommand;
                        msg.fullCommand = err;
                        Console.WriteLine(err);
                        break;
                }
                Console.WriteLine(msg.fullCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid command format " + ex);
                Console.WriteLine(ex.StackTrace);
            }
        }

       

    }
}
