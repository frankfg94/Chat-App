using ChatCommunication;
using ChatCommunicationClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace WebChatGuiClient
{
    public class ReceiverClientGUI : IClientChatActions
    {
        public void ConfigureAudioToSend(Message m)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var audioModule = new WindowsAudioModule();
                audioModule.Record();
                Console.WriteLine("Recording audio, press Enter to stop and send ...");



                audioModule.StopRecording();
                var audioData = File.ReadAllBytes("c:\\temp\\toSend.wav");
                Console.WriteLine("Audio recorded, size : " + audioData.Length + " bytes");
                m.content = audioData;
            }
            else
            {
               MessageBox.Show("Platform  {" + RuntimeInformation.OSDescription + "}  is not supported","Unable to record audio",MessageBoxButton.OK,MessageBoxImage.Error);
            }
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

        public void DisplayAuthentificationResult(Message m)
        {
            throw new NotImplementedException();
        }

        public void DownloadTopics(Message m)
        {
            throw new NotImplementedException();
        }

        public void HandleFileFromUser(Message m)
        {
            throw new NotImplementedException();
        }

        public void HandleMsg(ChatMessage msg)
        {
            throw new NotImplementedException();
        }

        public void ReceiveAudioMsg(Message m)
        {
            throw new NotImplementedException();
        }

        public void SendFileToUser(string username)
        {
            throw new NotImplementedException();
        }
    }
}
