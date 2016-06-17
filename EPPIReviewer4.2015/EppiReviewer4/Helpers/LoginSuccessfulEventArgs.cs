using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EppiReviewer4
{
    public class LoginSuccessfulEventArgs : EventArgs
    {
        private LoginSuccessfulEventArgs() { }
        public LoginSuccessfulEventArgs(string username, string password)
        {
            Username = username;
            Password = password;
        }
        public LoginSuccessfulEventArgs(string username, string code, string status)
        {
            Username = username;
            Password = "";
            Status = status;
            Code = code;
        }
        public LoginSuccessfulEventArgs(string username, string password, string code, string status)
        {
            Username = username;
            Password = password;
            Status = status;
            Code = code;
        }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Status { get; private set; }
        public string Code { get; private set; }
    }
}