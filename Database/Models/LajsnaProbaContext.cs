using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Database.Models
{
    public partial class LajsnaProbaContext : DbContext
    {
        public LajsnaProbaContext()
        {
        }

        public LajsnaProbaContext(DbContextOptions<LajsnaProbaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<BookedSessions> BookedSessions { get; set; }
        public virtual DbSet<ContactMethods> ContactMethods { get; set; }
        public virtual DbSet<HostedServicesInformation> HostedServicesInformation { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
        public virtual DbSet<PendingRatings> PendingRatings { get; set; }
        public virtual DbSet<Ratings> Ratings { get; set; }
        public virtual DbSet<Sessions> Sessions { get; set; }
        public virtual DbSet<Specialities> Specialities { get; set; }
        public virtual DbSet<Survey> Survey { get; set; }
        public virtual DbSet<TherapistApplications> TherapistApplications { get; set; }
        public virtual DbSet<TherapistApplicationsSpecialities> TherapistApplicationsSpecialities { get; set; }
        public virtual DbSet<Therapists> Therapists { get; set; }
        public virtual DbSet<TherapistsContactMethods> TherapistsContactMethods { get; set; }
        public virtual DbSet<TherapistsSpecialities> TherapistsSpecialities { get; set; }
        public virtual DbSet<Withdrawals> Withdrawals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=LajsnaProba;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.AmountDue).HasDefaultValueSql("((0.0000000000000000e+000))");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.FirstName).HasMaxLength(128);

                entity.Property(e => e.LastName).HasMaxLength(128);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.ProfilePhoto).HasMaxLength(450);

                entity.Property(e => e.SurveyId).HasMaxLength(450);

                entity.Property(e => e.TherapistAccountId).HasMaxLength(450);

                entity.Property(e => e.UserName).HasMaxLength(256);

                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.AspNetUsers)
                    .HasForeignKey(d => d.SurveyId)
                    .HasConstraintName("FK_Survey");

                entity.HasOne(d => d.TherapistAccount)
                    .WithMany(p => p.AspNetUsers)
                    .HasForeignKey(d => d.TherapistAccountId)
                    .HasConstraintName("FK_TherapistAccount");
            });

            modelBuilder.Entity<BookedSessions>(entity =>
            {
                entity.Property(e => e.ClientEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ClientFirstName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.ClientLastName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ContactInfo).HasMaxLength(450);

                entity.Property(e => e.ContactMethodName)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Description).HasMaxLength(192);

                entity.Property(e => e.SessionId).HasMaxLength(450);

                entity.Property(e => e.Subject).HasMaxLength(64);

                entity.Property(e => e.TherapistCity).HasMaxLength(128);

                entity.Property(e => e.TherapistCountry).HasMaxLength(128);

                entity.Property(e => e.TherapistEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.TherapistFirstName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TherapistHouseNumber).HasMaxLength(128);

                entity.Property(e => e.TherapistId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.TherapistIsPaid)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");

                entity.Property(e => e.TherapistLastName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TherapistPhoneNumber).IsRequired();

                entity.Property(e => e.TherapistPostalCode).HasMaxLength(64);

                entity.Property(e => e.TherapistStreet).HasMaxLength(128);
            });

            modelBuilder.Entity<ContactMethods>(entity =>
            {
                entity.Property(e => e.Color).HasMaxLength(100);

                entity.Property(e => e.Icon).HasMaxLength(450);

                entity.Property(e => e.Name).HasMaxLength(128);
            });

            modelBuilder.Entity<HostedServicesInformation>(entity =>
            {
                entity.Property(e => e.Information)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.InformationType)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Notifications>(entity =>
            {
                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasMaxLength(384);

                entity.Property(e => e.Icon).HasMaxLength(32);

                entity.Property(e => e.Read)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");

                entity.Property(e => e.ReceiverUserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.SenderUserId).HasMaxLength(450);

                entity.Property(e => e.Severity)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(384);

                entity.HasOne(d => d.ReceiverUser)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.ReceiverUserId)
                    .HasConstraintName("FK_NotificationsReceiverUserId_AspNetUsersId");
            });

            modelBuilder.Entity<PendingRatings>(entity =>
            {
                entity.Property(e => e.BookedSessionId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Comment).HasMaxLength(256);

                entity.Property(e => e.Rating).HasDefaultValueSql("((5.0000000000000000e+000))");

                entity.Property(e => e.Refused).HasDefaultValueSql("((0))");

                entity.Property(e => e.SessionId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.TherapistId)
                    .IsRequired()
                    .HasMaxLength(450);
            });

            modelBuilder.Entity<Ratings>(entity =>
            {
                entity.Property(e => e.AdminIdWhoApproved)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.BookedSessionId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.ClientFirstName).HasMaxLength(4);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.ClientLastName).HasMaxLength(4);

                entity.Property(e => e.Comment).HasMaxLength(256);

                entity.Property(e => e.Rating).HasDefaultValueSql("((5.0000000000000000e+000))");

                entity.Property(e => e.SessionId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.TherapistId)
                    .IsRequired()
                    .HasMaxLength(450);
            });

            modelBuilder.Entity<Sessions>(entity =>
            {
                entity.Property(e => e.Color).HasMaxLength(32);

                entity.Property(e => e.Description).HasMaxLength(192);

                entity.Property(e => e.Icon).HasMaxLength(32);

                entity.Property(e => e.Subject).HasMaxLength(64);

                entity.Property(e => e.TherapistId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Therapist)
                    .WithMany(p => p.Sessions)
                    .HasForeignKey(d => d.TherapistId)
                    .HasConstraintName("FK_Sessions_Therapists");
            });

            modelBuilder.Entity<Specialities>(entity =>
            {
                entity.Property(e => e.Color).HasMaxLength(32);

                entity.Property(e => e.Icon).HasMaxLength(32);

                entity.Property(e => e.Name).HasMaxLength(128);
            });

            modelBuilder.Entity<Survey>(entity =>
            {
                entity.Property(e => e.ListOfProblems).HasMaxLength(256);
            });

            modelBuilder.Entity<TherapistApplications>(entity =>
            {
                entity.Property(e => e.Accepted).HasDefaultValueSql("((0))");

                entity.Property(e => e.ApplicationDate).HasMaxLength(128);

                entity.Property(e => e.City).HasMaxLength(128);

                entity.Property(e => e.Country).HasMaxLength(128);

                entity.Property(e => e.Gender).HasMaxLength(30);

                entity.Property(e => e.HouseNumber).HasMaxLength(128);

                entity.Property(e => e.PastCompanies).HasMaxLength(512);

                entity.Property(e => e.PostalCode).HasMaxLength(64);

                entity.Property(e => e.ProfilePhoto).HasMaxLength(512);

                entity.Property(e => e.Street).HasMaxLength(128);

                entity.Property(e => e.University).HasMaxLength(512);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TherapistApplications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_TherapistApplications_AspNetUsers");
            });

            modelBuilder.Entity<TherapistApplicationsSpecialities>(entity =>
            {
                entity.HasKey(e => new { e.TherapistApplicationId, e.SpecialityId });

                entity.HasOne(d => d.Speciality)
                    .WithMany(p => p.TherapistApplicationsSpecialities)
                    .HasForeignKey(d => d.SpecialityId)
                    .HasConstraintName("FK_TherapistApplicationsSpecialities_Specialities");

                entity.HasOne(d => d.TherapistApplication)
                    .WithMany(p => p.TherapistApplicationsSpecialities)
                    .HasForeignKey(d => d.TherapistApplicationId)
                    .HasConstraintName("FK_TherapistApplicationsSpecialities_TherapistApplications");
            });

            modelBuilder.Entity<Therapists>(entity =>
            {
                entity.Property(e => e.About).HasMaxLength(1024);

                entity.Property(e => e.City).HasMaxLength(128);

                entity.Property(e => e.Country).HasMaxLength(128);

                entity.Property(e => e.Gender).HasMaxLength(30);

                entity.Property(e => e.HouseNumber).HasMaxLength(128);

                entity.Property(e => e.PastCompanies).HasMaxLength(512);

                entity.Property(e => e.PostalCode).HasMaxLength(64);

                entity.Property(e => e.Street).HasMaxLength(128);

                entity.Property(e => e.TherapistsContactMethodsId).HasMaxLength(450);

                entity.Property(e => e.University).HasMaxLength(512);
            });

            modelBuilder.Entity<TherapistsContactMethods>(entity =>
            {
                entity.HasKey(e => new { e.TherapistId, e.ContactMethodId });

                entity.HasOne(d => d.ContactMethod)
                    .WithMany(p => p.TherapistsContactMethods)
                    .HasForeignKey(d => d.ContactMethodId)
                    .HasConstraintName("FK_TherapistsContactMethods_ContactMethodId");

                entity.HasOne(d => d.Therapist)
                    .WithMany(p => p.TherapistsContactMethods)
                    .HasForeignKey(d => d.TherapistId)
                    .HasConstraintName("FK_ContactMethods");
            });

            modelBuilder.Entity<TherapistsSpecialities>(entity =>
            {
                entity.HasKey(e => new { e.TherapistId, e.SpecialityId });

                entity.HasOne(d => d.Speciality)
                    .WithMany(p => p.TherapistsSpecialities)
                    .HasForeignKey(d => d.SpecialityId)
                    .HasConstraintName("FK_TherapistsSpecialities_Specialities");

                entity.HasOne(d => d.Therapist)
                    .WithMany(p => p.TherapistsSpecialities)
                    .HasForeignKey(d => d.TherapistId)
                    .HasConstraintName("FK_TherapistsSpecialities_Therapists");
            });

            modelBuilder.Entity<Withdrawals>(entity =>
            {
                entity.Property(e => e.BankAccountNumber)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.HouseNumber)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).IsRequired();

                entity.Property(e => e.PostalCode)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Street)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TherapistId).HasMaxLength(450);

                entity.HasOne(d => d.Therapist)
                    .WithMany(p => p.Withdrawals)
                    .HasForeignKey(d => d.TherapistId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_WithdrawalsTherapistId_TherapistsId");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
