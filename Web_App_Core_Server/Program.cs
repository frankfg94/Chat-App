using ChatCommunication;
using System;
using System.Net;

namespace Web_App_Core_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server().Start(new IPAddress(new byte[] { 127, 0, 0, 1 }), 8976);
        }


    }
}
