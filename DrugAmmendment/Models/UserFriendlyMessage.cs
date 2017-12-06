using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugAmmendment.Models
{
    public class UserFriendlyMessage
    {
        private static string Message;
        public UserFriendlyMessage()
        {
            Message = null;
        }
        public static string getMessage()
        {
            return Message;
        }
        public static void setMessage(string msg)
        {
            Message = msg;
        }
    }
}