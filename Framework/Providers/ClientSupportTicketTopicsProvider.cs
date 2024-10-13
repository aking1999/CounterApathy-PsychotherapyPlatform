using Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Providers
{
    public class ClientSupportTicketTopicsProvider
    {
        public static ClientSupportTicketTopics ReportError => new ClientSupportTicketTopics { Id = "report-error", Name = "Prijavite grešku / bag", Color = "#000", Icon = "far fa-bug" };
        public static ClientSupportTicketTopics AskQuestion => new ClientSupportTicketTopics { Id = "ask-question", Name = "Postavite pitanje", Color = "#27bcfd", Icon = "far fa-question" };
        public static ClientSupportTicketTopics GiveFeedback => new ClientSupportTicketTopics { Id = "feedback", Name = "Dajte povratnu informaciju", Color = "#34a4ff", Icon = "far fa-comment-alt-lines" };
        public static ClientSupportTicketTopics RequestAccountDeletion => new ClientSupportTicketTopics { Id = "account-deletion-request", Name = "Zahtev za gašenje naloga", Color = "#e62e52", Icon = "fas fa-times" };
        public static ClientSupportTicketTopics WebCreditQuestion => new ClientSupportTicketTopics { Id = "web-credit-question", Name = "Pitanje o Veb kreditu", Color = "#00d27a", Icon = "far fa-usd-circle" };
        public static ClientSupportTicketTopics WebCreditPaymentQuestion => new ClientSupportTicketTopics { Id = "web-credit-payment-question", Name = "Pitanje u vezi uplate Veb kredita", Color = "#00d27a", Icon = "far fa-usd-circle" };
    }
}
