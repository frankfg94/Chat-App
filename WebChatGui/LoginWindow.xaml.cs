using ChatCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebChatGuiClient;

namespace WebChatGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static string IP_SERVER_ADDRESS = "127.0.0.1";
        private static int PORT = 8976;
        TcpClient comm;
        User curUser = new User("guest", "guest");
        bool serverIresponsive = false;

        public LoginWindow()
        {
            InitializeComponent();
            connectButton.Click += ConnectButton_Click;
            this.Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                comm = new TcpClient(IP_SERVER_ADDRESS, PORT);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if(MessageBox.Show(ex.Message + Environment.NewLine + " Do you wish to try again ?","Error of connection",MessageBoxButton.YesNo,MessageBoxImage.Warning) ==  MessageBoxResult.Yes)
                {
                    LoginWindow_Loaded(sender, e);
                }
                else
                {
                    // We stop the program
                    Application.Current.Shutdown();
                }
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            Net.SendMsg(comm.GetStream(),new Message(curUser,$"connect | u:{usernameTB.Text.Trim()} p:{userPassword.Password}"));
            var msg = Net.RcvMsg(comm.GetStream());
            msg.Parse();

            string message = msg.GetArgument(ArgType.MESSAGE);
            bool isSuccess = msg.content as User != null;
            
            var displayMsg = "Credentials are validated for this user : " + message.Replace("_", " ");
            var msgColor = Brushes.Green;
            if (isSuccess)
            {
                curUser = msg.content as User;
                curUser.isAuthentified = true;
                    System.Timers.Timer t = new System.Timers.Timer(3000);
                    t.Elapsed += T_Elapsed;
                    t.AutoReset = false;
                    t.Start();
            }
            else
            {
                displayMsg = "Credentials are wrong for this user :" + message.Replace("_", " ");
                msgColor = Brushes.Red;
            }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    infoTBlock.Visibility = Visibility.Visible;
                    infoTBlock.Text = displayMsg;
                    infoTBlock.Foreground = msgColor;
                }));


            }
            catch (InvalidCommandFormatException ex)
            {
                Console.WriteLine("Invalid Command format exception  : " + ex.Message);
            }
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                new MessengerWindow(comm).Show();
                Close();
            }));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

    }
}
