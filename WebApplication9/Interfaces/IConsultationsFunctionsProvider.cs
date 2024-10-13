using Database.Models;
using DataTransferObjects.ViewModels.Client;
using DataTransferObjects.ViewModels.Therapist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.ViewModels;

namespace WebApplication9.Interfaces
{
    interface IConsultationsFunctionsProvider
    {
        bool UserHasConsultationToAttendDuringPeriod(string userId, DateTime periodStart, DateTime periodEnd);
        bool EmailHasConsultationToAttendDuringPeriod(string email, DateTime periodStart, DateTime periodEnd);
        bool TherapistHasConsultationDuringPeriod(string therapistId, DateTime periodStart, DateTime periodEnd);
        TherapistConsultationsViewModel GetTherapistConsultationsGroupedByDate(string therapistId);
        bool UserAlreadyBookedConsultationWithTherapist(string userId, string therapistId);
        List<BookedConsultations> GetBookedConsultations(string filter, string predicate);
        List<BookedConsultationViewModel> GetTherapistBookedConsultations(string therapistId, string filter, string predicate);
        Task<BookedConsultationAddInviteLinkViewModel> GetBookedConsultationDetailsForTherapistAsync(string therapistId, string bookingId);
        Task<BookedConsultationInviteLinkViewModel> GetBookedConsultationDetailsForClientAsync(string userId, string bookingId);
    }
}
