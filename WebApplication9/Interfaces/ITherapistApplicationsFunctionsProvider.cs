using Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.ViewModels;

namespace WebApplication9.Interfaces
{
    public interface ITherapistApplicationsFunctionsProvider
    {
        Task<bool> InsertPsychotherapyTechniquesForTherapistFromApplicationAsync(string therapistId, string applicationId);
        Task<bool> InsertSpecialtiesForTherapistFromApplicationAsync(string therapistId, string applicationId);
        TherapistApplications GetApplication(string userId);
        List<ApplicationsViewModel> GetApplications(string predicate);
        List<Specialities> GetTherapistApplicationSpecialties(string userId);
        List<PsychotherapyTechniques> GetTherapistApplicationPsychotherapyTechniques(string userId);
        string GetApplicationPhotoPath(string userId, string applicationId);
        Task<bool> AcceptApplicationAsync(string userId);
        Task<bool> RejectApplicationAsync(string userId);
    }
}