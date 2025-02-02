﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace ChatCommunication
{
    public class Net
    {
        public static void SendMsg(Stream s, Message msg)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(s, msg);
        }   

        public static Message RcvMsg(Stream s)
        {
            lock(s)
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (Message)bf.Deserialize(s);
            }
        }

    }
}
