using Database.Data;
using Database.Models;
using Database.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.RepositoryImplementations
{
    public class UnitOfWork : IDisposable
    {
        private readonly LajsnaProbaContext _context;
        public IRepository<Survey> Surveys { get; set; }
        public IRepository<Therapists> Therapists { get; set; }
        public IRepository<ContactMethods> ContactMethods { get; set; }
        public IRepository<TherapistsContactMethods> TherapistsContactMethods { get; set; }
        public IRepository<Specialities> Specialities { get; set; }
        public IRepository<TherapistApplications> TherapistApplications { get; set; }
        public IRepository<TherapistApplicationsSpecialities> TherapistApplicationsSpecialities { get; set; }
        public IRepository<AspNetUsers> AspNetUsers { get; set; }
        public IRepository<TherapistsSpecialities> TherapistsSpecialities { get; set; }
        public IRepository<Sessions> Sessions { get; set; }
        public IRepository<BookedSessions> BookedSessions { get; set; }
        public IRepository<PendingRatings> PendingRatings { get; set; }
        public IRepository<Ratings> Ratings { get; set; }
        public IRepository<HostedServicesInformation> HostedServicesInformation { get; set; }
        public IRepository<Withdrawals> Withdrawals { get; set; }
        public IRepository<Notifications> Notifications { get; set; }
        public IRepository<PsychotherapyTechniques> PsychotherapyTechniques { get; set; }
        public IRepository<TherapistPsychotherapyTechniques> TherapistPsychotherapyTechniques { get; set; }
        public IRepository<TherapistApplicationsPsychotherapyTechniques> TherapistApplicationsPsychotherapyTechniques { get; set; }
        public IRepository<WebCreditLogs> WebCreditLogs { get; set; }
        public IRepository<TherapistEarningsLogs> TherapistEarningsLogs { get; set; }
        public IRepository<ErrorLogs> ErrorLogs { get; set; }
        public IRepository<ClientSupportTickets> ClientSupportTickets { get; set; }
        public IRepository<ClientSupportTicketTopics> ClientSupportTicketTopics { get; set; }
        public IRepository<TherapistSupportTickets> TherapistSupportTickets { get; set; }
        public IRepository<TherapistSupportTicketTopics> TherapistSupportTicketTopics { get; set; }
        public IRepository<UserActivityLogs> UserActivityLogs { get; set; }
        public IRepository<UserAccountInformation> UserAccountInformation { get; set; }
        public IRepository<NewsletterSubscribers> NewsletterSubscribers { get; set; }
        public IRepository<BookedSessionsContactMethods> BookedSessionsContactMethods { get; set; }
        public IRepository<PaymentTypes> PaymentTypes { get; set; }
        public IRepository<PayPalPaymentRequests> PayPalPaymentRequests { get; set; }
        public IRepository<Transactions> Transactions { get; set; }
        public IRepository<StripeAccount> StripeAccounts { get; set; }
        public IRepository<StripeCustomers> StripeCustomers { get; set; }
        public IRepository<StripePaymentIntents> StripePaymentIntents { get; set; }
        public IRepository<Consultations> Consultations { get; set; }
        public IRepository<BookedConsultations> BookedConsultations { get; set; }
        public IRepository<BookedConsultationsContactMethods> BookedConsultationsContactMethods { get; set; }

        public UnitOfWork(LajsnaProbaContext context)
        {
            _context = context;
            Surveys = new Repository<Survey>(_context);
            Therapists = new Repository<Therapists>(_context);
            ContactMethods = new Repository<ContactMethods>(_context);
            TherapistsContactMethods = new Repository<TherapistsContactMethods>(_context);
            Specialities = new Repository<Specialities>(_context);
            TherapistApplicationsSpecialities = new Repository<TherapistApplicationsSpecialities>(_context);
            TherapistApplications = new Repository<TherapistApplications>(_context);
            AspNetUsers = new Repository<AspNetUsers>(_context);
            TherapistsSpecialities = new Repository<TherapistsSpecialities>(_context);
            Sessions = new Repository<Sessions>(_context);
            BookedSessions = new Repository<BookedSessions>(_context);
            PendingRatings = new Repository<PendingRatings>(_context);
            Ratings = new Repository<Ratings>(_context);
            HostedServicesInformation = new Repository<HostedServicesInformation>(_context);
            Withdrawals = new Repository<Withdrawals>(_context);
            Notifications = new Repository<Notifications>(_context);
            PsychotherapyTechniques = new Repository<PsychotherapyTechniques>(_context);
            TherapistPsychotherapyTechniques = new Repository<TherapistPsychotherapyTechniques>(_context);
            TherapistApplicationsPsychotherapyTechniques = new Repository<TherapistApplicationsPsychotherapyTechniques>(_context);
            WebCreditLogs = new Repository<WebCreditLogs>(_context);
            TherapistEarningsLogs = new Repository<TherapistEarningsLogs>(_context);
            ErrorLogs = new Repository<ErrorLogs>(_context);
            ClientSupportTickets = new Repository<ClientSupportTickets>(_context);
            ClientSupportTicketTopics = new Repository<ClientSupportTicketTopics>(_context);
            TherapistSupportTickets = new Repository<TherapistSupportTickets>(_context);
            TherapistSupportTicketTopics = new Repository<TherapistSupportTicketTopics>(_context);
            UserActivityLogs = new Repository<UserActivityLogs>(_context);
            UserAccountInformation = new Repository<UserAccountInformation>(_context);
            NewsletterSubscribers = new Repository<NewsletterSubscribers>(_context);
            BookedSessionsContactMethods = new Repository<BookedSessionsContactMethods>(_context);
            PaymentTypes = new Repository<PaymentTypes>(_context);
            PayPalPaymentRequests = new Repository<PayPalPaymentRequests>(_context);
            Transactions = new Repository<Transactions>(_context);
            StripeAccounts = new Repository<StripeAccount>(_context);
            StripeCustomers = new Repository<StripeCustomers>(_context);
            StripePaymentIntents = new Repository<StripePaymentIntents>(_context);
            Consultations = new Repository<Consultations>(_context);
            BookedConsultations = new Repository<BookedConsultations>(_context);
            BookedConsultationsContactMethods = new Repository<BookedConsultationsContactMethods>(_context);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        //dodao sam da bude async, i task sam dodao i await
        //ako bude problema, da vratim na staro, tj. da ne bude async
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            //_context.Database.BeginTransaction();
            _context.Dispose();
        }
    }
}
