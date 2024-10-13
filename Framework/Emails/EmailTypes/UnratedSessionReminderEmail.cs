using System;
using System.Collections.Generic;
using System.Text;
using Database.Models;

namespace Framework.Emails.EmailTypes
{
    public class UnratedSessionReminderEmail
    {
        public string ToEmail { get; set; }
        public List<BookedSessions> BookedSessionsToRemindAbout { get; set; }
        
        public UnratedSessionReminderEmail()
        {
            BookedSessionsToRemindAbout = new List<BookedSessions>();
        }
    }
}
