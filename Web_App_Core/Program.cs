using ChatCommunication;
using ChatCommunicationClient;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Web_App_Core
{
    class Program
    {
        private static string IP_SERVER_ADDRESS = "127.0.0.1";
        private static int PORT = 8976;
        static void Main(string[] args)
        {
            new ReceiverClient().JoinServerConsole(IP_SERVER_ADDRESS,PORT);
        }
    }
}