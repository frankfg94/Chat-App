using ChatCommunication;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebChatGuiClient
{
    /// <summary>
    /// Logique d'interaction pour MessengerWindow.xaml
    /// </summary>
    public partial class MessengerWindow : Window
    {
        public ReceiverClientGUI clientActions;
        public static User curUser = new GuiUser("guest", "guest");

        // The user we are currently speaking to
        User curChatter = null;

        // The topic we are currently messaging
        public Topic curTopic = null;
        private readonly TcpClient serverComm;
        public List<ChatMessage> privateMessages = new List<ChatMessage>();

        public MessengerWindow(TcpClient serverComm, User user)
        {

            // Loading the graphic components
            InitializeComponent();
            curUser = user;
            this.serverComm = serverComm;

            var clientActions = new ReceiverClientGUI(serverComm, this);
            new Thread(clientActions.ListenServerMsgs).Start();

            // Sync the user list
            clientActions.SyncUserList(curUser);

            // Get and display all the created topics
            clientActions.SyncTopicList(curUser);

            // Hide the join topic card
            Dispatcher.BeginInvoke(new Action(() => topicCard.Visibility = Visibility.Collapsed));

                this.Loaded += MessengerWindow_Loaded;
            this.Closing += MessengerWindow_Closing;
        }

        internal void SetUserList(List<User> list)
        {
            User.userList = list;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                usersListbox.Items.Clear();
                foreach (var us in list)
                {
                    if (!us.isAuthentified)
                    {
                        us.addInfos = "(Offline)";
                    }
                    usersListbox.Items.Add(us);
                }
            }));
        }

        // The storyboard animation for closing the app
        Storyboard sb;
        private bool appIsClosable;

        private void MessengerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // AutoDisconnect when closing the window
            Net.SendMsg(serverComm.GetStream(), new Message(curUser, "disconnect | data") { mustBeParsed = true });
            clientActions.SyncUserList(curUser);
            curUser = null;

            if (!appIsClosable)
            {
                sb = this.FindResource("DeZoomAnim") as Storyboard;
                Storyboard.SetTarget(sb, this);
                sb.Completed += Sb_Completed;
                sb.Begin();
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }

        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            appIsClosable = true;
            Dispatcher.BeginInvoke(new Action(() => Close()));
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }

        private void MessengerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the user tab
            this.Dispatcher.BeginInvoke(new Action(() => welcomeTblock.Text = "Welcome " + curUser.username));

            // Show the image of the user stored on the server
            this.Dispatcher.BeginInvoke(new Action(() => profileImg.Source = ByteToImage((curUser as GuiUser).ImgData)));


        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
            }
            ));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

            }
            ));
        }


        private void convLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (convListbox.Visibility == Visibility.Visible)
                    convListbox.Visibility = Visibility.Collapsed;
                else
                    convListbox.Visibility = Visibility.Visible;
            }
            ));
        }

        private void usersLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (usersListbox.Visibility == Visibility.Visible)
                    usersListbox.Visibility = Visibility.Collapsed;
                else
                    usersListbox.Visibility = Visibility.Visible;
            }
           ));
        }

        private void TxtHideShow(bool displayHide, Label l, bool isConv)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (displayHide)
                {
                    var dp = new DockPanel();
                    dp.Children.Add(new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle });
                    dp.Children.Add(new TextBlock { Text = " Cacher ", Margin = new Thickness(5, 0, 0, 0) });
                    l.Content = dp;
                }
                else
                {
                    var dp = new DockPanel();
                    if (isConv)
                    {
                        dp.Children.Add(new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.Chat });
                        dp.Children.Add(new TextBlock { Text = "Conversations", Margin = new Thickness(5, 0, 0, 0) });
                    }
                    else
                    {
                        dp.Children.Add(new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.User });
                        dp.Children.Add(new TextBlock { Text = "Users", Margin = new Thickness(5, 0, 0, 0) });
                    }
                    l.Content = dp;
                }
            }));

        }

        private void usersLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            TxtHideShow(true, usersLabel, false);
        }

        private void usersLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            TxtHideShow(false, usersLabel, false);
        }

        private void convLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            TxtHideShow(true, convLabel, true);
        }

        private void convLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            TxtHideShow(false, convLabel, true);
        }

        private void sendFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    // Configuring the command to send
                    var path = openFileDialog.FileName;
                    var tabDots = path.Split('.');
                    var extension = tabDots[tabDots.Length - 1]; // Ex: .jpg
                    var newName = "guiFile" + "." + extension;

                    // Configuring the command for whether it is for an user or a conversation
                    Message m;
                    string command = "";
                    if (curChatter != null)
                    {
                        command = $"send file user | p:{path} n:{newName} u:{curUser.username}";
                    }
                    else if (curTopic != null)
                    {
                        command = $"send file topic | p:{path} fn:{newName} n:{curTopic.Name}";
                    }
                    else
                    {
                        command = $"send file user | p:{path} n:{newName} u:{curUser.username}";
                    }
                    m = new Message(curUser, command) { mustBeParsed = true };
                    m.content = File.ReadAllBytes(path);
                    try
                    {
                        // Correcting the problem with the path
                        clientActions.ConfigureFileToSend(m);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                    // Sending the file to the server
                    Net.SendMsg(serverComm.GetStream(), m);
                }
            }));
        }

        private void SendMessageOnEnterPress(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if((sender as TextBox).Text.Trim() != string.Empty )
                    {
                        if(curTopic != null)
                        {
                            Net.SendMsg(serverComm.GetStream(),new Message(curUser,$"msg topic | n:{curTopic.Name} m:{(sender as TextBox).Text}"));
                           (sender as TextBox).Text = string.Empty;
                        }
                        else if (curChatter != null)
                        {
                            Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"msg user | u:{curChatter.username} m:{(sender as TextBox).Text}"));
                            messageListbox.Items.Add(new ChatMessage(DateTime.Now, curUser, (sender as TextBox).Text));
                            privateMessages.Add(new ChatMessage(DateTime.Now,curUser,(sender as TextBox).Text));
                            (sender as TextBox).Text = string.Empty;
                        }
                    }
                }));
            }
        }


        private void joinTopicBut_Click(object sender, RoutedEventArgs e)
        {
            if (curTopic != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Net.SendMsg(serverComm.GetStream(),new Message(curUser,$"join | n:{curTopic.Name}"));
                    Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"download topic | n:{curTopic.Name}"));
                    curTopic.users.Add(curUser);
                    topicCard.Visibility = Visibility.Collapsed;
                    chatPanel.IsEnabled = true;
                }));
            }
        }

        private void createNewTopicBut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void sendAudioButton_Click(object sender, RoutedEventArgs e)
        {

            var path = "c:\\temp\\toSend.wav";
            var tabDots = path.Split('.');
            var extension = tabDots[tabDots.Length - 1]; // Here .wav

            // Configuring the command for whether it is for an user or a conversation
            Message m = null;
            string command = "";
            if (curChatter != null)
            {
                command = $"send audio user | u:{curUser.username}";
            }
            else if (curTopic != null)
            {
                command = $"send audio topic |  n:{curTopic.Name}";
            }
            else
            {
                command = $"send audio user |  u:{curUser.username}";
            }
            try
            {
            m = new Message(curUser, command) { mustBeParsed = true };
            clientActions.ConfigureAudioToSend(m);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            // Sending the file to the server
            Net.SendMsg(serverComm.GetStream(), m);
        }


        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;

            // We check if a listboxitem is clicked
            if (item != null)
            {
                if (clientActions == null)
                {
                    clientActions = new ReceiverClientGUI(serverComm, this);
                }
                curTopic = item.Content as Topic;
                if (curTopic.users.Find(x => x.username.Equals(curUser.username)) == null)
                {
                    topicCard.Visibility = Visibility.Visible;
                    topicNameTblock.Text = curTopic.Name;
                    topicUsCountTblock.Text = curTopic.users.Count + " users are on this topic";
                    userTopicsItemsControl.Items.Clear();
                    foreach (var user in curTopic.users)
                    {
                        userTopicsItemsControl.Items.Add(user);
                    }
                    chatPanel.IsEnabled = false;
                }
                else
                {
                    topicCard.Visibility = Visibility.Collapsed;
                    chatPanel.IsEnabled = true;
                }
                clientActions.DisplayTopicChat(item.Content as Topic);
            }
        }


        private void usersListbox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var lboxItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (lboxItem != null)
            {
                if (clientActions == null)
                {
                    clientActions = new ReceiverClientGUI(serverComm, this);
                }
                curChatter = lboxItem.Content as User;
                clientActions.DisplayUserChat(curChatter);
            }
        }
    }
          
}
