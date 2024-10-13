using Database.Models;
using Database.RepositoryImplementations;
using Framework.Interfaces;
using Framework.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication9.Interfaces;
using WebApplication9.Models;

namespace WebApplication9.Implementations
{
    public class StripeFunctionsProvider : IStripeFunctionsProvider
    {
        private IErrorLogger _errors => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IErrorLogger>();
        private readonly UnitOfWork _context;
        private readonly StripeSettings _stripeSettings;
        private ClaimsPrincipal User => new HttpContextAccessor().HttpContext.User;
        private UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        public StripeFunctionsProvider()
        {
            _stripeSettings = new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("StripeSettings").Get<StripeSettings>();
            StripeConfiguration.ApiKey = _stripeSettings.ApiKey;
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public string GetStripeCustomerId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var stripeCustomers = _context.StripeCustomers.ReadOnlyFind(acc => acc.UserIdOrAnonymous == userId);
            return stripeCustomers.Any() ? stripeCustomers.OrderByDescending(acc => acc.CreatedDateTime).ToList().ElementAt(0).Id : null;
        }

        public string GetStripeCustomerIdByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            email = email.ToLower();
            var stripeCustomers = _context.StripeCustomers.ReadOnlyFind(acc => acc.Email.ToLower() == email);
            return stripeCustomers.Any() ? stripeCustomers.OrderByDescending(acc => acc.CreatedDateTime).ToList().ElementAt(0).Id : null;
        }

        public StripeCustomers GetStripeCustomer(CustomClient user)
        {
            if (user == null)
                return null;

            var stripeCustomers = _context.StripeCustomers.ReadOnlyFind(acc => acc.UserIdOrAnonymous == user.Id);

            return stripeCustomers.Any() ? stripeCustomers.OrderByDescending(acc => acc.CreatedDateTime).ToList().ElementAt(0) : null;
        }

        public string GetTherapistStripeAccountId(string therapistId)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return null;

            var stripeAccounts = _context.StripeAccounts.ReadOnlyFind(acc => acc.TherapistId == therapistId);
            return stripeAccounts.Any() ? stripeAccounts.OrderByDescending(acc => acc.CreatedDateTime).ToList().ElementAt(0).Id : null;
        }

        public StripeAccount GetTherapistStripeAccount(Therapists therapist)
        {
            if (therapist == null)
                return null;

            var stripeAccounts = _context.StripeAccounts.ReadOnlyFind(acc => acc.TherapistId == therapist.Id);

            return stripeAccounts.Any() ? stripeAccounts.OrderByDescending(acc => acc.CreatedDateTime).ToList().ElementAt(0) : null;
        }

        public async Task<TherapistStripeAccountCompletionStatus> TherapistHasCompletedStripeAccountSetupAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return TherapistStripeAccountCompletionStatus.Error;

                var therapist = _context.Therapists.GetById(user.TherapistAccountId);

                if (therapist == null)
                    return TherapistStripeAccountCompletionStatus.Error;

                var stripeAccounts = _context.StripeAccounts.ReadOnlyFind(acc => acc.TherapistId == therapist.Id);

                if(!stripeAccounts.Any())
                    return TherapistStripeAccountCompletionStatus.NotSetUpYet;

                var account = new AccountService().Get(stripeAccounts.OrderByDescending(acc => acc.CreatedDateTime).ToList().ElementAt(0).Id);

                if (!account.ChargesEnabled || !account.PayoutsEnabled || !account.DetailsSubmitted)
                    return TherapistStripeAccountCompletionStatus.NotSetUpYet;

                return TherapistStripeAccountCompletionStatus.Completed;
            }
            catch (Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "StripeFunctionsProvider", "TherapistHasCompletedStripeAccountSetupAsync");
                return TherapistStripeAccountCompletionStatus.Error;
            }
        }

        public async Task<TherapistStripeAccountCompletionStatus> TherapistHasCompletedStripeAccountSetupAsync(Therapists therapist)
        {
            try
            {
                if (therapist == null)
                    return TherapistStripeAccountCompletionStatus.Error;

                var stripeAccounts = _context.StripeAccounts.ReadOnlyFind(acc => acc.TherapistId == therapist.Id);

                if (!stripeAccounts.Any())
                    return TherapistStripeAccountCompletionStatus.NotSetUpYet;

                var account = new AccountService().Get(stripeAccounts.OrderByDescending(acc => acc.CreatedDateTime).ToList().ElementAt(0).Id);

                if (!account.ChargesEnabled || !account.PayoutsEnabled || !account.DetailsSubmitted)
                    return TherapistStripeAccountCompletionStatus.NotSetUpYet;

                return TherapistStripeAccountCompletionStatus.Completed;
            }
            catch (Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "StripeFunctionsProvider", "TherapistHasCompletedStripeAccountSetupAsync");
                return TherapistStripeAccountCompletionStatus.Error;
            }
        }

        public StripePaymentIntents GetLatestPaymentIntentByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return default;

            return _context.StripePaymentIntents.Find(payment => payment.UserIdOrAnonymous == userId).OrderByDescending(payment => payment.CreatedDateTime).ElementAtOrDefault(0);
        }

        public StripePaymentIntents GetLatestReadOnlyPaymentIntentByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return default;

            return _context.StripePaymentIntents.ReadOnlyFind(payment => payment.UserIdOrAnonymous == userId).OrderByDescending(payment => payment.CreatedDateTime).ElementAtOrDefault(0);
        }

        public StripePaymentIntents GetLatestPaymentIntentByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return default;

            email = email.ToLower();
            return _context.StripePaymentIntents.Find(payment => payment.Email.ToLower() == email).OrderByDescending(payment => payment.CreatedDateTime).ElementAtOrDefault(0);
        }

        public StripePaymentIntents GetLatestReadOnlyPaymentIntentByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return default;

            email = email.ToLower();
            return _context.StripePaymentIntents.ReadOnlyFind(payment => payment.Email.ToLower() == email).OrderByDescending(payment => payment.CreatedDateTime).ElementAtOrDefault(0);
        }
    }
}
