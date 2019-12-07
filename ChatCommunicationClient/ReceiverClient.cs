using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ChatCommunication
{
    public class ReceiverClient : CommunicatorBase
    {
        private static string IP_SERVER_ADDRESS = "127.0.0.1";
        private static int PORT = 8976;

        User curUser = new User("guest","guest");
        TcpClient comm;

        Thread commandThread;
        void ListenForClientCommands()
        {
            while(true)
            {
                    keyboardCommand = Console.ReadLine(); 
                    // We send data here
                    if (keyboardCommand != "")
                    {
                        var msg = new Message(curUser, keyboardCommand);
                        if(keyboardCommand.StartsWith("send file user"))
                        {
                            try
                            {
                                msg.fullCommand = msg.fullCommand.Replace(":\\","<<doubledot>>");
                                msg.Parse();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Invalid Format :" + ex); ;
                            }
                            var fileToUploadPath = msg.GetArgument(ArgType.PATH);
                            msg.fullCommand = msg.fullCommand.Replace("<<doubledot>>",":\\");
                            if (File.Exists(fileToUploadPath.Replace("<<doubledot>>", ":\\")))
                            {
                                msg.content = File.ReadAllBytes(fileToUploadPath.Replace("<<doubledot>>", ":\\"));
                            }
                            else
                            {
                             Console.WriteLine($"(!) The you want to upload doesn't exist at {fileToUploadPath}");
                            }
                        }
                        Net.SendMsg(comm.GetStream(), msg);
                        if(comm.GetStream().DataAvailable)
                        {
                            var responseMsg = Net.RcvMsg(comm.GetStream());
                            if (responseMsg != null)
                            {
                                if (!responseMsg.mustBeParsed)
                                    Console.WriteLine(Environment.NewLine + "Info = " + responseMsg.fullCommand);
                                else
                                {
                                    try
                                    {
                                        DoOperationClientSide(responseMsg);
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
        }

        string keyboardCommand = "";
        public void JoinServerConsole()
        {
            Console.WriteLine("[CLIENT] Creating TcpClient");
             comm = new TcpClient(IP_SERVER_ADDRESS, PORT);
            Console.WriteLine("[CLIENT] Connection OK");
            commandThread = new Thread(ListenForClientCommands);
            commandThread.Start();
            while (true)
            {
                

                if(keyboardCommand != "" || comm.GetStream().DataAvailable)
                {
                    var keyboardTemp = keyboardCommand;

                    /*
                     * Offline commands
                     */
                    if (keyboardTemp.StartsWith("@local"))
                    {
                        switch (keyboardTemp.Trim())
                        {
                            //case "@local prepare":
                            //    {
                            //        lock(obj)
                            //        {
                            //            Console.Write("Enter the path : ");
                            //            var pathSend = Console.ReadLine();
                            //            Console.Write("\nEnter the username : ");
                            //            var username = Console.ReadLine();


                            //            // Format : send file user | u:{username to send the file to} n:{filename with extension}
                            //            prepMsg = new Message(this.curUser, $"send file user |  u:{username}");
                            //            prepMsg.fullCommand = prepMsg.fullCommand.Replace(":\\", "<doubledot>\\");
                            //            try
                            //            {
                            //                prepMsg.Parse();
                            //            }
                            //            catch (InvalidCommandFormatException ex)
                            //            {
                            //                Console.WriteLine("Invalid command : " + ex.Message);
                            //            }
                            //            var path = prepMsg.GetArgument(ArgType.PATH);
                            //            if (File.Exists(path.Replace("<doubledot>\\", ":\\")))
                            //            {
                            //                prepMsg.content = File.ReadAllBytes(path.Replace("<doubledot>\\", ":\\"));
                            //                Console.WriteLine("Content set, size : " + (prepMsg.content as byte[]).Length);
                            //            }
                            //            else
                            //            {
                            //                Console.WriteLine($"(!) The file at the location '{path}' doesn't exists on this computer");
                            //                prepMsg.content = null;
                            //                prepMsg.fullCommand = "A client tried to upload a file but it doesn't exist";
                            //                prepMsg.mustBeParsed = false;
                            //            }
                            //        }
                            //    }
                            //    break;
                            case "@local clear":
                                Console.Clear();
                                break;
                            case "@local list topics":
                                if(Data.topicList.Count > 0)
                                {
                                    foreach (var topic in Data.topicList)
                                    {
                                        Console.WriteLine("\tSaved topic found : " + topic.Name + " / " + topic.chatMessages.Count + " msgs");
                                    }
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

                        // On reçoit quelque chose, comme par exemple un message
                        
                        {
                            var msg = Net.RcvMsg(comm.GetStream());
                            if (!msg.mustBeParsed)
                                Console.WriteLine(Environment.NewLine + "Info = " + msg.fullCommand);
                            else
                            {
                                try
                                {
                                    DoOperationClientSide(msg);
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

        }



        // When the client receives an instruction from the server

        void DoOperationClientSide(Message msg)
        {
            try
            {
                msg.Parse();
                switch (msg.CommandPart)
                {
                    case "rcv file user":
                        DownloadFile(msg);
                        break;
                    case "rcv user msg":
                        var chatMsgPrivate = msg.content as ChatMessage;
                        Console.WriteLine( Environment.NewLine + chatMsgPrivate);
                        break;
                    case "auth status":
                        string message = msg.GetArgument(ArgType.MESSAGE);
                        bool success = msg.content as User != null;
                        if (success)
                        {
                            curUser = msg.content as User;
                            curUser.isAuthentified = true;
                            Console.WriteLine("Credentials are validated for this user : " + message.Replace("_", " "));
                        }
                        else
                            Console.WriteLine("Credentials are wrong for this user :" + message.Replace("_"," "));
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

        private void DownloadFile(Message msg)
        {
            var preciousData = msg.content as byte[];
            var filenameWithExtension = msg.GetArgument(ArgType.NAME);
            DownloadFile(preciousData, filenameWithExtension);
        }

        private void DownloadFile(byte[] preciousData, string filenameWithExtension)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + filenameWithExtension;
            File.WriteAllBytes(path,preciousData);
            Console.WriteLine("File downloaded to : " + path);
        }
    }
}
