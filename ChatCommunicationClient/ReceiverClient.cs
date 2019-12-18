﻿using ChatCommunicationClient;
using ChatCommunicationClient.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace ChatCommunication
{

    public class ReceiverClient : CommunicatorBase, IClientChatActions
    {

        private static string IP_SERVER_ADDRESS = "127.0.0.1";
        private static int PORT = 8976;
        private const bool DEBUG_SHOW_RCV_COMMANDS = false;

        IAudioModule audioModule;
        User curUser = new User("guest", "guest");
        TcpClient comm;
        Thread commandThread;


        void ListenForClientCommands()
        {
            while (true)
            {
                keyboardCommand = Console.ReadLine();
                // We send data here
                if (keyboardCommand != string.Empty)
                {
                    if (confirmationForDownloadRequested)
                    {
                        confirmationForDownloadRequested = false;
                        if (keyboardCommand.ToLower().Trim().Equals("y"))
                            downloadAllowed = true;
                        else
                            downloadAllowed = false;

                        // From 1 to 0, we allow the other thread to be executed
                        fileSem.Release();
                    }

                    var msg = new Message(curUser, keyboardCommand);
                    if (keyboardCommand.StartsWith("send file"))
                    {
                        ConfigureFileToSend(msg);
                    }
                    else if (keyboardCommand.StartsWith("send audio"))
                    {
                        ConfigureAudioToSend(msg);
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



        // When the client receives an instruction from the server
        Semaphore fileSem = new Semaphore(0, 1);
        private bool confirmationForDownloadRequested;
        private bool downloadAllowed = false;

        /// <summary>
        /// Wait for keyboard input from the 'Keyboard thread' to continue, while displaying textual information
        /// </summary>
        /// <param name="sem">The semaphore that will block the current thread, must be initialized to 1 to directly block it</param>
        /// <param name="waitMsg">The message to display while we are waiting for keyboard input</param>
        /// <param name="invalidMsg">The message to display if the keyboard input denies the request</param>
        /// <returns></returns>
        bool IsKeyboardConfirmed(Semaphore sem, string waitMsg,string invalidMsg)
        {

            Console.WriteLine($"{waitMsg} [Y/N]");

            // We signal the other thread to listen to a YES / NO answer for the keyboard input
            confirmationForDownloadRequested = true;

            // Block this thread until the other thread unblock it
            sem.WaitOne(); // From 0 to 1

            if(!downloadAllowed)
                Console.WriteLine(invalidMsg);

            var tempVal = downloadAllowed;

            // We reset the auth to false
            downloadAllowed = false;
            return tempVal;
        }

        void DoOperationClientSide(Message msg)
        {
            try
            {
                msg.Parse();
                switch (msg.CommandPart)
                {
                    case "rcv file user":
                        HandleFileFromUser(msg);
                        break;
                    case "rcv audio":
                        ReceiveAudioMsg(msg);
                        break;
                    case "rcv user msg":
                        var chatMsgPrivate = msg.content as ChatMessage;
                        AddMsgUser(chatMsgPrivate);
                        break;
                    case "auth status":
                        DisplayAuthentificationResult(msg);
                        break;
                    case "refresh topic":
                        string topicName = msg.GetArgument(ArgType.NAME);
                        var content = msg.content as ChatMessage;
                        AddMsgTopic(topicName,content);
                        break;
                    case "sync topics":
                        DisplayTopics(msg);
                        break;
                    case "disconnect":
                        DisconnectSuccess();
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

        private void DisconnectSuccess()
        {
            curUser = new User("guest", "guest");
            Console.WriteLine("Disconnection is successful. Use connect | u:{username} p:{password} to log again");
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



        public void ConfigureFileToSend(Message msgToSend)
        {
            try
            {
                msgToSend.fullCommand = msgToSend.fullCommand.Replace(":\\", "<<doubledot>>");
                msgToSend.Parse();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid Format :" + ex); ;
            }
            var fileToUploadPath = msgToSend.GetArgument(ArgType.PATH);
            msgToSend.fullCommand = msgToSend.fullCommand.Replace("<<doubledot>>", ":\\");
            if (File.Exists(fileToUploadPath.Replace("<<doubledot>>", ":\\")))
            {
                msgToSend.content = File.ReadAllBytes(fileToUploadPath.Replace("<<doubledot>>", ":\\"));
            }
            else
            {
                Console.WriteLine($"(!) The you want to upload doesn't exist at {fileToUploadPath}");
            }
        }

        public void ConfigureAudioToSend(Message m)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                audioModule = new WindowsAudioModule();
                audioModule.Record();
                Console.WriteLine("Recording audio, press Enter to stop and send ...");
                Console.ReadLine();
                audioModule.StopRecording();
                var audioData = File.ReadAllBytes("c:\\temp\\toSend.wav");
                Console.WriteLine("Audio recorded, size : " + audioData.Length + " bytes");
                m.content = audioData;
            }
            else
            {
                Console.WriteLine("Platform  {" + RuntimeInformation.OSDescription + "}  is not supported");
            }
        }

        public void AddMsgUser(ChatMessage msg)
        {
            Console.WriteLine(Environment.NewLine + msg);
        }

        public void HandleFileFromUser(Message msg)
        {
            var downloadMsg = $"Do you wish to download the file '{msg.GetArgument(ArgType.NAME)}' " +
                     $"from the user '{msg.author.username}' ?";
            if (IsKeyboardConfirmed(fileSem, downloadMsg, "(X) You refused the download"))
            {
                DownloadFile(msg);
            }
        }



        public void ReceiveAudioMsg(Message m)
        {
            var downloadMsg = $"Do you wish to listen the audioClip  the user '{m.author.username}' ?";
            if (IsKeyboardConfirmed(fileSem, downloadMsg, "(X) You refused to listen to the audio clip"))
            {
                if (audioModule == null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        audioModule = new WindowsAudioModule();
                        audioModule.Play(m.content as byte[]);
                    }
                    else
                        Console.WriteLine("System {" + RuntimeInformation.OSDescription + "} is not supported yet for audio reading. It only works on windows");
                }
            }
        }


        public void AddMsgTopic(string topicName, ChatMessage msg)
        {
            Console.WriteLine($"[{topicName}]" + msg);
        }

        public void DisplayTopics(Message msg)
        {
            Data.topicList = msg.content as List<Topic>;
            Console.WriteLine("Sync of topics is a success!");
        }
        public void SyncUserList(User senderOfCommand)
        {
            Net.SendMsg(comm.GetStream(), new Message(senderOfCommand, "sync user list | data") { mustBeParsed = true });
            Console.WriteLine("Sent sync request (Users)");
        }

        public void DisplayAuthentificationResult(Message m)
        {
            string message = m.GetArgument(ArgType.MESSAGE);
            bool isSuccess = m.content as User != null;
            if (isSuccess)
            {
                curUser = m.content as User;
                curUser.isAuthentified = true;
                Console.WriteLine("Credentials are validated for this user : " + message);
                SyncUserList(curUser);
            }
            else
                Console.WriteLine("Credentials are wrong for this user :" + message);
        }
    }
}
