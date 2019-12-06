using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatCommunication
{
    public class CommunicatorBase
    {





        public bool VerifyCredentials(string login, string password, out string msg)
        {
            msg = "Incorrect username";
            foreach (var user in User.GetAllUsers())
            {
                if(user.username.Equals(login))
                {
                    if (user.password.Equals(password))
                    {
                        msg = $"Connection is successful for '{login}'";
                        return true;
                    }
                    else
                    {
                        msg = "Incorrect password";
                    }
                }
                 
            }
            return false;
        }

        public void Login(string login, string password)
        {
            if (VerifyCredentials(login, password, out string errMsg))
            {
                Console.WriteLine("Credentials are valids for user " + User.GetAllUsers().First(x=>x.username.Equals(login)));
            }
            else
            {
                Console.WriteLine(errMsg);
            }
        }


    }
}
