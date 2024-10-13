using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Framework.Emails.EmailTypes;

namespace Framework.Emails
{
    public interface IMailService
    {
        Task SendEmailAsync(Email email);
        Task SendEmailAsync(EmailForRole email, string userRole);
        Task SendEmailConfirmationEmailAsync(ConfirmationEmail email, bool includeTemplateIfExists);
        Task SendPasswordResetEmailAsync(PasswordResetEmail email, bool includeTemplateIfExists);
        Task SendWelcomeEmailAsync(WelcomeEmail email, bool includeTemplateIfExists);
        Task SendSignInSuccessfulEmailAsync(SignInSuccessfulEmail email, bool includeTemplateIfExists);
        Task SendPasswordChangedEmailAsync(PasswordChangedEmail email, bool includeTemplateIfExists);
        Task SendWebCreditAddedEmailAsync(WebCreditAddedEmail email, bool includeTemplateIfExists);
        Task SendWithdrawalRequestAcceptedEmailAsync(WithdrawalRequestAcceptedEmail email, bool includeTemplateIfExists);
        Task SendTherapistApplicationReceivedEmailAsync(TherapistApplicationStatusEmail email, bool includeTemplateIfExists);
        Task SendTherapistApplicationAcceptedEmailAsync(TherapistApplicationStatusEmail email, bool includeTemplateIfExists);
        Task SendTherapistApplicationRejectedEmailAsync(TherapistApplicationStatusEmail email, bool includeTemplateIfExists);
        Task SendClientBookedConsultationEmailAsync(BookedConsultationEmail email, bool includeTemplateIfExists);
        Task SendClientBookedSessionEmailAsync(BookedSessionEmail email, bool includeTemplateIfExists);
        Task SendTherapistConsultationBookedEmailAsync(BookedConsultationEmail email, bool includeTemplateIfExists);
        Task SendTherapistSessionBookedEmailAsync(BookedSessionEmail email, bool includeTemplateIfExists);
        Task SendConsultationInviteLinkEmailAsync(ConsultationInviteLinkEmail email, bool includeTemplateIfExists);
        Task SendSessionInviteLinkEmailAsync(SessionInviteLinkEmail email, bool includeTemplateIfExists);
        Task SendUnratedSessionsReminderEmailAsync(UnratedSessionReminderEmail email, bool includeTemplateIfExists);
        //Task SendPayPalPaymentSuccessfulEmailAsync(PayPalPaymentSuccessfulEmail email, bool includeTemplateIfExists);
    }
}
