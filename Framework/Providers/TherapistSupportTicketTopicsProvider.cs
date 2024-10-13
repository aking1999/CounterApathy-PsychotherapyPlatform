using Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Providers
{
    public class TherapistSupportTicketTopicsProvider
    {
        public static TherapistSupportTicketTopics ReportError => new TherapistSupportTicketTopics { Id = "report-error", Name = "Prijavite grešku / bag", Color = "#000", Icon = "far fa-bug" };
        public static TherapistSupportTicketTopics AskQuestion => new TherapistSupportTicketTopics { Id = "ask-question", Name = "Postavite pitanje", Color = "#27bcfd", Icon = "far fa-question" };
        public static TherapistSupportTicketTopics GiveFeedback => new TherapistSupportTicketTopics { Id = "feedback", Name = "Dajte povratnu informaciju", Color = "#34a4ff", Icon = "far fa-comment-alt-lines" };
        public static TherapistSupportTicketTopics RequestAccountDeletion => new TherapistSupportTicketTopics { Id = "account-deletion-request", Name = "Zahtev za gašenje naloga", Color = "#e62e52", Icon = "fas fa-times" };
        public static TherapistSupportTicketTopics EarningsQuestion => new TherapistSupportTicketTopics { Id = "earnings-question", Name = "Pitanje o zaradi", Color = "#00d27a", Icon = "far fa-usd-circle" };
        public static TherapistSupportTicketTopics EarningsWithdrawalQuestion => new TherapistSupportTicketTopics { Id = "earnings-withdrawal-question", Name = "Pitanje o isplati zarade", Color = "#00d27a", Icon = "far fa-arrow-to-bottom" };
    }
}
