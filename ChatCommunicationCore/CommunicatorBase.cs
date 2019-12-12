using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatCommunication
{
    public class CommunicatorBase
    {





        public bool VerifyCredentials(string login, string password, out string msg, out User u)
        {
            msg = "Incorrect username";
            foreach (var user in User.GetAllUsers())
            {
                if(user.username.Equals(login))
                {
                    if (user.password.Equals(password))
                    {
                        if(user.isAuthentified)
                        {
                            msg = $"User is already connected";
                            u = null;
                            return false;
                        }
                        msg = $"Connection is successful for '{login}'";
                        u = user;
                        return true;
                    }
                    else
                    {
                        msg = "Incorrect password";
                    }
                }
                 
            }
            u = null;
            return false;
        }



    }
}
