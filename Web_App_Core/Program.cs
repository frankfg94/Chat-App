using ChatCommunication;
using ChatCommunicationClient;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Web_App_Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Done");
            new ReceiverClient().JoinServerConsole();
        }
    }
}