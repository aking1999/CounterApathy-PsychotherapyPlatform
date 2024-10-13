using Database.Models;
using System.Threading.Tasks;

namespace WebApplication9.Interfaces
{
    public enum TherapistStripeAccountCompletionStatus
    {
        Completed,
        NotSetUpYet,
        Error
    }

    interface IStripeFunctionsProvider
    {
        string GetStripeCustomerId(string userId);
        string GetStripeCustomerIdByEmail(string email);
        StripeCustomers GetStripeCustomer(CustomClient user);
        string GetTherapistStripeAccountId(string therapistId);
        StripeAccount GetTherapistStripeAccount(Therapists therapist);
        Task<TherapistStripeAccountCompletionStatus> TherapistHasCompletedStripeAccountSetupAsync();
        Task<TherapistStripeAccountCompletionStatus> TherapistHasCompletedStripeAccountSetupAsync(Therapists therapist);
        StripePaymentIntents GetLatestPaymentIntentByUserId(string userId);
        StripePaymentIntents GetLatestReadOnlyPaymentIntentByUserId(string userId);
        StripePaymentIntents GetLatestPaymentIntentByEmail(string email);
        StripePaymentIntents GetLatestReadOnlyPaymentIntentByEmail(string email);
    }
}
