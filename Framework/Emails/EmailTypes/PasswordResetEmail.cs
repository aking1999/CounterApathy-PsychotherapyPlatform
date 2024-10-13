using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Emails.EmailTypes
{
    public class PasswordResetEmail
    {
        public string ToEmail { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string Token { get; set; }
    }
}
