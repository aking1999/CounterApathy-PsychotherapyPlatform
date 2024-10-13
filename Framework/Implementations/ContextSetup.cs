using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers;
using Framework.Providers;
using Framework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Implementations
{
    public class ContextSetup : IContextSetup
    {
        private const string PROJECT = "Framework";
        private const string CLASS = "ContextSetup";

        private readonly IServiceProvider _serviceProvider;
        private readonly ISystemErrorLogger _systemErrors;
        private readonly UnitOfWork _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ContextSetup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        }

        public bool AddClientRole()
        {
            try
            {
                if (!_roleManager.RoleExistsAsync(UserRoles.Client).Result)
                {
                    var roleCreated = _roleManager.CreateAsync(new IdentityRole(UserRoles.Client)).Result;

                    return roleCreated.Succeeded ? true : throw new Exception(string.Join('|', roleCreated.Errors.Select(e => e.Description)));
                }

                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddClientRole");
                return false;
            }
        }

        public bool AddTherapistRole()
        {
            try
            {
                if (!_roleManager.RoleExistsAsync(UserRoles.Therapist).Result)
                {
                    var roleCreated = _roleManager.CreateAsync(new IdentityRole(UserRoles.Therapist)).Result;

                    return roleCreated.Succeeded ? true : throw new Exception(string.Join('|', roleCreated.Errors.Select(e => e.Description)));
                }

                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddTherapistRole");
                return false;
            }
        }

        public bool AddAdminRole()
        {
            try
            {
                if (!_roleManager.RoleExistsAsync(UserRoles.Admin).Result)
                {
                    var roleCreated = _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin)).Result;

                    return roleCreated.Succeeded ? true : throw new Exception(string.Join('|', roleCreated.Errors.Select(e => e.Description)));
                }

                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddAdminRole");
                return false;
            }
        }

        public bool AddAdminAccount()
        {
            try
            {
                var userManager = _serviceProvider.GetRequiredService<UserManager<CustomClient>>();
                var passwordHasher = _serviceProvider.GetRequiredService<IPasswordHasher<CustomClient>>();

                var admin1 = new CustomClient
                {
                    Id = Helper.GenerateNumbersId(),
                    UserName = "<email>",
                    NormalizedUserName = "<email>",
                    Email = "<email>",
                    NormalizedEmail = "<email>",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = "<phone>",
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    FirstName = "Admin",
                    LastName = "Main",
                    WebCredit = 1000000,
                    YearOfBirth = 1999
                };

                admin1.PasswordHash = passwordHasher.HashPassword(admin1, "<password>");

                if (AddAdminRole())
                {
                    if (userManager.FindByNameAsync(admin1.UserName).Result == null)
                    {
                        var created = userManager.CreateAsync(admin1).Result;
                        if (created.Succeeded)
                        {
                            var addedToRole = userManager.AddToRoleAsync(admin1, UserRoles.Admin).Result;

                            if (addedToRole.Succeeded)
                                return true;

                            throw new Exception(string.Join('|', addedToRole.Errors.Select(e => e.Description)));
                        }

                        throw new Exception(string.Join('|', created.Errors.Select(e => e.Description)));
                    }
                }
                else throw new Exception("Unable to add Admin role.");

                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddAdminAccount");
                return false;
            }
        }

        public bool AddContactMethods()
        {
            try
            {
                var contactMethods = new List<ContactMethods>
                {
                    ContactMethodsProvider.InPerson,
                    ContactMethodsProvider.GoogleMeet
                };

                foreach (var contactMethod in contactMethods)
                {
                    if (_context.ContactMethods.GetById(contactMethod.Id) == null)
                        _context.ContactMethods.Insert(contactMethod);
                }

                _context.Save();
                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddContactMethods");
                return false;
            }
        }

        public bool AddSpecialties()
        {
            try
            {
                var specialties = new List<Specialities>
                {
                    new Specialities { Id = "samopouzdanje", Name = "Samopouzdanje", Color = "#e0a902", Icon = "fal fa-crown" },
                    new Specialities { Id = "zavisnost", Name = "Zavisnost", Color = "#030361", Icon = "fal fa-smoking" },
                    new Specialities { Id = "lični-razvoj", Name = "Lični razvoj", Color = "#00d27a", Icon = "fal fa-sort-amount-up" },
                    new Specialities { Id = "životne-krize", Name = "Životne krize", Color = "#dc3545", Icon = "fal fa-heart-rate" },
                    new Specialities { Id = "zlostavljanje", Name = "Zlostavljanje", Color = "#dc3545", Icon = "fal fa-sad-tear" },
                    new Specialities { Id = "nasilje-u-porodici", Name = "Nasilje u porodici", Color = "#dc3545", Icon = "fal fa-fist-raised" },
                    new Specialities { Id = "gubitak-i-tuga", Name = "Gubitak i tuga", Color = "#34a4ff", Icon = "far fa-tombstone" },
                    new Specialities { Id = "posttraumatski-stresni-poremećaj", Name = "Posttraumatski stresni poremećaj", Color = "#34a4ff", Icon = "fal fa-bomb" },
                    new Specialities { Id = "anksioznost", Name = "Anksioznost", Color = "#030361", Icon = "fal fa-podium" },
                    new Specialities { Id = "prokrastinacija", Name = "Prokrastinacija", Color = "#00d27a", Icon = "fal fa-bed" },
                    new Specialities { Id = "kontrola-besa", Name = "Kontrola besa", Color = "#dc3545", Icon = "fal fa-angry" },
                    new Specialities { Id = "bipolarni-poremećaj", Name = "Bipolarni poremećaj", Color = "#b134ff", Icon = "fal fa-theater-masks" },
                    new Specialities { Id = "seksualno-zlostavljanje", Name = "Seksualno zlostavljanje", Color = "#e0a902", Icon = "fal fa-sickle" },
                    new Specialities { Id = "depresija", Name = "Depresija", Color = "#030361", Icon = "fal fa-frown" },
                    new Specialities { Id = "opsesivno-kompulsivni-poremećaj", Name = "Opsesivno-kompulsivni poremećaj", Color = "#34a4ff", Icon = "fal fa-pump-soap" },
                    new Specialities { Id = "poremećaj-ishrane", Name = "Poremećaj ishrane", Color = "#dc3545", Icon = "fal fa-burger-soda" },
                    new Specialities { Id = "sukobi-u-porodici", Name = "Sukobi u porodici", Color = "#34a4ff", Icon = "fal fa-house-damage" },
                    new Specialities { Id = "strah-od-bliskosti", Name = "Strah od bliskosti", Color = "#d417b4", Icon = "fal fa-praying-hands" },
                    new Specialities { Id = "izolacija-i-samoća", Name = "Izolacija i samoća", Color = "#34a4ff", Icon = "fal fa-people-arrows" },
                    new Specialities { Id = "nagle-promene-raspoloženja", Name = "Nagle promene raspoloženja", Color = "#34a4ff", Icon = "fal fa-cloud-moon-rain" },
                    new Specialities { Id = "partnerski-odnosi", Name = "Partnerski odnosi", Color = "#d417b4", Icon = "fal fa-heart" },
                    new Specialities { Id = "traume", Name = "Traume", Color = "#030361", Icon = "fal fa-scarecrow" },
                    new Specialities { Id = "panični-napadi", Name = "Panični napadi", Color = "#dc3545", Icon = "fal fa-exclamation-circle" },
                    new Specialities { Id = "fobije", Name = "Fobije", Color = "#030361", Icon = "fal fa-ghost" },
                    new Specialities { Id = "samopovređivanje", Name = "Samopovređivanje", Color = "#dc3545", Icon = "fal fa-scalpel" },
                    new Specialities { Id = "poremećaj-spavanja", Name = "Poremećaj spavanja", Color = "#00d27a", Icon = "fal fa-bed-empty" },
                    new Specialities { Id = "stres", Name = "Stres", Color = "#00d27a", Icon = "fal fa-thunderstorm" },
                    new Specialities { Id = "komunikacijske-veštine", Name = "Komunikacijske veštine", Color = "#d417b4", Icon = "ca-icon-chat-heart" },
                    new Specialities { Id = "burnout", Name = "Burnout", Color = "#f57205", Icon = "fal fa-fire" }
                };

                foreach (var specialty in specialties)
                {
                    if (_context.Specialities.GetById(specialty.Id) == null)
                        _context.Specialities.Insert(specialty);

                }

                _context.Save();
                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddSpecialties");
                return false;
            }
        }

        public bool AddPsychotherapyTechniques()
        {
            try
            {
                var psychotherapyTechniques = new List<PsychotherapyTechniques>
                {
                    new PsychotherapyTechniques { Id = "bihejvioralna-terapija", Name = "Bihejvioralna terapija", Color = "#00d27a", Icon = "fal fa-puzzle-piece" },
                    new PsychotherapyTechniques { Id = "psihodinamska-terapija", Name = "Psihodinamska terapija", Color = "#b134ff", Icon = "fal fa-atom-alt" },
                    new PsychotherapyTechniques { Id = "psihoanaliza", Name = "Psihoanaliza", Color = "#34a4ff", Icon = "fal fa-search" },
                    new PsychotherapyTechniques { Id = "geštalt", Name = "Geštalt", Color = "#b134ff", Icon = "fal fa-shapes" },
                    new PsychotherapyTechniques { Id = "dijalektička-bihejvioralna-terapija", Name = "Dijalektička bihejvioralna terapija", Color = "#030361", Icon = "fal fa-yin-yang" },
                    new PsychotherapyTechniques { Id = "humanistička-terapija", Name = "Humanistička terapija", Color = "#007bff", Icon = "fal fa-male" },
                    new PsychotherapyTechniques { Id = "kognitivno-bihejvioralna-terapija", Name = "Kognitivno-bihejvioralna terapija", Color = "#eb4034", Icon = "fal fa-brain" },
                    new PsychotherapyTechniques { Id = "transakciona-analiza", Name = "Transakciona analiza", Color = "#34a4ff", Icon = "fal fa-chart-network" },
                    new PsychotherapyTechniques { Id = "sistemska-porodična-terapija", Name = "Sistemska porodična terapija", Color = "#d417b4", Icon = "fal fa-home-heart" },
                    new PsychotherapyTechniques { Id = "asertivni-trening", Name = "Asertivni trening", Color = "#007bff", Icon = "ca-icon-man-assertive" },
                    new PsychotherapyTechniques { Id = "racionalno-emotivno-bihejvioralna-terapija", Name = "Racionalno-emotivno-bihejvioralna terapija", Color = "#d417b4", Icon = "ca-icon-heart-head" },
                    new PsychotherapyTechniques { Id = "mindfulness", Name = "Mindfulness terapija", Color = "#00d27a", Icon = "ca-icon-water-lily" }
                };

                foreach (var technique in psychotherapyTechniques)
                {
                    if (_context.PsychotherapyTechniques.GetById(technique.Id) == null)
                        _context.PsychotherapyTechniques.Insert(technique);
                }

                _context.Save();
                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddPsychotherapyTechniques");
                return false;
            }
        }

        public bool AddClientSupportTicketTopics()
        {
            try
            {
                var clientSupportTicketTopics = new List<ClientSupportTicketTopics>
                {
                    ClientSupportTicketTopicsProvider.ReportError,
                    ClientSupportTicketTopicsProvider.AskQuestion,
                    ClientSupportTicketTopicsProvider.GiveFeedback,
                    ClientSupportTicketTopicsProvider.RequestAccountDeletion,
                    ClientSupportTicketTopicsProvider.WebCreditQuestion,
                    ClientSupportTicketTopicsProvider.WebCreditPaymentQuestion,
                };

                foreach (var topic in clientSupportTicketTopics)
                {
                    if (_context.ClientSupportTicketTopics.GetById(topic.Id) == null)
                        _context.ClientSupportTicketTopics.Insert(topic);

                }

                _context.Save();
                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddClientSupportTicketTopics");
                return false;
            }
        }

        public bool AddTherapistSupportTicketTopics()
        {
            try
            {
                var therapistSupportTicketTopics = new List<TherapistSupportTicketTopics>
                {
                    TherapistSupportTicketTopicsProvider.ReportError,
                    TherapistSupportTicketTopicsProvider.AskQuestion,
                    TherapistSupportTicketTopicsProvider.GiveFeedback,
                    TherapistSupportTicketTopicsProvider.RequestAccountDeletion,
                    TherapistSupportTicketTopicsProvider.EarningsQuestion,
                    TherapistSupportTicketTopicsProvider.EarningsWithdrawalQuestion
                };

                foreach (var topic in therapistSupportTicketTopics)
                {
                    if (_context.TherapistSupportTicketTopics.GetById(topic.Id) == null)
                        _context.TherapistSupportTicketTopics.Insert(topic);
                }

                _context.Save();
                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddTherapistSupportTicketTopics");
                return false;
            }
        }

        public bool AddPaymentTypes()
        {
            try
            {
                var paymentTypes = new List<PaymentTypes>
                {
                    PaymentTypesProvider.Withdrawal,
                    PaymentTypesProvider.PaymentCards,
                    PaymentTypesProvider.BankTransfer,
                    PaymentTypesProvider.Posta,
                    PaymentTypesProvider.PayPal,
                    PaymentTypesProvider.CounterApathyPayment,
                    PaymentTypesProvider.USDCoin,
                    PaymentTypesProvider.USDTether
                };

                foreach (var type in paymentTypes)
                {
                    if (_context.PaymentTypes.GetById(type.Id) == null)
                        _context.PaymentTypes.Insert(type);
                }

                _context.Save();
                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddPaymentTypes");
                return false;
            }
        }

        public bool PrepareToRun()
        {
            try
            {
                var clientRoleAdded = AddClientRole();
                var therapistRoleAdded = AddTherapistRole();
                var adminRoleAdded = AddAdminRole();
                var adminAccountAdded = AddAdminAccount();
                var contactMethodsAdded = AddContactMethods();
                var specialtiesAdded = AddSpecialties();
                var techniquesAdded = AddPsychotherapyTechniques();
                var clientTicketTopicsAdded = AddClientSupportTicketTopics();
                var therapistTicketTopicsAdded = AddTherapistSupportTicketTopics();
                var paymentTypesAdded = AddPaymentTypes();

                var failed = string.Empty;

                if (!clientRoleAdded) failed += "Client role not added.|";
                if (!therapistRoleAdded) failed += "Therapist role not added.|";
                if (!adminRoleAdded) failed += "Admin role not added.|";
                if (!adminAccountAdded) failed += "Admin account not added.|";
                if (!contactMethodsAdded) failed += "Contact methods not added.|";
                if (!specialtiesAdded) failed += "Specialties not added.|";
                if (!techniquesAdded) failed += "Techniques not added.|";
                if (!clientTicketTopicsAdded) failed += "Client support ticket topics not added.|";
                if (!therapistTicketTopicsAdded) failed += "Therapist support ticket topics not added.|";
                if (!paymentTypesAdded) failed += "Payment types not added.";

                if (!string.IsNullOrEmpty(failed)) throw new Exception(failed);

                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "PrepareToRun");
                return false;
            }
        }
    }
}
