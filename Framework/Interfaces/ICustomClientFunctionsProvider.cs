using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    public enum EmailConfirmationStatus
    {
        Confirmed,
        NotConfirmed,
        Error
    }

    public interface ICustomClientFunctionsProvider
    {
        void UpdateUserAccountInformation(string userId, bool isSigningIn);
        List<SelectListItem> Get_ToChooseFrom_Topics();
        bool HasUpcomingConsultation(string userId);
        bool HasUpcomingSession(string userId);
        bool HasUnreviewedBookedSession(string userId);
        List<string> GetUnreviewedBookedSessionsIds(string userId);
        Task<EmailConfirmationStatus> HasConfirmedEmailAsync();
    }
}
