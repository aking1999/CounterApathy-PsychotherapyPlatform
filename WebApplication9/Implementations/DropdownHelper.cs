using Database.Models;
using Database.RepositoryImplementations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using WebApplication9.Interfaces;

namespace WebApplication9.Implementations
{
    public class DropdownHelper : IDropdownHelper
    {
        private readonly ITherapistFunctionsProvider _therapistFunctions;
        //private readonly IContactMethodsFunctionsProvider _contactMethodFunctions;
        private readonly UnitOfWork _context;

        public DropdownHelper()
        {
            _therapistFunctions = new TherapistFunctionsProvider();
            //_contactMethodFunctions = new ContactMethodsFunctionsProvider();
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public bool GenderExists(string gender)
        {
            return gender.ToLower() == "male" || gender.ToLower() == "female";
        }

        public List<SelectListItem> GetGendersForDropdown()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "far fa-male|Muško|text-primary",
                    Value = "Male"
                },
                new SelectListItem
                {
                    Text = "far fa-female|Žensko|text-pink",
                    Value = "Female"
                }
            };
        }

        public bool SessionTypeExists(int type)
        {
            return (type == 0 || type == 1);
        }

        public bool ContactMethodExistsInDatabase(string contactMethodId)
        {
            //return _contactMethodFunctions.ContactMethodExistsInDatabase(contactMethodId);
            return _context.ContactMethods.GetById(contactMethodId) != null;
        }

        public bool CustomerSupportTicketTopicExistsInDatabase(string topicId)
        {
            return _context.ClientSupportTicketTopics.GetById(topicId) != null;
        }

        public bool TherapistSupportTicketTopicExistsInDatabase(string topicId)
        {
            return _context.TherapistSupportTicketTopics.GetById(topicId) != null;
        }

        public List<SelectListItem> GetContactMethodsForDropdown(string therapistId)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return new List<SelectListItem>();

            var dropdownMethods = new List<SelectListItem>();

            foreach(var method in _context.ContactMethods.GetAll().ToList())
            {
                dropdownMethods.Add(new SelectListItem
                {
                    Text = method.Icon + "|" + method.Name + "|" + method.Color,
                    Value = method.Id,
                    Selected = _therapistFunctions.HasContactMethod(therapistId, method.Id)
                });
            }

            return dropdownMethods;
        }

        public bool PsychotherapyTechniqueExistsInDatabase(string psychotherapyTechniqueId)
        {
            return _context.PsychotherapyTechniques.GetById(psychotherapyTechniqueId) != null;
        }

        public bool SpecialtyExistsInDatabase(string specialtyId)
        {
            return _context.Specialities.GetById(specialtyId) != null;
        }

        public List<SelectListItem> GetPsychotherapyTechniquesForDropdown()
        {
            var dropdown = new List<SelectListItem>();

            foreach (var technique in _context.PsychotherapyTechniques.GetAll().ToList())
            {
                dropdown.Add(new SelectListItem
                {
                    Text = technique.Icon + "|" + technique.Name + "|" + technique.Color,
                    Value = technique.Id
                });
            }

            return dropdown.OrderBy(x => x.Text).ToList();
        }

        public List<SelectListItem> GetSpecialtiesForDropdown()
        {
            var dropdown = new List<SelectListItem>();

            foreach (var specialty in _context.Specialities.GetAll().ToList())
            {
                dropdown.Add(new SelectListItem
                {
                    Text = specialty.Icon + "|" + specialty.Name + "|" + specialty.Color,
                    Value = specialty.Id
                });
            }

            return dropdown.OrderBy(x => x.Text).ToList();
        }
    }
}
