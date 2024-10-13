using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Models
{
    public class GeneralException : Exception
    {
        public bool SignOutUser { get; set; }

        public GeneralException() { }

        public GeneralException(string message, bool signOutUser = false)
            : base(message)
        {
            SignOutUser = signOutUser;
        }

        public GeneralException(string message, Exception inner, bool signOutUser)
            : base(message, inner)
        {
            SignOutUser = signOutUser;
        }
    }
}
