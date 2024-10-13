using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Emails.EmailTypes
{
    public class WebCreditAddedEmail
    {
        public string ToEmail { get; set; }
        public string FirstName { get; set; }
        public string Amount { get; set; }
        public string CurrentAmount { get; set; }
    }
}
