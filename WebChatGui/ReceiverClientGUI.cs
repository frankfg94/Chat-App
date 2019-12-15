using ChatCommunication;
using ChatCommunicationClient;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WebChatGuiClient
{
    public class ReceiverClientGUI : IClientChatActions
    {
        private const bool DEBUG_SHOW_RCV_COMMANDS  = true;
        private readonly TcpClient comm;
        private readonly MessengerWindow window;


        public ReceiverClientGUI(TcpClient comm,MessengerWindow window)
        {
            this.comm = comm;
            this.window = window;
        }
        
        public void ListenServerMsgs()
        {
            while(true)
            {
                if(comm.GetStream().DataAvailable)
                {
                    var incommingMsg = Net.RcvMsg(comm.GetStream());
                    DoOperationClientSide(incommingMsg);
                }
            }
        }

        void DoOperationClientSide(Message msg)
        {
            try
            {
                msg.Parse();
                switch (msg.CommandPart)
                {
                    case "sync user list":
                        window.SetUserList(msg.content as List<User>);
                        Console.WriteLine("User list is updated!");
                        break;
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
                        AddMsgTopic(topicName, content);
                        break;
                    case "sync topic":
                        UpdateOrCreateTopic(msg);
                        break;
                    case "sync topics":
                        DisplayTopics(msg);
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

        
        private void AddMsgTopic(string topicName, ChatMessage chatMessage)
        {
            var topic = Data.topicList.Find(x => x.Name.Equals(topicName));
            if (topic == null)
            {
                var t = new Topic(topicName);
                t.chatMessages.Add(chatMessage);
                AddAndDisplayTopic(t);
            }
            else
            {
                topic.chatMessages.Add(chatMessage);

                // Add the message live to the currentTopic
                if (topic.Name.Equals(window.curTopic.Name))
                {
                    window.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        window.messageListbox.Items.Add(chatMessage);
                    }));
                }
                UpdateTopicDisplay(topic);
            }
        }

        public void SyncTopicList(User senderOfCommand)
        {
            Net.SendMsg(comm.GetStream(), new Message(senderOfCommand, "download topics | data") { mustBeParsed = true });
            Console.WriteLine("Sent sync request (Topics)");
        }

        private void AddAndDisplayTopic(string topicName)
        {
           Topic t = new Topic(topicName);
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                window.convListbox.Items.Add(t);
            }));
        }

        private void AddAndDisplayTopic(Topic t)
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                window.convListbox.Items.Add(t);
            }));
        }
       
        // Listbox only
        private void UpdateTopicDisplay(Topic t)
        {

            // We update the topic display on the left side
            var count = t.chatMessages.Count;
            if (count > 0)
            {
                if(count > 1)
                     t.addInfos = "( " + t.chatMessages.Count + " msgs ) ";
                else
                    t.addInfos = "( 1 message ) ";
            }
            else
                t.addInfos = "( Empty ) ";

           
        
        }



        public void SyncUserList(User senderOfCommand)
        {
            Net.SendMsg(comm.GetStream(), new Message(senderOfCommand, "sync user list | data") { mustBeParsed = true });
            Console.WriteLine("Sent sync request (Users)");
        }

        public void ConfigureAudioToSend(Message m)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var audioModule = new WindowsAudioModule();
                audioModule.Record();
                MessageBox.Show("Recording audio, Click OK to stop and send ...", "Recording Audio",MessageBoxButton.OK,MessageBoxImage.Information);
                audioModule.StopRecording();
                var audioData = File.ReadAllBytes("c:\\temp\\toSend.wav");
                File.Delete("c:\\temp\\toSend.wav");
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
                MessageBox.Show($"(!) The you want to upload doesn't exist at {fileToUploadPath}");
            }
        }

        public void DisplayAuthentificationResult(Message m)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Download/Refresh a single topic and display it
        /// </summary>
        private void UpdateOrCreateTopic(Message msg)
        {
            // We want to rename the topic if necessary
            var topicName = msg.TryGetArgument(ArgType.NAME);

            var downloadedTopic = msg.content as Topic;
            var localTopic = Data.topicList.Find(x => x.Name.Equals(downloadedTopic.Name));
            if (localTopic != null)
            {
                Data.topicList.Remove(localTopic);
            }
            else if(topicName != null)
            {
                Data.topicList.Remove(Data.topicList.Find(x=>x.Name.Equals(topicName)));
            }
            Data.topicList.Add(downloadedTopic);

            // Graphical update
            DisplayConversationsInLbox(Data.topicList);
            DisplayTopicChat(downloadedTopic);

        }

        public void DisplayTopics(Message m)
        {
           var topics = Data.topicList=  m.content as List<Topic>;
            DisplayConversationsInLbox(topics);
        }

        private void DisplayConversationsInLbox(List<Topic> topics)
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                window.convListbox.Items.Clear();
                foreach (var t in topics)
                {
                    window.convListbox.Items.Add(t);
                }
            }));

        }

        public void HandleFileFromUser(Message m)
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                panel.Children.Add(new PackIcon { Margin=new Thickness(10) , Kind = PackIconKind.FileDownload , Foreground = Brushes.CadetBlue, Tag = m});
                panel.Children.Add(new TextBlock { FontSize=14, Text = $"File available to download from {m.author.username} |  {m.GetArgument(ArgType.NAME)}" });
                panel.MouseLeftButtonDown += DownloadTheFileInMsg;
                window.messageListbox.Items.Add(panel);
            }));
        }

        private void DownloadTheFileInMsg(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ClickCount.Equals(2))
            {
                var message = (sender as StackPanel).Children.OfType<PackIcon>().First().Tag as Message;
                var data = message.content as byte[];
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = message.GetArgument(ArgType.NAME);
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName,data);
                    MessageBox.Show("File downloaded to " + saveFileDialog.FileName);
                }
            }
        }

        public void AddMsgUser(ChatMessage msg)
        {
            // We add the message to the user's inbox
            window.privateMessages.Add(msg);
            User downloadedUser = msg.author;
            var sameUser = User.userList.Find(x => x.username.Equals(downloadedUser.username));

            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                // We update the display of the user in the users listbox
                
                // Real time sync
                if(window.curChatter != null && window.curChatter.username.Equals(msg.author.username))
                {
                    window.messageListbox.Items.Add(msg);
                }

            }));



            // We find the number of messages for this user
            if (downloadedUser!= null )
            {
                int msgCountUser = 0;
                  foreach (var m in window.privateMessages)
                  {
                      if(m.author.username.Equals(downloadedUser.username))
                      {
                          msgCountUser++;
                      }
                  }

                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                foreach (User user in window.usersListbox.Items)
                {
                    // We change the display for the listbox item
                    if(user.username.Equals(downloadedUser.username))
                        downloadedUser.addInfos = msgCountUser + " msgs" ;
                }

                    window.usersListbox.Items.Refresh();
                }));
            }

        }

        Semaphore audioSem = new Semaphore(0, 1);
        public void ReceiveAudioMsg(Message m)
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                panel.Children.Add(new PackIcon { Margin = new Thickness(10), Kind = PackIconKind.Audio, Foreground = Brushes.CadetBlue, Tag = m }); ;
                panel.Children.Add(new TextBlock { FontSize = 14, Margin = new Thickness(5,0,0,0), Text = $"Audio File ready to listen from {m.author.username}" });
                          
                panel.MouseLeftButtonDown +=

                // When we click directly on the audio tile, it will play the sound
                (s, e) =>
                {
                    if(e.ClickCount ==2)
                    {
                        var oldBg = panel.Background;
                        panel.Background = Brushes.CadetBlue;
                        new Thread(() =>
                        {
                            using (MemoryStream ms = new MemoryStream(m.content as byte[]))
                            {
                                SoundPlayer sp = new SoundPlayer(ms);
                                sp.Play();
                            }
                        }).Start();
                        panel.Background = oldBg;
                    }

                };

                window.messageListbox.Items.Add(panel);
            }));
        }



        public void SendFile(string username)
        {
            throw new NotImplementedException();
        }

        internal void DisplayTopicChat(Topic topic)
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                // We only clear the screen if we already are in this topic
                if(window.curTopic == null || window.curTopic.Name.Equals(topic.Name))
                {
                    window.headerConversationNameTblock.Text = topic.Name;
                    if (topic.Description != null)
                        window.headerConversationNameTblock.Text += " / " + topic.Description;

                    window.messageListbox.Items.Clear();
                    window.editConvButton.IsEnabled = true;

                    foreach (var msg in topic.chatMessages)
                    {
                        window.messageListbox.Items.Add(msg);
                    }
                }
            }));
        }

        // 
       
        internal void DisplayUserChat(User curChatter)
        {
            window.curTopic = null;
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                window.editConvButton.IsEnabled = false;
                // We only clear the screen if we already are in this topic
                window.headerConversationNameTblock.Text = curChatter.username;
                    window.messageListbox.Items.Clear();

                    foreach (var msg in window.privateMessages)
                    {
                    // If the message is sent to us by the curChatter     US <-- curchatter
                        if (msg.destName.Equals(MessengerWindow.curUser.username) && msg.author.username.Equals(curChatter.username)
                    // Or if the message is sent by us to the currrent Chatter  US --> curchatter
                    || msg.author.username.Equals(MessengerWindow.curUser.username) && msg.destName.Equals(curChatter.username))
                        // Then we display it
                            window.messageListbox.Items.Add(msg);
                    }
                window.topicCard.Visibility = Visibility.Collapsed;
                window.chatPanel.IsEnabled = true;
                window.curChatterImg.Source = MessengerWindow.ByteToImage(curChatter.ImgData);
            }));
        }
    }
}
