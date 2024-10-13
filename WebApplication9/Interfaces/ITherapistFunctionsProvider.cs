using Database.Models;
using DataTransferObjects.ViewModels.Client;
using DataTransferObjects.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;

namespace WebApplication9.Interfaces
{
    public enum TherapistAccountCompletionStatus
    {
        Completed,
        NotSetUpYet,
        Error
    }

    public interface ITherapistFunctionsProvider
    {
        Task<TherapistAccountCompletionStatus> TherapistHasCompletedAccountSetupAsync();
        bool HasCompletedTherapistAccountSetup(Therapists therapist);
        Task<bool> CreateTherapistAsync(Therapists therapist, string userIdToAttachTherapistTo);
        List<Specialities> GetSpecialties(string therapistId);
        List<PsychotherapyTechniques> GetPsychotherapyTechniques(string therapistId);
        List<string> GetSpecialtyNames(string therapistId);
        List<string> GetPsychotherapyTechniqueNames(string therapistId);
        bool HasContactMethods(string therapistId);
        bool HasContactMethod(string therapistId, string contactMethodId);
        List<ContactMethods> GetContactMethods(string therapistId);
        List<ContactMethodViewModel> GetContactMethodsViewModel(string therapistId);
        List<Consultations> GetUpcomingConsultations(string therapistId, bool orderByStartDateTime);
        List<Sessions> GetUpcomingSessions(string therapistId, bool orderByStartDateTime);
        List<ConsultationViewModel> GetUpcomingConsultationsViewModels(string therapistId);
        List<SessionViewModel> GetUpcomingSessionsViewModels(string therapistId);
        Task<bool> HasAnyUpcomingSessionAsync(string therapistId);
        Task<bool> HasAnyUpcomingConsultationAsync(string therapistId);
        double GetTherapistAverageRating(string therapistId);
        List<PartialViewModels.ClientReviewPartialViewModel> GetClientReviews(string therapistId);
        int GetNumberOfBookedSessions(string therapistId);
        int GetNumberOfUniqueClients(string therapistId);
        List<Therapists> GetTherapists(string filter, string predicate);
        List<Therapists> GetTherapistsWithSetUpAccount(string filter = null, string predicate = null);
        Therapists GetTherapistWithSetUpAccount(string therapisId);
        Task DeleteTherapistContactMethods(string therapistId);
        List<PsychotherapyTechniqueExperienceViewModel> GetPsychotherapyTechniquesExperiences(string therapistId);
        List<PsychotherapyTechniqueExperienceProfileViewModel> GetPsychotherapyTechniquesExperiencesForProfile(string therapistId);
        List<SpecialtyExperienceViewModel> GetSpecialtiesExperiences(string therapistId);
        List<SpecialtyExperienceProfileViewModel> GetSpecialtiesExperiencesForProfile(string therapistId);
        List<SelectListItem> Get_ToChooseFrom_ContactMethods(string therapistId);
        List<SelectListItem> Get_ToChooseFrom_Topics();
        GenderViewModel GetGender(string therapistId);
        List<TherapistShowcaseViewModel> GetTherapistsWithUpcomingConsultations();

        // DO NOT CHANGE THIS METHOD, ENTIRE SESSION BOOKING PAYMENT DEPENDS ON THIS METHOD
        string GetPriceRangeString(string therapistId);

        string GetPsychotherapyTechniquesNamesForDisplay(string therapistId, string delimiter = " · ");
        string GetSpecialtiesNamesForDisplay(string therapistId, string delimiter = " · ");
    }
}
