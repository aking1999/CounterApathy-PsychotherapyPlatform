using Database.Models;
using Framework.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Emails.EmailTypes
{
    public class BookedSessionEmail
    {
        public BookedSessions BookedSession { get; set; }

        public BookedSessionEmail()
        {
            BookedSession = new BookedSessions();
        }
    }
}
