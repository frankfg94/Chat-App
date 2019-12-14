using ChatCommunication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChatCommunication
{
    [Serializable]
    public class GuiUser :  User, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

     

        public GuiUser(string login, string password) : base(login, password)
        {
        }

      


    }
}
