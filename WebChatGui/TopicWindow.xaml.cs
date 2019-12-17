using ChatCommunication;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebChatGuiClient
{
    /// <summary>
    /// Logique d'interaction pour CreationWindow.xaml
    /// </summary>
    public partial class TopicWindow : Window
    {
        private readonly TcpClient serverComm;

        public TopicWindow(TcpClient serverComm)
        {
            this.serverComm = serverComm;
            InitializeComponent();
            StartCreateMode();
        }

        private void StartCreateMode()
        {
            saveTopicButton.Click += (s, e) =>
            {
             
             Dispatcher.BeginInvoke( new Action(() =>{

                 // update the server by sending the topic creation signal
                 Net.SendMsg(serverComm.GetStream(),new Message(MessengerWindow.curUser,
                     $"create topic | n:{topicNameTbox.Text.Trim().Replace(' ', '-')} d:{topicDescriptionTbox.Text.Trim()}"));
                 Close();
             }));
            };
        }

        // We start the window in the edition mode
        public TopicWindow(TcpClient serverComm,Topic topic)
        {
            this.serverComm = serverComm;
            InitializeComponent();
            StartEditMode(topic);
        }

        
        void StartEditMode(Topic topic)
        {
            Button deleteBut  = new Button
                    {

                        // Setting up our 'delete topic' button
                        Content = "Delete this topic",
                        Style = this.Resources["MaterialDesignFlatAccentBgButton"] as Style,
                        Background = Brushes.Red,
                        BorderBrush = Brushes.Red,
                        VerticalAlignment = VerticalAlignment.Bottom
                    };
                    optionsPanel.Children.Insert(0,deleteBut);

            string oldName = topic.Name;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    topicNameTbox.Text = topic.Name;
                    topicDescriptionTbox.Text = topic.Description;
                    saveTopicButton.Content = "Edit this topic";
                }));

            deleteBut.Click += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() => {

                    // update the server by telling that we want to delete the topic
                    Net.SendMsg(serverComm.GetStream(), new Message(MessengerWindow.curUser, $"delete topic | n:{oldName}") { mustBeParsed = true});
                    Close();
                }));
            };

            // When we click the Edit button
            saveTopicButton.Click += (s, e) =>
            {

                Dispatcher.BeginInvoke(new Action(() =>{

                 // update the server by applying the changes on the topic
                 Net.SendMsg(serverComm.GetStream(), new Message(MessengerWindow.curUser,
                                 $"update topic | n:{oldName} nn:{topicNameTbox.Text.Trim().Replace(' ','-')} d:{topicDescriptionTbox.Text.Trim()}"));
                 Close();
             }));
            };
        }
    }
}
