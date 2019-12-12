using ChatCommunication;
using MaterialDesignThemes.Wpf;
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
    /// Logique d'interaction pour MessengerWindow.xaml
    /// </summary>
    public partial class MessengerWindow : Window
    {
        public IClientChatActions clientActions;
        User curUser = new User("guest", "guest");
        private readonly TcpClient serverComm;

        public MessengerWindow(TcpClient serverComm)
        {
            clientActions = new ReceiverClientGUI();

            // Loading the graphic components
            InitializeComponent();
            this.Loaded += MessengerWindow_Loaded;
            this.Closing += MessengerWindow_Closing;
            this.serverComm = serverComm;
        }

        private void MessengerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // AutoDisconnect when closing the window
            Net.SendMsg(serverComm.GetStream(),new Message(curUser,"disconnect | data") { mustBeParsed = true});
            curUser = null;
            var response = Net.RcvMsg(serverComm.GetStream());

            Console.WriteLine("Disconnect info : " + response.fullCommand);
        }

        private void MessengerWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
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
               if(displayHide)
                {
                    var dp = new DockPanel();
                    dp.Children.Add(new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.RemoveCircle });
                    dp.Children.Add(new TextBlock { Text=" Cacher ",Margin = new Thickness(5,0,0,0)});
                    l.Content = dp;
                }
               else
                {
                    var dp = new DockPanel();
                    if(isConv)
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
            TxtHideShow(true, usersLabel,false);
        }

        private void usersLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            TxtHideShow(false, usersLabel,false);
        }

        private void convLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            TxtHideShow(true, convLabel,true);
        }

        private void convLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            TxtHideShow(false, convLabel,true);
        }

    }
}
