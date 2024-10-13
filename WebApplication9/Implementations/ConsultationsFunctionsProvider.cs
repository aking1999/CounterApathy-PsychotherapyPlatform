using Database.Models;
using Database.RepositoryImplementations;
using DataTransferObjects.ViewModels.Client;
using DataTransferObjects.ViewModels.Therapist;
using Framework.Helpers.ExtensionMethods;
using Framework.Implementations;
using Framework.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Helpers;
using WebApplication9.Interfaces;
using WebApplication9.ViewModels;

namespace WebApplication9.Implementations
{
    public class ConsultationsFunctionsProvider : IConsultationsFunctionsProvider
    {
        private readonly IFileRepository _files;
        private readonly IDateTimeHelper _dateHelper;
        private IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        private readonly ISystemErrorLogger _systemErrors;
        private readonly UnitOfWork _context;
        private UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        public ConsultationsFunctionsProvider(IDateTimeHelper dateHelper)
        {
            _files = new FileRepository();
            _dateHelper = dateHelper;
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public bool UserHasConsultationToAttendDuringPeriod(string userId, DateTime periodStart, DateTime periodEnd)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            return _context.BookedConsultations.ReadOnlyAny(s => s.StartDateTime < periodEnd &&
                                                                 s.EndDateTime > periodStart);
        }

        public bool EmailHasConsultationToAttendDuringPeriod(string email, DateTime periodStart, DateTime periodEnd)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.ToUpperInvariant();
            return _context.BookedConsultations.ReadOnlyAny(s => s.StartDateTime < periodEnd &&
                                                                 s.EndDateTime > periodStart);
        }

        public bool TherapistHasConsultationDuringPeriod(string therapistId, DateTime periodStart, DateTime periodEnd)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return false;

            return _context.Consultations.ReadOnlyAny(s => s.TherapistId == therapistId &&
                                                           s.StartDateTime < periodEnd &&
                                                           s.EndDateTime > periodStart);
        }

        public TherapistConsultationsViewModel GetTherapistConsultationsGroupedByDate(string therapistId)
        {
            //var maxDate = minDate.AddDays(7);
            var minDate = DateTime.UtcNow.AddHours(24);

            var therapistConsultationVm = new TherapistConsultationsViewModel
            {
                TherapistId = therapistId
            };

            foreach (var consultation in (from consultation in _context.Consultations.ReadOnlyFind(c => c.TherapistId == therapistId && minDate < c.StartDateTime && c.Booked == 0)
                                          group consultation by consultation.StartDateTime.Date into groupedConsultationsByDate
                                          orderby groupedConsultationsByDate.Key
                                          select groupedConsultationsByDate).ToList())
            {
                var dayOfWeekName = string.Empty;

                switch (consultation.Key.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        {
                            dayOfWeekName = "Ponedeljak";
                            break;
                        }
                    case DayOfWeek.Tuesday:
                        {
                            dayOfWeekName = "Utorak";
                            break;
                        }
                    case DayOfWeek.Wednesday:
                        {
                            dayOfWeekName = "Sreda";
                            break;
                        }
                    case DayOfWeek.Thursday:
                        {
                            dayOfWeekName = "Četvrtak";
                            break;
                        }
                    case DayOfWeek.Friday:
                        {
                            dayOfWeekName = "Petak";
                            break;
                        }
                    case DayOfWeek.Saturday:
                        {
                            dayOfWeekName = "Subota";
                            break;
                        }
                    case DayOfWeek.Sunday:
                        {
                            dayOfWeekName = "Nedelja";
                            break;
                        }
                    default:
                        {
                            _systemErrors.SaveError($"Unrecognized week day: {consultation.Key.DayOfWeek}.", "WebApplication9", "ConsultationsFunctionsProvider", "GetTherapistConsultationsGroupedByDate");
                            break;
                        }
                }

                consultation.ToList().ForEach(c =>
                {
                    c.StartDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(c.StartDateTime);
                    c.EndDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(c.EndDateTime);
                });

                therapistConsultationVm.DayOfWeekNameConsultations.Add(new DayOfWeekNameConsultationsViewModel
                {
                    DayOfWeekName = dayOfWeekName,
                    DayOfWeekDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalDateString(consultation.Key), //ovde mozda treba consultation.Key.Date
                    Consultations = consultation.ToList()
                });
            }

            return therapistConsultationVm;
        }

        public bool UserAlreadyBookedConsultationWithTherapist(string userId, string therapistId)
        {
            return _context.BookedConsultations.ReadOnlyAny(c => c.ClientId == userId && c.TherapistId == therapistId);
        }

        public List<BookedConsultations> GetBookedConsultations(string filter, string predicate)
        {
            var bookedConsultations = _context.BookedConsultations.ReadOnlyGetAll();

            if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(predicate))
                return bookedConsultations
                               .OrderByDescending(s => s.StartDateTime)
                               .ToList();

            switch (filter.ToLower())
            {
                case "datum":
                    {
                        switch (predicate.ToLower())
                        {
                            case "danas":
                                {
                                    return bookedConsultations
                                                    .Where(c => c.StartDateTime.Date == DateTime.UtcNow.Date)
                                                    .OrderByDescending(s => s.StartDateTime)
                                                    .ToList();
                                }
                            case "ova-nedelja":
                                {
                                    var startOfWeek = _dateHelper.GetStartOfWeekDate(DayOfWeek.Monday);
                                    var nextMonday = startOfWeek.AddDays(7);

                                    return bookedConsultations
                                                   .Where(s => startOfWeek <= s.StartDateTime &&
                                                          s.StartDateTime <= nextMonday)
                                                   .OrderByDescending(s => s.StartDateTime)
                                                   .ToList();
                                }
                            case "ovaj-mesec":
                                {
                                    var now = DateTime.UtcNow;
                                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                                    return bookedConsultations
                                                   .Where(s => firstDayOfMonth <= s.StartDateTime &&
                                                          s.StartDateTime <= lastDayOfMonth)
                                                   .OrderByDescending(s => s.StartDateTime)
                                                   .ToList();
                                }
                            case "ova-godina":
                                {
                                    var thisYear = DateTime.UtcNow.Year;
                                    var startOfYear = new DateTime(thisYear, 1, 1);
                                    var endOfYear = new DateTime(thisYear, 12, 31);

                                    return bookedConsultations
                                                   .Where(s => startOfYear <= s.StartDateTime &&
                                                          s.StartDateTime <= endOfYear)
                                                   .OrderByDescending(s => s.StartDateTime)
                                                   .ToList();
                                }
                            default: return new List<BookedConsultations>();
                        }
                    }
                case "status":
                    {
                        switch (predicate.ToLower())
                        {
                            case "na-čekanju":
                                {
                                    return bookedConsultations
                                                   .Where(s => s.EndDateTime >= DateTime.UtcNow)
                                                   .OrderByDescending(s => s.StartDateTime)
                                                   .ToList();
                                }
                            case "završeno":
                                {
                                    return bookedConsultations
                                                   .Where(s => s.EndDateTime <= DateTime.UtcNow)
                                                   .OrderByDescending(s => s.StartDateTime)
                                                   .ToList();
                                }
                            default: return new List<BookedConsultations>();
                        }
                    }
                default: return new List<BookedConsultations>();
            }
        }

        // Therapist/BookedConsultations
        public List<BookedConsultationViewModel> GetTherapistBookedConsultations(string therapistId, string filter, string predicate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(therapistId))
                    return new List<BookedConsultationViewModel>();

                var bookedConsultationVms = new List<BookedConsultationViewModel>();

                foreach (var bookedConsultation in GetBookedConsultations(filter, predicate)
                                                        .Where(c => c.TherapistId == therapistId)
                                                        .ToList())
                {
                    // This must be exactly in this order
                    var temp = new BookedConsultationViewModel();
                    temp.Map(bookedConsultation);
                    temp.StartDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.StartDateTime);
                    temp.EndDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.EndDateTime);
                    temp.BookingDate = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.BookingDate);

                    temp.ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, bookedConsultation.ClientId);

                    var contactMethod = _context.ContactMethods.GetById(temp.ContactMethod.Id);

                    if (contactMethod != null)
                    {
                        temp.ContactMethod.Name = contactMethod.Name;
                        temp.ContactMethod.Color = contactMethod.Color;
                        temp.ContactMethod.Icon = contactMethod.Icon;
                    }

                    bookedConsultationVms.Add(temp);
                }

                return bookedConsultationVms;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "WebApplication9", "ConsultationsFunctionsProvider", "GetTherapistBookedConsultations");
                return new List<BookedConsultationViewModel>();
            }
        }

        // Therapist/Details
        public async Task<BookedConsultationAddInviteLinkViewModel> GetBookedConsultationDetailsForTherapistAsync(string therapistId, string bookingId)
        {
            try
            {
                var bookedConsultationInviteLink = (from bc in _context.BookedConsultations.ReadOnlyFind(c => c.TherapistId == therapistId && c.Id == bookingId)
                                                    join bcCm in _context.BookedConsultationsContactMethods.ReadOnlyFind(c => c.BookedConsultationId == bookingId).ToList() // .ToList() must be here or DataReaderException
                                                    on bc.Id equals bcCm.BookedConsultationId
                                                    where bc.ContactMethodId == bcCm.ContactMethodId
                                                    select new BookedConsultationAddInviteLinkViewModel(bcCm.InviteLink, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, bc.ClientId),
                                                                                                  bc.ConsultationId, bc.Id, bc.TherapistId,
                                                                                                  bc.TherapistFirstName, bc.TherapistLastName,
                                                                                                  bc.TherapistEmail, bc.TherapistPhoneNumber,
                                                                                                  bc.ClientId, bc.ClientFirstName, bc.ClientLastName,
                                                                                                  bc.ClientEmail, bc.ClientPhoneNumber,
                                                                                                  _dateHelper.ConvertDateTimeFromUtcToLocal(bc.StartDateTime),
                                                                                                  _dateHelper.ConvertDateTimeFromUtcToLocal(bc.EndDateTime),
                                                                                                  _dateHelper.ConvertDateTimeFromUtcToLocal(bc.BookingDate),
                                                                                                  bc.ContactMethodId, bc.ContactMethodName, bc.ContactMethodColor, bc.ContactMethodIcon)).SingleOrDefault();

                if (bookedConsultationInviteLink == default)
                {
                    var bookedConsultation = _context.BookedConsultations.GetById(bookingId);

                    if (bookedConsultation == null || bookedConsultation.TherapistId != therapistId) return default;

                    bookedConsultationInviteLink = new BookedConsultationAddInviteLinkViewModel(null, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, bookedConsultation.ClientId),
                                                                                          bookedConsultation.ConsultationId, bookedConsultation.Id, bookedConsultation.TherapistId,
                                                                                          bookedConsultation.TherapistFirstName, bookedConsultation.TherapistLastName,
                                                                                          bookedConsultation.TherapistEmail, bookedConsultation.TherapistPhoneNumber,
                                                                                          bookedConsultation.ClientId, bookedConsultation.ClientFirstName, bookedConsultation.ClientLastName,
                                                                                          bookedConsultation.ClientEmail, bookedConsultation.ClientPhoneNumber,
                                                                                          _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.StartDateTime),
                                                                                          _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.EndDateTime),
                                                                                          _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.BookingDate),
                                                                                          bookedConsultation.ContactMethodId, bookedConsultation.ContactMethodName, bookedConsultation.ContactMethodColor, bookedConsultation.ContactMethodIcon);
                }

                // If PhoneNumber is deleted in meantime in BookedConsultations, we start
                // taking the PhoneNumber info from AspNetUsers for that client.
                if (string.IsNullOrWhiteSpace(bookedConsultationInviteLink.BookedConsultation.ClientPhoneNumber))
                    bookedConsultationInviteLink.BookedConsultation.ClientPhoneNumber = (await _userManager.FindByIdAsync(bookedConsultationInviteLink.BookedConsultation.ClientId)).PhoneNumber;

                // If the ContactMethod data are changed in the meantime in table ContactMethods,
                // here we update them in BookedConsultationAddInviteLink, but if that ContactMethod is deleted,
                // then we leave the ContactMethod data from BookedConsultationAddInviteLink
                var contactMethod = _context.ContactMethods.GetById(bookedConsultationInviteLink.BookedConsultation.ContactMethod.Id);
                if (contactMethod != null)
                {
                    bookedConsultationInviteLink.BookedConsultation.ContactMethod.Name = contactMethod.Name;
                    bookedConsultationInviteLink.BookedConsultation.ContactMethod.Color = contactMethod.Color;
                    bookedConsultationInviteLink.BookedConsultation.ContactMethod.Icon = contactMethod.Icon;
                }

                return bookedConsultationInviteLink;
            }
            catch(Exception e)
            {
                await _systemErrors.SaveErrorAsync(e, "WebApplication9", "ConsultationsFunctionsProvider", "GetBookedConsultationDetailsForTherapistAsync");
                return default;
            }
        }

        // Client/ConsultationDetails
        public async Task<BookedConsultationInviteLinkViewModel> GetBookedConsultationDetailsForClientAsync(string userId, string bookingId)
        {
            try
            {
                var bookedConsultationInviteLinkVm = (from bc in _context.BookedConsultations.ReadOnlyFind(c => c.ClientId == userId && c.Id == bookingId)
                                                      join bcCm in _context.BookedConsultationsContactMethods.ReadOnlyFind(c => c.BookedConsultationId == bookingId).ToList() // .ToList() must be here or DataReaderException
                                                      on bc.Id equals bcCm.BookedConsultationId
                                                      where bc.ContactMethodId == bcCm.ContactMethodId
                                                      select new BookedConsultationInviteLinkViewModel
                                                      {
                                                          InviteLink = bcCm.InviteLink,
                                                          ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, _userManager.FindByTherapistAccountIdAsync(bc.TherapistId).Result.Id),
                                                          BookingId = bc.Id,
                                                          TherapistId = bc.TherapistId,
                                                          TherapistFirstName = bc.TherapistFirstName,
                                                          TherapistLastName = bc.TherapistLastName,
                                                          TherapistEmail = bc.TherapistEmail,
                                                          TherapistPhoneNumber = bc.TherapistPhoneNumber,
                                                          ClientFirstName = bc.ClientFirstName,
                                                          ClientLastName = bc.ClientLastName,
                                                          ClientEmail = bc.ClientEmail,
                                                          ClientPhoneNumber = bc.ClientPhoneNumber,
                                                          StartDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bc.StartDateTime),
                                                          EndDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bc.EndDateTime),
                                                          BookingDate = _dateHelper.ConvertDateTimeFromUtcToLocal(bc.BookingDate),
                                                          ContactMethod = new DataTransferObjects.ViewModels.Shared.ContactMethodViewModel
                                                          {
                                                              Id = bc.ContactMethodId,
                                                              Name = bc.ContactMethodName,
                                                              Color = bc.ContactMethodColor,
                                                              Icon = bc.ContactMethodIcon
                                                          }
                                                      }).SingleOrDefault();

                if(bookedConsultationInviteLinkVm == default)
                {
                    var bookedConsultation = _context.BookedConsultations.GetById(bookingId);

                    if (bookedConsultation == null || bookedConsultation.ClientId != userId) return default;

                    bookedConsultationInviteLinkVm = new BookedConsultationInviteLinkViewModel
                    {
                        InviteLink = null,
                        ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, (await _userManager.FindByTherapistAccountIdAsync(bookedConsultation.TherapistId)).Id),
                        BookingId = bookedConsultation.Id,
                        TherapistId = bookedConsultation.TherapistId,
                        TherapistFirstName = bookedConsultation.TherapistFirstName,
                        TherapistLastName = bookedConsultation.TherapistLastName,
                        TherapistEmail = bookedConsultation.TherapistEmail,
                        TherapistPhoneNumber = bookedConsultation.TherapistPhoneNumber,
                        ClientFirstName = bookedConsultation.ClientFirstName,
                        ClientLastName = bookedConsultation.ClientLastName,
                        ClientEmail = bookedConsultation.ClientEmail,
                        ClientPhoneNumber = bookedConsultation.ClientPhoneNumber,
                        StartDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.StartDateTime),
                        EndDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.EndDateTime),
                        BookingDate = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedConsultation.BookingDate),
                        ContactMethod = new DataTransferObjects.ViewModels.Shared.ContactMethodViewModel
                        {
                            Id = bookedConsultation.ContactMethodId,
                            Name = bookedConsultation.ContactMethodName,
                            Color = bookedConsultation.ContactMethodColor,
                            Icon = bookedConsultation.ContactMethodIcon
                        }
                    };
                }

                // If PhoneNumber is deleted in meantime in BookedConsultations, we start
                // taking the PhoneNumber info from AspNetUsers for that client.
                if (string.IsNullOrWhiteSpace(bookedConsultationInviteLinkVm.ClientPhoneNumber))
                    bookedConsultationInviteLinkVm.ClientPhoneNumber = (await _userManager.FindByIdAsync(userId)).PhoneNumber;

                // If the ContactMethod data are changed in the meantime in table ContactMethods,
                // here we update them in BookedConsultationInviteLink, but if that ContactMethod is deleted,
                // then we leave the ContactMethod data from BookedConsultations
                var contactMethod = _context.ContactMethods.GetById(bookedConsultationInviteLinkVm.ContactMethod.Id);
                if (contactMethod != null)
                {
                    bookedConsultationInviteLinkVm.ContactMethod.Name = contactMethod.Name;
                    bookedConsultationInviteLinkVm.ContactMethod.Color = contactMethod.Color;
                    bookedConsultationInviteLinkVm.ContactMethod.Icon = contactMethod.Icon;
                }

                return bookedConsultationInviteLinkVm;
            }
            catch (Exception e)
            {
                await _systemErrors.SaveErrorAsync(e, "WebApplication9", "ConsultationsFunctionsProvider", "GetBookedConsultationDetailsForClientAsync");
                return default;
            }
        }
    }
}
