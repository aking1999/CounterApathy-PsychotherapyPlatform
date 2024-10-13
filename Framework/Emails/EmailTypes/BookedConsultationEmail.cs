using Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Emails.EmailTypes
{
    public class BookedConsultationEmail
    {
        public BookedConsultations BookedConsultation { get; set; }

        public BookedConsultationEmail()
        {
            BookedConsultation = new BookedConsultations();
        }
    }
}
