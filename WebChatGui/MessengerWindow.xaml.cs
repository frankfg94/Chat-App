using ChatCommunication;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace WebChatGuiClient
{
    /// <summary>
    /// Logique d'interaction pour MessengerWindow.xaml
    /// </summary>
    public partial class MessengerWindow : Window
    {
        public ReceiverClientGUI clientGuiReceiver;

        // The user that is used for sending messages
        public static User curUser = new User("guest", "guest");

        // The user we are currently speaking to
        public User curChatter = null;

        // The topic we are currently messaging
        public Topic curTopic = null;

        // The TcpClient of the server
        private readonly TcpClient serverComm;

        // The local list of the privateMessages (sent and received) for this user
        public List<ChatMessage> privateMessages = new List<ChatMessage>();

        // The selected chat message that is being edited
        private ChatMessage chatMsgToEdit = null;

        public MessengerWindow(TcpClient serverComm, User user)
        {

            var clientActions = new ReceiverClientGUI(serverComm, this);
            // Loading the graphic components
            InitializeComponent();
            curUser = user;
            this.serverComm = serverComm;

            new Thread(clientActions.ListenServerMsgs).Start();


            // Sync the user list (download all the existing users and display them)
            clientActions.SyncUserList(curUser);

            // Clear the listbox for the sample topics
            Dispatcher.BeginInvoke(new Action(() =>  convListbox.Items.Clear()));

            // Get and display all the created topics
            clientActions.SyncTopicList(curUser);

            // Hide the join topic card
            Dispatcher.BeginInvoke(new Action(() => topicCard.Visibility = Visibility.Collapsed));

            this.Loaded += MessengerWindow_Loaded;
            this.Closing += MessengerWindow_Closing;
        }

        
        internal void DisplayUsersStatus(List<User> list)
        {
            User.userList = list;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                usersListbox.Items.Clear();
                // We indicate in the user panel if the users are offline or not
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
            if (!appIsClosable)
            {
                // AutoDisconnect when closing the window
                Net.SendMsg(serverComm.GetStream(), new Message(curUser, "disconnect | data") { mustBeParsed = true });
                if (clientGuiReceiver == null)
                {
                    clientGuiReceiver = new ReceiverClientGUI(serverComm, this);
                }
                clientGuiReceiver.SyncUserList(curUser);
                curUser = null;
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
            this.Dispatcher.BeginInvoke(new Action(() => profileImg.Source = ByteToImage(curUser.ImgData)));


        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
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
                    dp.Children.Add(new PackIcon { Kind = PackIconKind.RemoveCircle });
                    dp.Children.Add(new TextBlock { Text = " Cacher ", Margin = new Thickness(5, 0, 0, 0) });
                    l.Content = dp;
                }
                else
                {
                    var dp = new DockPanel();
                    if (isConv)
                    {
                        dp.Children.Add(new PackIcon { Kind = PackIconKind.Chat });
                        dp.Children.Add(new TextBlock { Text = "Conversations", Margin = new Thickness(5, 0, 0, 0) });
                    }
                    else
                    {
                        dp.Children.Add(new PackIcon { Kind = PackIconKind.User });
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
                    string command;
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
                        clientGuiReceiver.ConfigureFileToSend(m);
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

        // When you press the Enter key in the textbar
        private void SendMessageOnEnterPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if ((sender as TextBox).Text.Trim() != string.Empty)
                    {
                        if (chatMsgToEdit != null)
                        {
                            Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"edit msg | id:{chatMsgToEdit.id} m:{(sender as TextBox).Text.Trim()}") { mustBeParsed = true });
                            chatMsgToEdit = null;
                        }
                        else if (curTopic != null)
                        {
                            Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"msg topic | n:{curTopic.Name} m:{(sender as TextBox).Text}"));
                        }
                        else if (curChatter != null)
                        {
                            clientGuiReceiver.pauseLoop = true;
                            Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"msg user | u:{curChatter.username} m:{(sender as TextBox).Text}"));
                        }
                        (sender as TextBox).Text = string.Empty;



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
                    Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"join | n:{curTopic.Name}"));
                    Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"download topic | n:{curTopic.Name}"));
                    curTopic.users.Add(curUser);
                    topicCard.Visibility = Visibility.Collapsed;
                    chatPanel.IsEnabled = true;
                    msgTbox.Focus();
                }));
            }
        }



        private void sendAudioButton_Click(object sender, RoutedEventArgs e)
        {

            var path = "c:\\temp\\toSend.wav";
            var tabDots = path.Split('.');

            // Configuring the command for whether it is for an user or a conversation
            Message m = null;
            string command;
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
                clientGuiReceiver.ConfigureAudioToSend(m);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            // Sending the file to the server
            Net.SendMsg(serverComm.GetStream(), m);
        }

        // When we click on an item of the user's listbox
        private void UserListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;

            // We check if a listboxitem is clicked
            if (item != null)
            {
                if (clientGuiReceiver == null)
                {
                    clientGuiReceiver = new ReceiverClientGUI(serverComm, this);
                }
                curChatter = null;
                curTopic = item.Content as Topic;
                if (curTopic.users.Find(x => x.username.Equals(curUser.username)) == null)
                {
                    // Showing the card and the users
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
                    msgTbox.Focus();
                }
                clientGuiReceiver.DisplayTopicChat(item.Content as Topic);
            }
        }


        private void usersListbox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var lboxItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (lboxItem != null)
            {
                if (clientGuiReceiver == null)
                {
                    clientGuiReceiver = new ReceiverClientGUI(serverComm, this);
                }
                curChatter = lboxItem.Content as User;
                clientGuiReceiver.DisplayUserChat(curChatter);
            }
        }

        private void createNewTopicBut_Click(object sender, RoutedEventArgs e)
        {
            new TopicWindow(serverComm).Show();
        }


        private void editConvButton_Click(object sender, RoutedEventArgs e)
        {
            if (curTopic != null)
                new TopicWindow(serverComm, curTopic).ShowDialog();
        }

        private void deleteMsg_Click(object sender, RoutedEventArgs e)
        {

            if (messageListbox.SelectedItem != null)
            {
                var chatMsg = messageListbox.SelectedItem as ChatMessage;
                Net.SendMsg(serverComm.GetStream(), new Message(curUser, $"delete msg | id:{chatMsg.id}") { mustBeParsed = true });
            }
        }

        /// <summary>
        ///  When the edit option is selected, we start a GUI configuration to edit a chat message
        /// </summary>
        private void editMsg_Click(object sender, RoutedEventArgs e)
        {
            if (messageListbox.SelectedItem != null)
            {
                var chatMsg = messageListbox.SelectedItem as ChatMessage;
                if (chatMsg is ImageChatMessage imgMsg)
                {
                    clientGuiReceiver.EditWithWindowsExplorer(imgMsg);
                }
                else
                {
                    chatMsgToEdit = chatMsg;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        msgTbox.Text = chatMsg.content;
                    }));
                }
            }
        }

        private void sendImgButton_Click(object sender, RoutedEventArgs e)
        {
            Message m = null;
            string command = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "Choose the image to send";
            openFileDialog.Filter = "Image Format (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    // Configuring the command to send
                    var path = openFileDialog.FileName;
                    var tabDots = path.Split('.');
                    var extension = tabDots[tabDots.Length - 1]; // Ex: .jpg
                    var filename = path.Split("\\").Last();
                    ImageChatMessage chMsg = null;
                    if (curChatter != null || (curTopic == null && curChatter == null))
                    {
                        command = $"msg user | p:na u:{curUser.username} m:{filename}";
                        chMsg = new ImageChatMessage(DateTime.Now, curUser, curUser.username, filename, File.ReadAllBytes(path));
                    }
                    else if (curTopic != null)
                    {
                        command = $"msg topic | p:na  n:{curTopic.Name} m:{filename}";
                        chMsg = new ImageChatMessage(DateTime.Now, curUser, curTopic.Name, filename, File.ReadAllBytes(path));
                    }
                    try
                    {
                        m = new Message(curUser, command) { mustBeParsed = true };
                        m.content = chMsg;
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

        // This code is used to allow / disallow users to edit or remove a message with a right right click
        private void contextMenuOpen_messageListbox(object sender, ContextMenuEventArgs e)
        {
            // Getting the context menu that you obtain with a right click on the listbox
            var cMenu = (ContextMenu)messageListbox.FindName("cMenu");

            // Getting the items of the selected chat message (the names are defined in the xaml view)
            MenuItem deleteItem = LogicalTreeHelper.FindLogicalNode(cMenu, "deleteMsg") as MenuItem;
            MenuItem editItem = LogicalTreeHelper.FindLogicalNode(cMenu, "editMsg") as MenuItem;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (messageListbox.SelectedItem is ChatMessage chatMsg)
                {
                    // Only the author of a message can delete it
                    if (curUser.username.Equals(chatMsg.author.username))
                    {
                        deleteItem.IsEnabled = true;
                        editItem.IsEnabled = true;
                    }
                    else
                    {
                        deleteItem.IsEnabled = false;
                        editItem.IsEnabled = false;
                    }

                }
                else
                {
                    // If the listbox selected item is of unknown nature, we prefer not to allow editing nor deleting it to avoid errors
                    deleteItem.IsEnabled = false;
                    editItem.IsEnabled = false;
                }
            }));

        }
    }

}
