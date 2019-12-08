using ChatCommunicationClient;
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
        private const bool DEBUG_SHOW_RCV_COMMANDS = false;

        WindowsModule audioModule;

        User curUser = new User("guest", "guest");
        TcpClient comm;

        Thread commandThread;


        void ListenForClientCommands()
        {
            while (true)
            {
                keyboardCommand = Console.ReadLine();
                // We send data here
                if (keyboardCommand != "")
                {
                    if (confirmationForDownloadRequested)
                    {
                        confirmationForDownloadRequested = false;
                        if (keyboardCommand.ToLower().Trim().Equals("y"))
                            downloadAllowed = true;
                        else
                            downloadAllowed = false;

                        // From 1 to 0, we allow the other thread to be executed
                        s.Release();
                    }

                    var msg = new Message(curUser, keyboardCommand);
                    if (keyboardCommand.StartsWith("send file"))
                    {
                        try
                        {
                            msg.fullCommand = msg.fullCommand.Replace(":\\", "<<doubledot>>");
                            msg.Parse();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Invalid Format :" + ex); ;
                        }
                        var fileToUploadPath = msg.GetArgument(ArgType.PATH);
                        msg.fullCommand = msg.fullCommand.Replace("<<doubledot>>", ":\\");
                        if (File.Exists(fileToUploadPath.Replace("<<doubledot>>", ":\\")))
                        {
                            msg.content = File.ReadAllBytes(fileToUploadPath.Replace("<<doubledot>>", ":\\"));
                        }
                        else
                        {
                            Console.WriteLine($"(!) The you want to upload doesn't exist at {fileToUploadPath}");
                        }
                    }
                    else if (keyboardCommand.StartsWith("send audio"))
                    {
                        audioModule = new WindowsModule();
                        audioModule.Record();
                        Console.WriteLine("Recording audio, press Enter to stop and send ...");
                        Console.ReadLine();
                        audioModule.StopRecording();
                        var audioData = File.ReadAllBytes("c:\\temp\\toSend.wav");
                        Console.WriteLine("Audio recorded, size : " + audioData.Length + " bytes");
                        msg.content = audioData;
                    }
                    Net.SendMsg(comm.GetStream(), msg);
                    if (comm.GetStream().DataAvailable)
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


                if (keyboardCommand != "" || comm.GetStream().DataAvailable)
                {
                    var keyboardTemp = keyboardCommand;

                    /*
                     * Offline commands
                     */
                    if (keyboardTemp.StartsWith("@local"))
                    {
                        switch (keyboardTemp.Trim())
                        {
                            case "@local clear":
                                Console.Clear();
                                break;
                            case "@local list topics":
                                if (Data.topicList.Count > 0)
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
        Semaphore s = new Semaphore(0, 1);
        string downloadMsg = null;
        private bool confirmationForDownloadRequested;
        private bool downloadAllowed = false;

        void DoOperationClientSide(Message msg)
        {

            try
            {
                msg.Parse();
                switch (msg.CommandPart)
                {
                    case "rcv file user":
                        downloadMsg =
                        $"Do you wish to download the file '{msg.GetArgument(ArgType.NAME)}' " +
                        $"from the user '{msg.user.username}' ?";
                        Console.WriteLine($"{downloadMsg} [Y/N]");
                        confirmationForDownloadRequested = true;

                        // BLOCK THE THREAD UNTIL SIGNAL IS RECEIVED
                        s.WaitOne(); // From 0 to 1

                        if (downloadAllowed)
                            DownloadFile(msg);
                        else
                            Console.WriteLine("(X) You refused the download");
                        downloadAllowed = false;
                        break;
                    case "rcv audio":
                        downloadMsg =
                        $"Do you wish to listen the audioClip  the user '{msg.user.username}' ?";
                        Console.WriteLine($"{downloadMsg} [Y/N]");
                        confirmationForDownloadRequested = true;

                        // BLOCK THE THREAD UNTIL SIGNAL IS RECEIVED
                        s.WaitOne(); // From 0 to 1

                        if (downloadAllowed)
                        {
                            if(audioModule == null)
                                audioModule = new WindowsModule();
                            audioModule.Play(msg.content as byte[]);
                        }
                        else
                            Console.WriteLine("(X) You refused to listen to the audio clip");
                        downloadAllowed = false;
                        break;
                    case "rcv user msg":
                        var chatMsgPrivate = msg.content as ChatMessage;
                        Console.WriteLine(Environment.NewLine + chatMsgPrivate);
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
                            Console.WriteLine("Credentials are wrong for this user :" + message.Replace("_", " "));
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

                if (DEBUG_SHOW_RCV_COMMANDS)
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
            File.WriteAllBytes(path, preciousData);
            Console.WriteLine("File downloaded to : " + path);
        }
    }
}
