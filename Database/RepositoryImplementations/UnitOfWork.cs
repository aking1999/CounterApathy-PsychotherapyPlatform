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
            _context.Dispose();
        }
    }
}
