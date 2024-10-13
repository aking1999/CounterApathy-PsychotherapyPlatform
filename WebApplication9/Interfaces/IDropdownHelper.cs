using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Interfaces
{
    public interface IDropdownHelper
    {
        bool GenderExists(string gender);
        List<SelectListItem> GetGendersForDropdown();
        bool SessionTypeExists(int type);
        bool ContactMethodExistsInDatabase(string contactMethodId);
        bool CustomerSupportTicketTopicExistsInDatabase(string topicId);
        bool TherapistSupportTicketTopicExistsInDatabase(string topicId);
        List<SelectListItem> GetContactMethodsForDropdown(string therapistId);
        bool PsychotherapyTechniqueExistsInDatabase(string psychotherapyTechniqueId);
        bool SpecialtyExistsInDatabase(string specialtyId);
        List<SelectListItem> GetPsychotherapyTechniquesForDropdown();
        List<SelectListItem> GetSpecialtiesForDropdown();
    }
}
